using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using PaymentsAPI.Entities;
using PaymentsAPI.Errors;
using PaymentsAPI.Services;

namespace PaymentsAPI.Controllers.Payments
{
    [Route("api")]
    [ApiController]
    public class PaymentController : Controller
    {
        const string CARD_HOLDER_REGEX = @"^([A-Za-z]{2,}\s[A-Za-z]+){1,3}$";
        private Regex cardHolderFormat = new Regex(CARD_HOLDER_REGEX);
        const string PAN_REGEX = @"^(\d{4}\-){3}\d{4}$";
        private Regex panFormat = new Regex(PAN_REGEX);
        const string CVV_REGEX = @"^[\d]{3}$";
        private Regex cvvFormat = new Regex(CVV_REGEX);
        const string CURRENCY_CODE_REGEX = @"^[A-Za-z]{3}$";
        private Regex currencyCodeFormat = new Regex(CURRENCY_CODE_REGEX);
        private readonly IToken tokenService;
        private readonly ICurrencyValidator currencyValidatorService;
        private readonly IMerchant merchantService;
        private readonly IPayment payments;

        public PaymentController(IToken _tokenService, ICurrencyValidator _currencyValidatorService, IMerchant _merchantService, IPayment _payments)
        {
            tokenService = _tokenService;
            currencyValidatorService = _currencyValidatorService;
            merchantService = _merchantService;
            payments = _payments;
        }

        [HttpPost("pay")]
        public async Task<ActionResult<string>> Pay(PaymentsPayload request)
        {
            var paymentRef = Guid.NewGuid().ToString();

            string username = null;
            try
            {
                username = tokenService.verifyToken(HttpContext.Request.Headers["Authorization"]);

                if (username == null)
                {
                    return BadRequest("Failed to authenticate");
                }
            }
            catch (Exception e)
            {
                // for cases where token decode fails
                return BadRequest("Failed to authenticate");
            }

            Merchant merchant = merchantService.getMerchantByUsername(username);

            if (merchant == null)
            {
                // cannot use Forbid - relies in auth handler and results in internal server error
                return StatusCode(403, "Merchant is not authorized to issue payment");
            }

            try
            {
                if (!cardHolderFormat.IsMatch(request.cardHolder))
                {
                    return BadRequest("Invalid card holder format");
                }

                if (!panFormat.IsMatch(request.pan))
                {
                    return BadRequest("Invalid card number format");
                }

                // TODO: improve validation using JSON schema or so...
                DateOnly expiryDate = DateOnly.Parse(request.expiryDate);
                if (expiryDate.CompareTo(DateOnly.FromDateTime(DateTime.Now)) < 0)
                {
                    return BadRequest("Card has expired");
                }

                if (!cvvFormat.IsMatch(request.cvv))
                {
                    return BadRequest("Invalid CVV format");
                }

                if (request.amount <= 0 || request.amount > 500)
                {
                    return BadRequest("Invalid amount. Only payments up to 500 are allowed");
                }

                if (!currencyCodeFormat.IsMatch(request.currencyCode))
                {
                    return BadRequest("Invalid currency code format");
                }

                if (!currencyValidatorService.isCurrencySupported(request.currencyCode))
                {
                    return BadRequest("Currency code is not supported");
                }

                string refUuid = payments.pay(merchant, paymentRef, request.cardHolder, request.pan.Replace("-", ""), expiryDate, request.cvv, request.amount, request.currencyCode);

                return Ok(refUuid);
            }
            // validation of input
            catch (FormatException e)
            {
                return BadRequest("Invalid expiry date");
            }
            // validation of business logic
            catch (PaymentException e)
            {
                if (e.code == PaymentExceptionCode.NOT_AUTHORIZED_BY_BANK)
                {
                    return BadRequest($"Payment rejected due to: {e.Message} - reference {e.paymentRef}");
                }

                if (e.code == PaymentExceptionCode.BANK_PAYMENT_PROCESSING)
                {
                    return StatusCode(500, $"Payment error: {e.Message}");
                }
                if (e.code == PaymentExceptionCode.ERROR_SAVING_PAYMENT)
                {
                    Console.WriteLine($"{e.Message} - payment was {(e.isAccepted ? "accepted" : "rejected")} by bank");
                    // gracefully handle processed payments by bank but not saved into DB
                    if (e.isAccepted)
                    {
                        return Ok($"{e.paymentRef} - your payment was accepted but it's still not available for status check");
                    }
                    else
                    {
                        return BadRequest($"Payment rejected due to: {e.Message} - reference {e.paymentRef} (payment not yet available for status check)");
                    }
                }

                return BadRequest($"Payment rejected due to: {e.Message}");
            }
            catch (Exception e)
            {
                return StatusCode(500, "Unkown error while processing payment");
            }
        }

        [HttpGet("payment/{paymentRef}")]
        public async Task<ActionResult<string>> GetPayment(string paymentRef)
        {
            try
            {
                bool isValid = Guid.TryParse(paymentRef, out var _);
                if (!isValid)
                {
                    return BadRequest("Invalid payment reference format");
                }
            }
            catch (FormatException)
            {
                return BadRequest("Invalid payment reference format");
            }

            string username = null;
            try
            {
                username = tokenService.verifyToken(HttpContext.Request.Headers["Authorization"]);

                if (username == null)
                {
                    return BadRequest("Failed to authenticate");
                }
            }
            catch (Exception e)
            {
                // for cases where token decode fails
                return BadRequest("Failed to authenticate");
            }

            try
            {
                Merchant merchant = merchantService.getMerchantByUsername(username);

                if (merchant == null)
                {
                    // cannot use Forbid - relies in auth handler and results in internal server error
                    return StatusCode(403, "Merchant is not authorized to get payment");
                }

                return Json(new PaymentViewModel(payments.getPaymentByRef(paymentRef, merchant)));
            }
            catch (PaymentException e)
            {
                Console.WriteLine(e.Message);
                // respond with a 404 regardless of what happened - payments are sensitive data so we don't want to heighten info awareness for possible attacker
                return NotFound($"Payment not found");
            }
            catch (Exception e)
            {
                return StatusCode(500, "Unkown error while processing payment retrieval");
            }
        }
    }
}
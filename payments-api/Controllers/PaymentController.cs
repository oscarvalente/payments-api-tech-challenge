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
        const string CCV_REGEX = @"^[\d]{3}$";
        private Regex ccvFormat = new Regex(CCV_REGEX);
        const string PAN_REGEX = @"^(\d{4}\-){3}\d{4}$";
        private Regex panFormat = new Regex(PAN_REGEX);
        private readonly IToken tokenService;
        private readonly IMerchant merchantService;
        private readonly IPayment payments;

        public PaymentController(IToken _tokenService, IMerchant _merchantService, IPayment _payments)
        {
            tokenService = _tokenService;
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

            Entities.Merchant merchant = merchantService.getMerchantByUsername(username);

            if (merchant == null)
            {
                // cannot use Forbid - relies in auth handler and results in internal server error
                return StatusCode(403, "Merchant is not authorized to issue payment");
            }

            try
            {
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

                if (!ccvFormat.IsMatch(request.ccv))
                {
                    return BadRequest("Invalid CCV format");
                }

                if (request.amount <= 0 || request.amount > 500)
                {
                    return BadRequest("Invalid amount. Only payments up to 500 are allowed"); // check currency too
                }

                payments.pay(merchant, paymentRef, request.pan.Replace("-", ""), expiryDate, request.ccv, request.amount);

                return NoContent();
            }
            // validation of input
            catch (FormatException e)
            {
                return BadRequest("Invalid expiry date");
            }
            // validation of business logic
            catch (Exception e)
            {
                if (e is PaymentException)
                {
                    return BadRequest($"Payment rejected due to: {e.Message}");
                }
                return StatusCode(500, "Unkown error while processing payment");
            }
        }

        [HttpGet("pay/{paymentRef}")]
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
                Entities.Merchant merchant = merchantService.getMerchantByUsername(username);

                if (merchant == null)
                {
                    return BadRequest("Merchant does not exist");
                }

                return Json(payments.getPaymentByRef(paymentRef, merchant));
            }
            catch (Exception e)
            {
                if (e is PaymentException)
                {
                    return BadRequest($"Payment retrieval rejected due to: {e.Message}");
                }
                return StatusCode(500, "Unkown error while processing payment retrieval");
            }
        }
    }
}
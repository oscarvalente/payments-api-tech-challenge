using MediatR;
using PaymentsAPI.Errors;
using PaymentsAPI.Payments.DTO;
using PaymentsAPI.Payments.Services;
using PaymentsAPI.Utils;

namespace PaymentsAPI.Payments.Handlers
{
    public class CreatePaymentHandler : IRequestHandler<CreatePaymentCommand, PaymentDTO>
    {
        private readonly ICurrencyValidator currencyValidatorService;
        private readonly IPayment payments;
        private readonly IAPIResponseBuilder apiResponseBuilder;
        private readonly IRequesterMerchant requesterMerchant;

        public CreatePaymentHandler(ICurrencyValidator _currencyValidatorService, IPayment _payments, IRequesterMerchant _requesterMerchant)
        {
            currencyValidatorService = _currencyValidatorService;
            payments = _payments;
            requesterMerchant = _requesterMerchant;
        }

        public async Task<PaymentDTO> Handle(CreatePaymentCommand ctxRequest, CancellationToken cancellationToken)
        {

            if (!currencyValidatorService.isCurrencySupported(ctxRequest.CurrencyCode))
            {
                throw new PaymentException("Currency code is not supported", PaymentExceptionCode.CURRENCY_NOT_SUPPORTED);
            }
            string paymentRef = payments.pay(requesterMerchant.merchant, ctxRequest.CardHolder, ctxRequest.Pan.Replace("-", ""), DateOnly.Parse(ctxRequest.ExpiryDate), ctxRequest.Cvv, ctxRequest.Amount, ctxRequest.CurrencyCode);
            return new PaymentDTO
            {
                RefUUID = paymentRef
            };
        }
    }
}

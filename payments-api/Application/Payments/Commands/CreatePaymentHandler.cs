using MediatR;
using PaymentsAPI.Entities;
using PaymentsAPI.Errors;
using PaymentsAPI.Services.Responses;
using PaymentsGatewayApi.WebApi.Services;

namespace PaymentsAPI.Services
{
    public class CreatePaymentHandler : IRequestHandler<CreatePaymentCommand, string>
    {
        private readonly ICurrencyValidator currencyValidatorService;
        private readonly IPayment payments;
        private readonly IAPIResponseBuilder apiResponseBuilder;
        private readonly IRequesterMerchant requesterMerchant;

        public CreatePaymentHandler(ICurrencyValidator _currencyValidatorService, IPayment _payments, IAPIResponseBuilder _apiResponseBuilder, IRequesterMerchant _requesterMerchant)
        {
            currencyValidatorService = _currencyValidatorService;
            payments = _payments;
            apiResponseBuilder = _apiResponseBuilder;
            requesterMerchant = _requesterMerchant;
        }

        public async Task<string> Handle(CreatePaymentCommand ctxRequest, CancellationToken cancellationToken)
        {
            var paymentRef = Guid.NewGuid().ToString();

            if (!currencyValidatorService.isCurrencySupported(ctxRequest.CurrencyCode))
            {
                throw new PaymentException("Currency code is not supported", PaymentExceptionCode.CURRENCY_NOT_SUPPORTED);
            }
            return payments.pay(requesterMerchant.merchant, paymentRef, ctxRequest.CardHolder, ctxRequest.Pan.Replace("-", ""), DateOnly.Parse(ctxRequest.ExpiryDate), ctxRequest.Cvv, ctxRequest.Amount, ctxRequest.CurrencyCode);

        }
    }
}

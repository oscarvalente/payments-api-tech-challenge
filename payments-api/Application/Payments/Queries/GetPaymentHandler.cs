using MediatR;
using PaymentsAPI.Errors;
using PaymentsAPI.Services.Responses;

namespace PaymentsAPI.Services
{
    public class GetPaymentHandler : IRequestHandler<GetPaymentQuery, PaymentViewModel>
    {
        private readonly IPayment payments;
        private readonly IAPIResponseBuilder apiResponseBuilder;
        private readonly IRequesterMerchant requesterMerchant;

        public GetPaymentHandler(ICurrencyValidator _currencyValidatorService, IPayment _payments, IAPIResponseBuilder _apiResponseBuilder, IRequesterMerchant _requesterMerchant)
        {
            payments = _payments;
            apiResponseBuilder = _apiResponseBuilder;
            requesterMerchant = _requesterMerchant;
        }

        public async Task<PaymentViewModel> Handle(GetPaymentQuery ctxRequest, CancellationToken cancellationToken)
        {
            return new PaymentViewModel(payments.getPaymentByRef(ctxRequest.RefUUID, requesterMerchant.merchant));
        }
    }
}
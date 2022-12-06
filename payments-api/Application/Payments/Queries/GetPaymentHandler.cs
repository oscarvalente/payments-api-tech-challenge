using MediatR;
using PaymentsAPI.Errors;
using PaymentsAPI.Payments;
using PaymentsAPI.Payments.Handlers;
using PaymentsAPI.Payments.Services;
using PaymentsAPI.Services;
using PaymentsAPI.Utils;

namespace PaymentsAPI.Payments.Handler
{
    public class GetPaymentHandler : IRequestHandler<GetPaymentQuery, PaymentViewModel>
    {
        private readonly IPayment payments;
        private readonly IRequesterMerchant requesterMerchant;

        public GetPaymentHandler(ICurrencyValidator _currencyValidatorService, IPayment _payments, IRequesterMerchant _requesterMerchant)
        {
            payments = _payments;
            requesterMerchant = _requesterMerchant;
        }

        public async Task<PaymentViewModel> Handle(GetPaymentQuery ctxRequest, CancellationToken cancellationToken)
        {
            return new PaymentViewModel(payments.getPaymentByRef(ctxRequest.RefUUID, requesterMerchant.merchant));
        }
    }
}
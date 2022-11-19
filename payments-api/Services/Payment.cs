using PaymentsAPI.Entities;
using PaymentsAPI.Errors;
using PaymentsAPI.Utils;

namespace PaymentsAPI.Services
{
    public class Payments : IPayment
    {
        private readonly IPaymentData paymentData;
        public Payments(IPaymentData _paymentData)
        {
            paymentData = _paymentData;
        }
        public void pay(Entities.Merchant merchant, string paymentRef, string pan, DateOnly cardExpiryDate, string ccv, decimal amount)
        {
            // send payment to acquiring bank - just a dummy mock
            if (!DummyAcquiringBank.isValidPayment(pan))
            {
                throw new PaymentException("Invalid payment", PaymentExceptionCode.NOT_AUTHORIZED_BY_BANK);
            }
            // if ok save payment to DB
            Payment payment = new Payment
            {
                RefUuid = paymentRef,
                Amount = amount,
                Pan = pan,
                ExpiryDate = cardExpiryDate,
                Ccv = ccv
            };
            paymentData.createPayment(payment, merchant);
        }

        public PaymentViewModel getPaymentByRef(string paymentRef, Entities.Merchant merchant)
        {
            Payment payment = paymentData.getPaymentByRefUUID(paymentRef);

            if (payment == null)
            {
                throw new PaymentException("Payment retrieval not authorized", PaymentExceptionCode.PAYMENT_NOT_FOUND);
            }

            if (payment.MerchantId != merchant.Id)
            {
                throw new PaymentException("Payment retrieval not authorized", PaymentExceptionCode.PAYMENT_RETRIEVAL_NOT_AUTHORIZED);
            }

            return new PaymentViewModel(payment);
        }
    }
}
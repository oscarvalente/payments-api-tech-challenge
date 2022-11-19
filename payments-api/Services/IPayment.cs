using PaymentsAPI.Entities;

namespace PaymentsAPI.Services
{
    public interface IPayment
    {
        public void pay(Entities.Merchant merchant, string paymentRef, string pan, DateOnly cardExpiryDate, string ccv, decimal amount);
        public PaymentViewModel getPaymentByRef(string paymentRef, Entities.Merchant merchant);
    }
}
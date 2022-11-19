using PaymentsAPI.Entities;

namespace PaymentsAPI.Services
{
    public interface IPayment
    {
        public string pay(Merchant merchant, string paymentRef, string cardHolder, string pan, DateOnly cardExpiryDate, string cvv, decimal amount, string currencyCode);
        public PaymentViewModel getPaymentByRef(string paymentRef, Entities.Merchant merchant);
    }
}
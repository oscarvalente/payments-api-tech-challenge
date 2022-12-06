using PaymentsAPI.Entities;

namespace PaymentsAPI.Payments.Services
{
    public interface IPayment
    {
        public string pay(Merchant merchant, string cardHolder, string pan, DateOnly cardExpiryDate, string cvv, decimal amount, string currencyCode);
        public Payment getPaymentByRef(string paymentRef, Merchant merchant);
    }
}
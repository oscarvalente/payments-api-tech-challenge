using PaymentsAPI.Entities;

public interface IPaymentData
{
    public Payment getPaymentByRefUUID(string uuid);
    public Payment addPayment(string paymentRef, decimal amount, string currencyCode, string cardHolder, string pan, DateOnly cardExpiryDate, string swiftCode, bool isAccepted, Merchant merchant);
}
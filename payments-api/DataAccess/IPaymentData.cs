using PaymentsAPI.Entities;

public interface IPaymentData
{
    public Payment getPaymentByRefUUID(string uuid);
    public Payment createPayment(Payment payment, Merchant merchant);
}
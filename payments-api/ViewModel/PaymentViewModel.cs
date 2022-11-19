using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore.Storage;
using PaymentsAPI.Entities;

public class PaymentViewModel
{

    public string Pan { get; set; }
    public string CardExpiryDate { get; set; }
    public string CCV { get; set; }
    public decimal Amount { get; set; }

    public PaymentViewModel(Payment payment)
    {
        Pan = $"****-****-****-{payment.Pan.Substring(12, 4)}";
        CardExpiryDate = payment.ExpiryDate.ToLongDateString();
        CCV = payment.Ccv;
        Amount = payment.Amount;
    }
}
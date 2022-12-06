using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore.Storage;
using PaymentsAPI.Entities;

public class PaymentViewModel
{

    public string Pan { get; set; }
    public string CardExpiryDate { get; set; }
    public string CreatedAt { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public bool IsAccepted { get; set; }

    public PaymentViewModel() // empty constructor for JSON serialization
    {
    }

    public PaymentViewModel(Payment payment)
    {
        Pan = $"****-****-****-{payment.Pan.Substring(12, 4)}";
        CardExpiryDate = payment.ExpiryDate.ToShortDateString();
        CreatedAt = payment.CreatedAt.ToString();
        Amount = payment.Amount;
        CurrencyCode = payment.CurrencyCode;
        IsAccepted = payment.IsAccepted;
    }
}
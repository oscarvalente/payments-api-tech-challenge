namespace PaymentsAPI.Controllers.Payments
{
    public class PaymentModel
    {
        public string cardHolder { get; set; } = string.Empty;
        public string pan { get; set; } = string.Empty;
        public string expiryDate { get; set; } = string.Empty;
        public string cvv { get; set; } = string.Empty;
        public decimal amount { get; set; } = 0;
        public string currencyCode { get; set; } = string.Empty;
    }
}
namespace PaymentsAPI.Controllers.Payments
{
    public class PaymentsPayload
    {
        public string pan { get; set; } = string.Empty;
        public string expiryDate { get; set; } = string.Empty;
        public string ccv { get; set; } = string.Empty;
        public decimal amount { get; set; } = 0;
    }
}
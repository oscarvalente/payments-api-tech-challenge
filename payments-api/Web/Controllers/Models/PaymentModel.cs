using System.ComponentModel.DataAnnotations;
using PaymentsAPI.Validations;

namespace PaymentsAPI.Controllers.Payments
{
    public class PaymentModel
    {
        [Required]
        [RegularExpression(@"^([A-Za-z]{2,}\s[A-Za-z]+){1,3}$", ErrorMessage = "Card holder must have a valid name")]
        public string cardHolder { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^(\d{4}\-){3}\d{4}$", ErrorMessage = "PAN must be in format xxxx-xxxx-xxxx-xxxx")]
        public string pan { get; set; } = string.Empty;
        [DateTodayOrFurther]
        public string expiryDate { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^[\d]{3}$", ErrorMessage = "CVV must be 3 digits")]
        public string cvv { get; set; } = string.Empty;
        [Required]
        [Range(1, 500, ErrorMessage = "Invalid amount. Only payments up to 500 are allowed")]
        public decimal amount { get; set; } = 0;
        [Required]
        [RegularExpression(@"^[A-Za-z]{3}$", ErrorMessage = "Currency code must 3 alphabetic characters")]
        public string currencyCode { get; set; } = string.Empty;
    }
}
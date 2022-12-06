using System.ComponentModel.DataAnnotations;
using MediatR;
using PaymentsAPI.Payments.DTO;
using PaymentsAPI.Validations;

namespace PaymentsAPI.Payments.Handlers
{
    public class CreatePaymentCommand : IRequest<PaymentDTO>
    {
        [Required]
        [Range(1, 500, ErrorMessage = "Invalid amount. Only payments up to 500 are allowed")]
        public decimal Amount { get; set; }
        [Required]
        [RegularExpression(@"^[A-Za-z]{3}$", ErrorMessage = "Currency code must 3 alphabetic characters")]
        public string CurrencyCode { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^([A-Za-z]{2,}\s[A-Za-z]+){1,3}$", ErrorMessage = "Card holder must have a valid name")]
        public string CardHolder { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^(\d{4}\-){3}\d{4}$", ErrorMessage = "PAN must be in format xxxx-xxxx-xxxx-xxxx")]
        public string Pan { get; set; } = string.Empty;
        [DateTodayOrFurther]
        public string ExpiryDate { get; set; }
        [Required]
        [RegularExpression(@"^[\d]{3}$", ErrorMessage = "CVV must be 3 digits")]
        public string Cvv { get; set; }
    }
}
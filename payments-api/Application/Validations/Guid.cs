using System.ComponentModel.DataAnnotations;

namespace PaymentsAPI.Validations
{

    public class GuidValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                bool isValid = Guid.TryParse((string)value, out var _);
                if (!isValid)
                {
                    return new ValidationResult(ErrorMessage ?? "Invalid payment reference format");
                }
                return ValidationResult.Success;
            }
            catch (FormatException)
            {
                return new ValidationResult(ErrorMessage ?? "Invalid payment reference format");
            }
        }
    }
}
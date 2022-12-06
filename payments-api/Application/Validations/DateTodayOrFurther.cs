using System.ComponentModel.DataAnnotations;

namespace PaymentsAPI.Validations
{
    public class DateTodayOrFurther : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateOnly expiryDate;
            DateOnly.TryParse((string)value, out expiryDate);
            if (expiryDate.CompareTo(DateOnly.FromDateTime(DateTime.Now)) >= 0)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? "Date must be at least today");
        }
    }
}
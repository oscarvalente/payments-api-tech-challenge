using System.ComponentModel.DataAnnotations;

public class DateTodayOrFurther : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        DateOnly expiryDate;
        DateOnly.TryParse(value?.ToString(), out expiryDate);
        if (expiryDate.CompareTo(DateOnly.FromDateTime(DateTime.Now)) >= 0)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage ?? "Date must be at least today");
    }

}
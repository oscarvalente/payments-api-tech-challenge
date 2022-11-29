using System.ComponentModel.DataAnnotations;

namespace PaymentsAPI.Controllers.Authentication
{
    public class AuthenticationModel
    {
        [Required]
        [RegularExpression(@"^[A-Za-z\d]{8,20}$", ErrorMessage = "Username must contain only alphanumeric characters with minimum of 8 to maximum of 20 length")]
        public string username { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{12,}", ErrorMessage = "Password must contain at least 12 characters, alphanumeric and special characters")]
        public string password { get; set; } = string.Empty;
    }
}
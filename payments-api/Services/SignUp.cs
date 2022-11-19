

using System.Text.RegularExpressions;
using PaymentsAPI.Entities;
using PaymentsAPI.Errors;

namespace PaymentsAPI.Services
{
    public class SignUp : ISignUp
    {
        const string USERNAME_REGEX = @"^[A-Za-z\d]{8,20}$";
        private Regex usernameFormat = new Regex(USERNAME_REGEX);
        const string PASSWORD_REGEX = @"(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{12,}";
        private Regex passwordFormat = new Regex(PASSWORD_REGEX);

        private readonly IMerchantData merchantData;
        public SignUp(IMerchantData _merchantData)
        {
            merchantData = _merchantData;
        }

        public Entities.Merchant signUpMerchant(string username, string password)
        {
            // check username format
            bool isUsernameValid = usernameFormat.IsMatch(username);
            if (!isUsernameValid)
            {
                throw new SignUpException("Invalid username format", SignUpExceptionCode.INVALID_USERNAME);
            }

            var existingCustomer = merchantData.getMerchantByUsername(username);
            if (existingCustomer != null)
            {
                throw new SignUpException("User already exists", SignUpExceptionCode.USER_ALREADY_EXISTS);
            }

            Entities.Merchant merchant = new Entities.Merchant();

            merchant.Username = username;

            // check password format
            bool isPasswordValid = passwordFormat.IsMatch(password);
            if (!isPasswordValid)
            {
                throw new SignUpException("Invalid password format", SignUpExceptionCode.INVALID_PASSWORD);
            }

            // TODO: factory to create entities

            Password.CreatePasswordHash(password, out string passwordHash, out string passwordSalt);
            merchant.PasswordHash = passwordHash;
            merchant.PasswordSalt = passwordSalt;
            merchant.Address = "default street";

            return merchantData.addMerchant(merchant);
        }
    }
}


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
        public SignUp(IBankMatcher _bankMatcher, IMerchantData _merchantData)
        {
            merchantData = _merchantData;
        }

        public Merchant signUpMerchant(string username, string password)
        {
            // check username format
            if (!usernameFormat.IsMatch(username))
            {
                throw new SignUpException("Invalid username format", SignUpExceptionCode.INVALID_USERNAME);
            }

            // check password format
            if (!passwordFormat.IsMatch(password))
            {
                throw new SignUpException("Invalid password format", SignUpExceptionCode.INVALID_PASSWORD);
            }

            var existingCustomer = merchantData.getMerchantByUsername(username);
            if (existingCustomer != null)
            {
                throw new SignUpException("User already exists", SignUpExceptionCode.USER_ALREADY_EXISTS);
            }

            Password.CreatePasswordHash(password, out string passwordHash, out string passwordSalt);

            // TODO: factory to create entities

            return merchantData.addMerchant(username, passwordSalt, passwordHash, "default street");
        }
    }
}


using System.Text.RegularExpressions;
using PaymentsAPI.Entities;
using PaymentsAPI.Errors;
using PaymentsAPI.Utils;

namespace PaymentsAPI.Services
{
    public class SignUp : ISignUp
    {
        private readonly IMerchantData merchantData;
        public SignUp(IBankMatcher _bankMatcher, IMerchantData _merchantData)
        {
            merchantData = _merchantData;
        }

        public Merchant signUpMerchant(string username, string password)
        {
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
using System.Runtime.Serialization.Formatters;
using PaymentsAPI.Errors;

namespace PaymentsAPI.Services
{
    public class SignIn : ISignIn
    {

        private readonly IMerchantData merchantData;
        public SignIn(IMerchantData _merchantData)
        {
            merchantData = _merchantData;
        }

        public bool signInMerchant(string username, string password)
        {
            var existingMerchant = merchantData.getMerchantByUsername(username);

            if (existingMerchant != null)
            {
                if (existingMerchant.IsVerified != true)
                {
                    throw new SignInException("Merchant is not verified", SignInExceptionCode.MERCHANT_NOT_VERIFIED);
                }

                return Password.VerifyPasswordHash(password, existingMerchant.PasswordHash, existingMerchant.PasswordSalt);
            }

            return false;
        }
    }
}
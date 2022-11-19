using PaymentsAPI.Entities;

namespace PaymentsAPI.Services
{
    public interface ISignUp
    {
        public Merchant signUpMerchant(string username, string password);
    }
}
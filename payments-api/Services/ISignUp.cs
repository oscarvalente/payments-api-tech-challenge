namespace PaymentsAPI.Services
{
    public interface ISignUp
    {
        public Entities.Merchant signUpMerchant(string username, string password);
    }
}
namespace PaymentsAPI.Services
{
    public interface ISignIn
    {
        public bool signInMerchant(string username, string password);
    }
}
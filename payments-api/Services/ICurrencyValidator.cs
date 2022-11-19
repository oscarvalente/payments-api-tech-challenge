
namespace PaymentsAPI.Services
{
    public interface ICurrencyValidator
    {
        public bool isCurrencySupported(string currencyCode);
    }
}
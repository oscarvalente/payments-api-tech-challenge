
namespace PaymentsAPI.Utils
{
    public interface ICurrencyValidator
    {
        public bool isCurrencySupported(string currencyCode);
    }
}
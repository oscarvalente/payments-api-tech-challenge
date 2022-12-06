
namespace PaymentsAPI.Utils
{
    public class CurrencyValidator : ICurrencyValidator
    {
        private readonly IConfiguration configuration;

        public CurrencyValidator(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        public bool isCurrencySupported(string currencyCode)
        {
            var supportedCurrencies = configuration.GetSection("ApplicationSettings:supportedCurrencyCodes").Get<string[]>();
            return supportedCurrencies.Contains(currencyCode.ToUpper());
        }
    }
}
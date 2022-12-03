using Microsoft.IdentityModel.Tokens;
using PaymentsAPI.Errors;
using PaymentsAPI.Services.Banks;
using PaymentsAPI.Utils;

namespace PaymentsAPI.Services
{
    public class BankMatcher : IBankMatcher
    {

        private readonly IConfiguration configuration;

        public BankMatcher(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        public bool isBankSupported(string pan)
        {
            Dictionary<string, string> supportedBankSwifts = configuration.GetSection("ApplicationSettings:supportedBankSwifts").Get<Dictionary<string, string>>();
            return supportedBankSwifts.ContainsKey(pan.Substring(0, 6));
        }

        public IDummyAcquiringBank loadBankByPAN(string pan)
        {
            string bankIdentNumber = pan.Substring(0, 6);
            // using Strategy pattern to determine which acquiring bank is responsible for handling this payment processing
            Dictionary<string, string> supportedBankSwifts = configuration.GetSection("ApplicationSettings:supportedBankSwifts").Get<Dictionary<string, string>>();

            if (!supportedBankSwifts.TryGetValue(bankIdentNumber, out var bankSwift))
            {
                return null;
            }
            switch (bankSwift)
            {
                case var value when bankSwift == "BANKXXXX":
                    return (IDummyAcquiringBank)new DummyAcquiringBankA();
                case var value when bankSwift == "BANKZZZZ":
                    return (IDummyAcquiringBank)new DummyAcquiringBankB();
                default:
                    return null;
            }
        }

    }
}
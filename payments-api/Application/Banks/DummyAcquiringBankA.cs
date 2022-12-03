using System.Text.RegularExpressions;

namespace PaymentsAPI.Services.Banks
{
    public class DummyAcquiringBankA : IDummyAcquiringBank
    {
        const string SWIFT_CODE = "BANKXXXX";
        public string getSwiftCode()
        {
            return SWIFT_CODE;
        }
        public DummyAcquiringBankA()
        {
        }
        public bool isValidPayment(string pan, string cardHolder, DateOnly cardExpiryDate, string cvv, decimal amount)
        {
            string CARD_ACCEPTANCE_REGEX = @"[0-4]$";
            Regex cardAcceptanceRegex = new Regex(CARD_ACCEPTANCE_REGEX);
            return cardAcceptanceRegex.IsMatch(pan);
        }
    }
}
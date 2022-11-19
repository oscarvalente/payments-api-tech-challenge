using System.Text.RegularExpressions;

namespace PaymentsAPI.Utils
{
    public class DummyAcquiringBank
    {
        public static bool isValidPayment(string pan)
        {
            string CARD_ACCEPTANCE_REGEX = @"[0-4]$";
            Regex cardAcceptanceRegex = new Regex(CARD_ACCEPTANCE_REGEX);
            return cardAcceptanceRegex.IsMatch(pan);
        }
    }
}
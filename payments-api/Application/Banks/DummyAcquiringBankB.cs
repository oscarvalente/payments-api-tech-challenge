using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

namespace PaymentsAPI.Services.Banks
{
    public class DummyAcquiringBankB : IDummyAcquiringBank
    {

        const string SWIFT_CODE = "BANKZZZZ";
        public string getSwiftCode()
        {
            return SWIFT_CODE;
        }

        public DummyAcquiringBankB()
        {
        }
        public bool isValidPayment(string pan, string cardHolder, DateOnly cardExpiryDate, string cvv, decimal amount)
        {
            return pan.EndsWith("00");
        }
    }
}
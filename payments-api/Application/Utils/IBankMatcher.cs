
using PaymentsAPI.Banks.Services;

namespace PaymentsAPI.Utils
{
    public interface IBankMatcher
    {
        public bool isBankSupported(string pan);
        public IDummyAcquiringBank loadBankByPAN(string pan);
    }
}

namespace PaymentsAPI.Services
{
    public interface IBankMatcher
    {
        public bool isBankSupported(string pan);
        public IDummyAcquiringBank loadBankByPAN(string pan);
    }
}
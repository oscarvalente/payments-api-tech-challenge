using PaymentsAPI.Entities;

namespace PaymentsAPI.Services
{
    public class Merchants : IMerchant
    {
        private readonly IMerchantData merchantData;
        public Merchants(IMerchantData _merchantData)
        {
            merchantData = _merchantData;
        }

        public Merchant getMerchantByUsername(string username)
        {
            return merchantData.getMerchantByUsername(username);
        }
    }
}
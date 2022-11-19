namespace PaymentsAPI.Services
{
    public class Merchant : IMerchant
    {
        private readonly IMerchantData merchantData;
        public Merchant(IMerchantData _merchantData)
        {
            merchantData = _merchantData;
        }

        public Entities.Merchant getMerchantByUsername(string username)
        {
            return merchantData.getMerchantByUsername(username);
        }
    }
}
using PaymentsAPI.Entities;
public interface IMerchantData
{
    public Merchant getMerchantByUsername(string username);
    public Merchant addMerchant(Merchant merchant);
}

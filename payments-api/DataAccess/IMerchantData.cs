using PaymentsAPI.Entities;
public interface IMerchantData
{
    public Merchant getMerchantByUsername(string username);
    public Merchant addMerchant(string username, string passwordSalt, string passwordHash, string address);
}

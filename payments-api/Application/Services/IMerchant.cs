using PaymentsAPI.Entities;

public interface IMerchant
{
    public Merchant getMerchantByUsername(string username);
}
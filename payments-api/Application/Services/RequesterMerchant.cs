namespace PaymentsGatewayApi.WebApi.Services;

using PaymentsAPI.Entities;

public class RequesterMerchant : IRequesterMerchant
{
    public RequesterMerchant()
    {
    }

    public Merchant merchant { get; set; }
}
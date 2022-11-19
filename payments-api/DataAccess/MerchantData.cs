using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.IdentityModel.Tokens;
using PaymentsAPI.EfStructures;
using PaymentsAPI.Entities;
// using PaymentsAPI.EfStructures;
// using PaymentsAPI.Entities;


namespace PaymentsAPI.DataAccess
{
    public class MerchantData : IMerchantData
    {
        private readonly PaymentsAPIDbContext context;
        public MerchantData(PaymentsAPIDbContext _context)
        {
            context = _context;
        }

        public Merchant getMerchantByUsername(string username)
        {
            IQueryable<Merchant> query = context.Merchants.AsQueryable();
            var foundMerchant = query.SingleOrDefault(c => c.Username == username);
            if (foundMerchant == null)
            {
                return null;
            }

            return (Merchant)foundMerchant;
        }
        public Merchant addMerchant(Merchant merchant)
        {
            context.Merchants.Add(merchant);
            context.SaveChanges();
            return merchant;
        }
    }
}
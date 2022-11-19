using PaymentsAPI.EfStructures;
using PaymentsAPI.Entities;


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
        public Merchant addMerchant(string username, string passwordSalt, string passwordHash, string address)
        {
            Merchant merchant = new Merchant
            {
                Username = username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Address = address
            };

            context.Merchants.Add(merchant);
            context.SaveChanges();
            return merchant;
        }
    }
}
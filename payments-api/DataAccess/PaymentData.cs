using PaymentsAPI.EfStructures;
using PaymentsAPI.Entities;

namespace PaymentsAPI.DataAccess
{
    public class PaymentData : IPaymentData
    {
        private readonly PaymentsAPIDbContext context;
        public PaymentData(PaymentsAPIDbContext _context)
        {
            context = _context;
        }

        public Payment getPaymentByRefUUID(string uuid)
        {
            return context.Payments.FirstOrDefault(payment => payment.RefUuid == uuid);
        }

        public Payment createPayment(Payment payment, Merchant merchant)
        {
            onPay(payment, merchant);
            return payment;
        }

        private void onPay(Payment payment, Merchant merchant)
        {
            // wrapping in transaction in case context is already polluted used
            using (var transaction = context.Database.BeginTransaction())
            {
                // merchant association only
                payment.Merchant = merchant;
                context.Payments.Add(payment);
                context.SaveChanges();
                transaction.Commit();
            }
        }
    }
}
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

        public Payment addPayment(string paymentRef, decimal amount, string currencyCode, string cardHolder, string pan, DateOnly cardExpiryDate, string swiftCode, bool isAccepted, Merchant merchant)
        {
            Payment payment = new Payment
            {
                RefUuid = paymentRef,
                Amount = amount,
                CurrencyCode = currencyCode.ToUpper(),
                CardHolder = cardHolder.ToUpper(),
                Pan = pan,
                ExpiryDate = cardExpiryDate,
                AcquiringBankSwift = swiftCode,
                IsAccepted = isAccepted,
                // PCI DSS compliance, we do not store CVV data
                Merchant = merchant
            };
            onPay(payment, merchant);
            return payment;
        }

        private void onPay(Payment payment, Merchant merchant)
        {
            // could wrap in transaction in case context is already polluted used
            // using (var transaction = context.Database.BeginTransaction())
            // {

            context.Payments.Add(payment);
            context.SaveChanges();
            // transaction.Commit();
            // }
        }
    }
}
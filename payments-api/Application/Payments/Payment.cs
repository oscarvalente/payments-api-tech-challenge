using PaymentsAPI.Entities;
using PaymentsAPI.Errors;
using PaymentsAPI.Utils;
using PaymentsAPI.Banks.Services;

namespace PaymentsAPI.Payments.Services
{
    public class Payments : IPayment
    {
        private readonly IBankMatcher bankMatcher;
        private readonly IPaymentData paymentData;
        public Payments(IBankMatcher _bankMatcher, IPaymentData _paymentData)
        {
            bankMatcher = _bankMatcher;
            paymentData = _paymentData;
        }
        public string pay(Merchant merchant, string cardHolder, string pan, DateOnly cardExpiryDate, string cvv, decimal amount, string currencyCode)
        {
            IDummyAcquiringBank dummyBank = bankMatcher.loadBankByPAN(pan);
            if (dummyBank == null)
            {
                throw new PaymentException($"Acquiring bank is not supported", PaymentExceptionCode.BANK_NOT_SUPPORTED);
            }

            Payment payment = null;
            var isValidPayment = false;
            // validate payment with acquiring bank - just a dummy mock
            try
            {
                isValidPayment = dummyBank.isValidPayment(pan, cardHolder, cardExpiryDate, cvv, amount);
            }
            catch (Exception e)
            {
                throw new PaymentException("Error communicating payment with acquiring bank", PaymentExceptionCode.BANK_PAYMENT_PROCESSING);
            }
            string paymentRef = Guid.NewGuid().ToString();
            if (!isValidPayment)
            {
                // if rejected we still save payment as rejected
                try
                {
                    payment = paymentData.addPayment(paymentRef, amount, currencyCode, cardHolder.ToUpper(), pan, cardExpiryDate, dummyBank.getSwiftCode(), false, merchant);
                }
                catch (Exception)
                {
                    throw new PaymentException("Error saving payment to database", PaymentExceptionCode.ERROR_SAVING_PAYMENT, paymentRef, false);
                }
            }
            else
            {
                // if accepted save payment as accepted
                try
                {
                    payment = paymentData.addPayment(paymentRef, amount, currencyCode, cardHolder.ToUpper(), pan, cardExpiryDate, dummyBank.getSwiftCode(), true, merchant);
                }
                catch (Exception)
                {
                    throw new PaymentException("Error saving payment to database", PaymentExceptionCode.ERROR_SAVING_PAYMENT, paymentRef, true);
                }
            }

            return payment.RefUuid;
        }

        public Payment getPaymentByRef(string paymentRef, Merchant merchant)
        {
            Payment payment = paymentData.getPaymentByRefUUID(paymentRef);

            if (payment == null)
            {
                throw new PaymentException("Payment retrieval not authorized", PaymentExceptionCode.PAYMENT_NOT_FOUND);
            }

            if (payment.MerchantId != merchant.Id)
            {
                throw new PaymentException("Payment retrieval not authorized", PaymentExceptionCode.PAYMENT_RETRIEVAL_NOT_AUTHORIZED);
            }

            return payment;
        }
    }
}
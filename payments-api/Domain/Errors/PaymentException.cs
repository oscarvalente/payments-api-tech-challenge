namespace PaymentsAPI.Errors
{

    public enum PaymentExceptionCode
    {
        UNAUTHORIZED_ACCESS,
        INVALID_FORMAT_CARD_HOLDER,
        INVALID_FORMAT_CARD_NUMBER,
        INVALID_CARD_EXPIRED,
        INVALID_FORMAT_CVV,
        INVALID_FORMAT_AMOUNT,
        INVALID_FORMAT_CURRENCY,
        INVALID_FORMAT_EXPIRY_DATE,
        CURRENCY_NOT_SUPPORTED,
        NOT_AUTHORIZED_BY_BANK,
        PAYMENT_RETRIEVAL_NOT_AUTHORIZED,
        PAYMENT_NOT_FOUND,
        BANK_NOT_SUPPORTED,
        BANK_PAYMENT_PROCESSING,
        ERROR_SAVING_PAYMENT
    }

    [Serializable]
    public class PaymentException : Exception
    {
        public PaymentExceptionCode code { get; set; }
        public string paymentRef { get; set; } = null;
        public bool isAccepted { get; set; }

        public PaymentException(string message, PaymentExceptionCode code)
                : base(message)
        {
            this.code = code;
        }

        public PaymentException(string message, PaymentExceptionCode code, string paymentRef)
        : base(message)
        {
            this.code = code;
            this.paymentRef = paymentRef;
        }
        public PaymentException(string message, PaymentExceptionCode code, string paymentRef, bool isAccepted)
        : base(message)
        {
            this.code = code;
            this.paymentRef = paymentRef;
            this.isAccepted = isAccepted;
        }
    }
}
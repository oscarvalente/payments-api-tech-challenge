namespace PaymentsAPI.Errors
{

    public enum PaymentExceptionCode
    {
        NOT_AUTHORIZED_BY_BANK,
        PAYMENT_RETRIEVAL_NOT_AUTHORIZED,
        PAYMENT_NOT_FOUND
    }

    [Serializable]
    public class PaymentException : Exception
    {
        private PaymentExceptionCode code { get; set; }

        public PaymentException(string message, PaymentExceptionCode code)
        : base(message)
        {
            this.code = code;
        }
    }
}
namespace PaymentsAPI.Errors
{

    public enum GetPaymentExceptionCode
    {
        UNAUTHORIZED_ACCESS,
        INVALID_FORMAT_PAYMENT_REF,
        NOT_FOUND,
    }

    [Serializable]
    public class GetPaymentException : Exception
    {
        public GetPaymentExceptionCode code { get; set; }
        public string paymentRef { get; set; } = null;
        public bool isAccepted { get; set; }

        public GetPaymentException(string message, GetPaymentExceptionCode code)
                : base(message)
        {
            this.code = code;
        }
    }
}
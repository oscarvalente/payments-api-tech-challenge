namespace PaymentsAPI.Errors
{

    public enum SignUpExceptionCode
    {
        USER_ALREADY_EXISTS,
        INVALID_USERNAME,
        INVALID_PASSWORD,
        INVALID_BANK_SWIFT,
        BANK_NOT_SUPPORTED
    }

    [Serializable]
    public class SignUpException : Exception
    {
        public SignUpExceptionCode code { get; set; }

        public SignUpException(string message, SignUpExceptionCode code)
        : base(message)
        {
            this.code = code;
        }
    }
}
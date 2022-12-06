namespace PaymentsAPI.Errors
{

    public enum SignInExceptionCode
    {
        MERCHANT_NOT_VERIFIED,
        INVALID_CREDENTIALS
    }

    [Serializable]
    public class SignInException : Exception
    {
        public SignInExceptionCode code { get; set; }

        public SignInException(string message, SignInExceptionCode code)
        : base(message)
        {
            this.code = code;
        }
    }
}
namespace PaymentsAPI.Errors
{

    public enum SignInExceptionCode
    {
        MERCHANT_NOT_VERIFIED
    }

    [Serializable]
    public class SignInException : Exception
    {
        private SignInExceptionCode code { get; set; }

        public SignInException(string message, SignInExceptionCode code)
        : base(message)
        {
            this.code = code;
        }
    }
}
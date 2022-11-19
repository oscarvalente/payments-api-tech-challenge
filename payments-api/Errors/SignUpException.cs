namespace PaymentsAPI.Errors
{

    public enum SignUpExceptionCode
    {
        USER_ALREADY_EXISTS,
        INVALID_USERNAME,
        INVALID_PASSWORD
    }

    [Serializable]
    public class SignUpException : Exception
    {
        private SignUpExceptionCode code { get; set; }

        public SignUpException(string message, SignUpExceptionCode code)
        : base(message)
        {
            this.code = code;
        }
    }
}
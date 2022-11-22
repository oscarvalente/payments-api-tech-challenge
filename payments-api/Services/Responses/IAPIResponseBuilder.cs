namespace PaymentsAPI.Services.Responses
{
    public interface IAPIResponseBuilder
    {
        public APIError buildInternalError(string code, string message);
        public APIError buildClientError(string type, string message);
        public APIErrorPaymentRef buildClientErrorWithPaymentRef(string type, string message, string paymentRef);

        public TokenResponse buildTokenResponse(string token);
        public PaymentRefResponse buildPaymentRefResponse(string paymentRef);
        public PaymentRefResponse buildPaymentRefResponse(string paymentRef, string message);
    }
}
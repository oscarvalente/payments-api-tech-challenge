using Microsoft.AspNetCore.Mvc.ModelBinding;
using PaymentsAPI.Web.Responses;

namespace PaymentsAPI.Utils
{
    public interface IAPIResponseBuilder
    {
        public APIError buildInternalError(string code, string message);
        public APIInputValidationError buildInputValidationError(string type, ModelStateDictionary model);
        public APIError buildClientError(string type, string message);
        public APIErrorPaymentRef buildClientErrorWithPaymentRef(string type, string message, string paymentRef);

        public TokenResponse buildTokenResponse(string token);
        public PaymentRefResponse buildPaymentRefResponse(string paymentRef);
        public PaymentRefResponse buildPaymentRefResponse(string paymentRef, string message);
    }
}
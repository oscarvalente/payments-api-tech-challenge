using Microsoft.AspNetCore.Mvc.ModelBinding;
using PaymentsAPI.Web.Responses;

namespace PaymentsAPI.Utils
{
    public class APIReponseBuilder : IAPIResponseBuilder
    {
        public APIError buildInternalError(string type, string message)
        {
            return new APIError
            {
                Code = $"I-{type}",
                Message = message,
            };
        }

        public APIError buildClientError(string type, string message)
        {
            return new APIError
            {
                Code = $"E-{type}",
                Message = message,
            };
        }

        public APIInputValidationError buildInputValidationError(string type, ModelStateDictionary model)
        {
            return new APIInputValidationError
            {
                Code = $"E-{type}",
                InputModel = model
            };
        }

        public APIErrorPaymentRef buildClientErrorWithPaymentRef(string type, string message, string paymentRef)
        {
            return new APIErrorPaymentRef
            {
                Code = $"E-{type}",
                Message = message,
                PaymentRef = paymentRef
            };
        }

        public TokenResponse buildTokenResponse(string token)
        {
            return new TokenResponse
            {
                Token = token
            };
        }

        public PaymentRefResponse buildPaymentRefResponse(string paymentRef)
        {
            return new PaymentRefResponse
            {
                PaymentRef = paymentRef
            };
        }
        public PaymentRefResponse buildPaymentRefResponse(string paymentRef, string msg)
        {
            return new PaymentRefResponse
            {
                PaymentRef = paymentRef,
                Message = msg
            };
        }
    }
}
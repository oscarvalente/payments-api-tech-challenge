using PaymentsAPI.Entities;
using PaymentsAPI.Errors;
using PaymentsAPI.Services;

namespace PaymentsAPI.Middlewares
{
    public class AuthenticationValidator
    {
        private readonly RequestDelegate next;
        private readonly IToken tokenService;

        public AuthenticationValidator(RequestDelegate _next, IToken _tokenService)
        {
            next = _next;
            tokenService = _tokenService;
        }

        public async Task InvokeAsync(HttpContext httpContext, IMerchant _merchantService, IRequesterMerchant _requesterMerchant)
        {
            var token = httpContext.Request?.Headers["Authorization"];

            string username = null;
            try
            {
                username = tokenService.verifyToken(token);

                if (username == null)
                {
                    throw new PaymentException("Failed to authenticate", PaymentExceptionCode.UNAUTHORIZED_ACCESS);
                }
            }
            catch (Exception e)
            {
                // for cases where token decode fails
                throw new PaymentException(e.Message, PaymentExceptionCode.UNAUTHORIZED_ACCESS);
            }

            Merchant merchant = _merchantService.getMerchantByUsername(username);

            if (merchant == null)
            {
                throw new PaymentException("Merchant is not authorized to do this operation", PaymentExceptionCode.UNAUTHORIZED_ACCESS);
            }

            _requesterMerchant.merchant = merchant;
            await next(httpContext);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using PaymentsAPI.Entities;
using PaymentsAPI.Errors;
using PaymentsAPI.Services;
using PaymentsAPI.Services.Responses;

namespace PaymentsAPI.Middlewares
{
    public class AuthenticationValidator
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;
        private readonly IToken tokenService;
        private readonly IAPIResponseBuilder apiResponseBuilder;

        public AuthenticationValidator(RequestDelegate _next, ILoggerFactory logFactory, IToken _tokenService, IAPIResponseBuilder _apiResponseBuilder)
        {
            next = _next;
            logger = logFactory.CreateLogger("AuthenticationValidator");
            tokenService = _tokenService;
            apiResponseBuilder = _apiResponseBuilder;
        }

        public async Task InvokeAsync(HttpContext httpContext, IMerchant _merchantService, IRequesterMerchant _requesterMerchant)
        {
            if (httpContext.Request.Path.Value.StartsWith("/api/pay")) // refactor this validation
            {
                logger.LogInformation("AuthenticationValidator executing..");

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
            }
            await next(httpContext);
        }
    }
    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class AuthenticationValidatorExtension
    {
        public static IApplicationBuilder UseAuthenticationValidator(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationValidator>();
        }
    }
}

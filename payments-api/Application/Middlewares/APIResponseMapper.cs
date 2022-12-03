using PaymentsAPI.Errors;
using PaymentsAPI.Services;

namespace PaymentsAPI.Middlewares
{
    public class APIResponseMapper
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public APIResponseMapper(RequestDelegate next, ILoggerFactory logFactory)
        {
            _next = next;
            _logger = logFactory.CreateLogger("APIResponseMapper");
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                _logger.LogInformation("APIResponseMapper executing..");

                await _next(httpContext); // calling next middleware

            }
            catch (PaymentException e)
            {

            }
            catch (Exception e)
            {
                throw e;
                // return APIResult<PaymentRefResponse>.Error(apiResponseBuilder.buildInternalError("UNKNOWN", "Unkown error while processing payment"));
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class APIResponseMapperExtension
    {
        public static IApplicationBuilder UseAPIResponseMapper(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<APIResponseMapper>();
        }
    }
}

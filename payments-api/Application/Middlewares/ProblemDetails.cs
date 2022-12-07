using Hellang.Middleware.ProblemDetails;
using PaymentsAPI.Errors;

public static class ProblemDetails
{
    public static IServiceCollection AddProblemDetailsMapper(this IServiceCollection services)
    {
        return services.AddProblemDetails(options =>
        {
            options.Map<PaymentException>(e =>
            {
                switch (e.code)
                {
                    case PaymentExceptionCode.PAYMENT_NOT_FOUND:
                    case PaymentExceptionCode.PAYMENT_RETRIEVAL_NOT_AUTHORIZED:
                        return new PaymentProblemDetails(e, StatusCodes.Status404NotFound);
                    case PaymentExceptionCode.BANK_PAYMENT_PROCESSING:
                        return new StatusCodeProblemDetails(StatusCodes.Status502BadGateway);
                    case PaymentExceptionCode.ERROR_SAVING_PAYMENT:
                        return new PaymentProblemDetails(e, StatusCodes.Status500InternalServerError);
                    default:
                        return new PaymentProblemDetails(e, StatusCodes.Status400BadRequest);
                }
            });
            options.Map<Exception>(ex => new StatusCodeProblemDetails(StatusCodes.Status500InternalServerError));
        });
    }


    public static IApplicationBuilder UseProblemDetailsMapper(this IApplicationBuilder app)
    {
        return app.UseProblemDetails();
    }
}
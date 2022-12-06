using Hellang.Middleware.ProblemDetails;
using PaymentsAPI.Errors;

public static class ProblemDetails
{
    public static IServiceCollection AddProblemDetailsMapper(this IServiceCollection services)
    {
        return services.AddProblemDetails(options =>
        {
            options.Map<PaymentException>(e => e.code == PaymentExceptionCode.PAYMENT_NOT_FOUND
            || e.code == PaymentExceptionCode.PAYMENT_RETRIEVAL_NOT_AUTHORIZED ?
            new StatusCodeProblemDetails(StatusCodes.Status404NotFound) :
            new PaymentProblemDetails(e));
            options.Map<Exception>(ex => new StatusCodeProblemDetails(StatusCodes.Status500InternalServerError));
        });
    }


    public static IApplicationBuilder UseProblemDetailsMapper(this IApplicationBuilder app)
    {
        return app.UseProblemDetails();
    }
}
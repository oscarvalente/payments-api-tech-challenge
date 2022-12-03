using Hellang.Middleware.ProblemDetails;
using PaymentsAPI.Errors;

public static class ProblemDetails
{
    public static IServiceCollection AddProblemDetailsMapper(this IServiceCollection services)
    {
        return services.AddProblemDetails(x =>
        {
            x.Map<PaymentException>(ex => ex.code == PaymentExceptionCode.PAYMENT_NOT_FOUND
            || ex.code == PaymentExceptionCode.PAYMENT_RETRIEVAL_NOT_AUTHORIZED ?
            new StatusCodeProblemDetails(StatusCodes.Status404NotFound) :
            new StatusCodeProblemDetails(StatusCodes.Status400BadRequest));
            x.Map<Exception>(ex => new StatusCodeProblemDetails(StatusCodes.Status500InternalServerError));
        });
    }


    public static IApplicationBuilder UseProblemDetailsMapper(this IApplicationBuilder app)
    {
        return app.UseProblemDetails();
    }
}
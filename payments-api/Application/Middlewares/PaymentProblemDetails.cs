using Hellang.Middleware.ProblemDetails;
using PaymentsAPI.Errors;

public class PaymentProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
{
    public string PaymentRef { get; set; }
    public PaymentProblemDetails() { }
    public PaymentProblemDetails(PaymentException exception)
    {
        Title = "Payment error";
        Detail = exception.Message;
        PaymentRef = exception.paymentRef;
        Status = StatusCodes.Status400BadRequest;
    }
}
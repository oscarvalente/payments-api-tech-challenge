using Hellang.Middleware.ProblemDetails;
using PaymentsAPI.Errors;

public class PaymentProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
{
    public string PaymentRef { get; set; }
    public PaymentProblemDetails() { }
    public PaymentProblemDetails(PaymentException exception, int statusCode)
    {
        Title = "Payment error";
        Detail = exception.Message;
        PaymentRef = exception.paymentRef;
        Status = statusCode;
    }
}
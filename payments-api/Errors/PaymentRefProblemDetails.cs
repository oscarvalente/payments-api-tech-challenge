
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

public class PaymentRefProblemDetails : ValidationProblemDetails
{
    public PaymentRefProblemDetails()
    {
    }

    [JsonPropertyName("paymentRef")]
    public string PaymentRef { get; }
}
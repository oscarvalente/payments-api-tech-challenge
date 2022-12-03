using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PaymentsAPI.Services.Responses
{
    public class APIInputValidationError : APIError
    {
        public ModelStateDictionary InputModel { get; set; }
    }
}
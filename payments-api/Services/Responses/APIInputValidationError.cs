using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PaymentsAPI.Services.Responses
{
    public class APIInputValidationError
    {
        public string Code { get; set; }
        public ModelStateDictionary InputModel { get; set; }
    }
}
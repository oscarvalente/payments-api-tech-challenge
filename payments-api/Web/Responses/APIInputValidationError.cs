using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PaymentsAPI.Web.Responses
{
    public class APIInputValidationError : APIError
    {
        public ModelStateDictionary InputModel { get; set; }
    }
}
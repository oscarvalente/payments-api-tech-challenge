using System.ComponentModel.DataAnnotations;
using PaymentsAPI.Validations;
using MediatR;

namespace PaymentsAPI.Payments.Handlers
{

    public class GetPaymentQuery : IRequest<PaymentViewModel>
    {
        [Required]
        [GuidValidator]
        public string RefUUID { get; set; }
    }
}
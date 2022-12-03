using System.ComponentModel.DataAnnotations;
using MediatR;
using PaymentsAPI.Controllers.Payments;

namespace PaymentsAPI.Services
{

    public class GetPaymentQuery : IRequest<PaymentViewModel>
    {
        [Required]
        [GuidValidator]
        public string RefUUID { get; set; }
    }
}
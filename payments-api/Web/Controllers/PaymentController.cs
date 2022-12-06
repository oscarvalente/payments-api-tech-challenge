using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentsAPI.Utils;
using PaymentsAPI.Validations;
using PaymentsAPI.Payments.Handlers;

namespace PaymentsAPI.Controllers.Payments
{
    [Route("api")]
    [ApiController]
    [Produces("application/json")]
    public class PaymentController : ControllerBase
    {
        private readonly IAPIResponseBuilder apiResponseBuilder;
        private readonly IMediator mediator;

        public PaymentController(IAPIResponseBuilder _apiResponseBuilder, IMediator _mediator)
        {
            apiResponseBuilder = _apiResponseBuilder;
            mediator = _mediator;
        }

        [HttpPost("pay")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Pay([FromBody] CreatePaymentCommand command, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(command, cancellationToken);
            return Created(result.RefUUID, apiResponseBuilder.buildPaymentRefResponse(result.RefUUID));
        }

        [HttpGet("payment/{paymentRef}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPayment([FromRoute][Required][GuidValidator] string paymentRef, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetPaymentQuery() { RefUUID = paymentRef }, cancellationToken);
            return Ok(result);
        }
    }
}
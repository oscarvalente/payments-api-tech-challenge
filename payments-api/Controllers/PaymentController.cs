using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentsAPI.Errors;
using PaymentsAPI.Services;
using PaymentsAPI.Services.Responses;

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
        public async Task<IActionResult> Pay([FromBody] CreatePaymentCommand command)
        {
            try
            {
                var result = await mediator.Send(command, CancellationToken.None);
                return Created(result, apiResponseBuilder.buildPaymentRefResponse(result));
            }
            // validation of business logic
            catch (PaymentException e)
            {
                if (e.code == PaymentExceptionCode.ERROR_SAVING_PAYMENT && e.isAccepted)
                {
                    return Created(e.paymentRef, apiResponseBuilder.buildPaymentRefResponse(e.paymentRef, "Your payment was accepted but it's still not available for status check"));
                }
                throw e;
            }
        }

        [HttpGet("payment/{paymentRef}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPayment(string paymentRef)
        {
            var result = await mediator.Send(new GetPaymentQuery() { RefUUID = paymentRef }, CancellationToken.None);
            return Ok(result);
        }
    }
}
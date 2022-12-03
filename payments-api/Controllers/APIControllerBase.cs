using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace PaymentsAPI.Controllers.Payments;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender mediator = null!;

    protected ISender Mediator => mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
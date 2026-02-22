using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class BaseController : Controller
{
    private IMediator _mediator;
    protected IMediator Mediator =>
        _mediator ??= (IMediator)HttpContext.RequestServices.GetService(typeof(IMediator));
}

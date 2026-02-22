using MediatR;

namespace WebApi.Extension;

public class HangfireMediatorHelper
{
    private readonly IMediator _mediator;

    public HangfireMediatorHelper(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Send<T>(IRequest<T> request)
    {
        await _mediator.Send(request);
    }
}

using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Infrastructures
{
    public class RequestPerformanceBehaviour<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly Stopwatch _timer;
        private readonly ILogger<TRequest> _logger;

        public RequestPerformanceBehaviour(ILogger<TRequest> logger)
        {
            _timer = new Stopwatch();

            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        )
        {
            _timer.Start();

            var response = await next();

            _timer.Stop();

            if (_timer.ElapsedMilliseconds > 10000)
            {
                var name = typeof(TRequest).Name;

                // TODO: Add User Details

                _logger.LogWarning(
                    "TheFork Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@Request}",
                    name,
                    _timer.ElapsedMilliseconds,
                    request
                );
            }

            return response;
        }
    }
}

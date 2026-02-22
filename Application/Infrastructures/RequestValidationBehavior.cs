using Application.Exceptions;
using FluentValidation;
using MediatR;

namespace Application.Infrastructures
{
    public class RequestValidationBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        )
        {
            var context = new ValidationContext<TRequest>(request);

            var validationTasks = _validators.Select(v =>
                v.ValidateAsync(context, cancellationToken)
            );

            var validationResults = await Task.WhenAll(validationTasks);

            var failures = validationResults
                .SelectMany(result => result.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                throw new AppValidationException(failures);
            }

            return await next();
        }
    }
}

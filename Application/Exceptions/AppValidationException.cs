using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;

namespace Application.Exceptions
{
    public class AppValidationException : Exception
    {
        public AppValidationException()
            : base("One or more validation failures have occurred.")
        {
            Failures = new Dictionary<string, string[]>();
        }

        public AppValidationException(IReadOnlyCollection<ValidationFailure> failures)
            : this()
        {
            var propertyNames = failures.Select(e => e.PropertyName).Distinct();

            foreach (var propertyName in propertyNames)
            {
                var propertyFailures = failures
                    .Where(e => e.PropertyName == propertyName)
                    .Select(e => e.ErrorMessage)
                    .ToArray();

                Failures.Add(propertyName, propertyFailures);
            }
        }

        public AppValidationException(List<IdentityError> failures)
            : this()
        {
            var propertyNames = failures.Select(e => e.Code).Distinct();

            foreach (var propertyName in propertyNames)
            {
                var propertyFailures = failures
                    .Where(e => e.Code == propertyName)
                    .Select(e => e.Description)
                    .ToArray();
                Failures.Add(propertyName, propertyFailures);
            }
        }

        public IDictionary<string, string[]> Failures { get; }
    }
}

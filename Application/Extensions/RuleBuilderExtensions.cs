using System.Text.RegularExpressions;
using Common.Constants;
using FluentValidation;
using Newtonsoft.Json;

namespace Application.Extensions
{
    public static class RuleBuilderExtensions
    {
        public static IRuleBuilder<T, string> Password<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            int minimumLength = 8
        )
        {
            IRuleBuilderOptions<T, string> options = ruleBuilder
                .Required()
                .MinimumLength(minimumLength)
                .WithMessage(ValidationMessages.PasswordLength)
                .Matches("[A-Z]")
                .WithMessage(ValidationMessages.PasswordUppercaseLetter)
                .Matches("[a-z]")
                .WithMessage(ValidationMessages.PasswordLowercaseLetter)
                .Matches("[0-9]")
                .WithMessage(ValidationMessages.PasswordDigit)
                .Matches("[^a-zA-Z0-9]")
                .WithMessage(ValidationMessages.PasswordSpecialCharacter);
            return options;
        }

        public static IRuleBuilder<T, string> Pin<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            int minimumLength = 4
        )
        {
            IRuleBuilderOptions<T, string> options = ruleBuilder
                .Required()
                .MinimumLength(minimumLength)
                .WithMessage(ValidationMessages.LessThan(minimumLength))
                .MaximumLength(minimumLength)
                .WithMessage(ValidationMessages.GreaterThan(minimumLength));
            return options;
        }

        public static IRuleBuilder<T, string> ZipCode<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            int minimumLength = 4,
            int maxLength = 6
        )
        {
            IRuleBuilderOptions<T, string> options = ruleBuilder
                .Required()
                .Must(p => int.TryParse(p, out int result))
                .MinimumLength(minimumLength)
                .WithMessage(ValidationMessages.LessThan(minimumLength))
                .MaximumLength(maxLength)
                .WithMessage(ValidationMessages.GreaterThan(maxLength));
            return options;
        }

        public static IRuleBuilder<T, string> Otp<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            int length = 4
        )
        {
            int minimumLength = length;
            int maximumLength = length;
            var regexPattern = $@"([0-9]{{{minimumLength},{maximumLength}}})";
            var options = ruleBuilder
                .Must(p => (string.IsNullOrWhiteSpace(p)) || (Regex.IsMatch(p, regexPattern)))
                .WithMessage(ValidationMessages.InvalidFormat);
            return options;
        }

        public static IRuleBuilder<T, string> Phone<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            bool isOptional = false
        )
        {
            int minimumLength = 10;
            int maximumLength = 15;
            var regexPattern = $@"\+([0-9]{{{minimumLength},{maximumLength}}})";
            Regex regex = new Regex(regexPattern);
            var options = ruleBuilder
                .Must(p =>
                    (string.IsNullOrWhiteSpace(p) && isOptional)
                    || (p != null && Regex.IsMatch(p, regexPattern))
                )
                .WithMessage(ValidationMessages.InvalidFormat);
            return options;
        }

        public static IRuleBuilder<T, string> PhoneUS<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            bool isOptional = false
        )
        {
            int minimumLength = 10;
            int maximumLength = 10;
            var regexPattern = $@"([0-9]{{{minimumLength},{maximumLength}}})";
            Regex regex = new Regex(regexPattern);
            var options = ruleBuilder
                .Must(p =>
                    (string.IsNullOrWhiteSpace(p) && isOptional)
                    || (p != null && Regex.IsMatch(p, regexPattern))
                )
                .WithMessage(ValidationMessages.InvalidFormat);
            return options;
        }

        public static IRuleBuilder<T, string> Time<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            int minimumLength = 8;
            IRuleBuilderOptions<T, string> options = ruleBuilder
                .Required()
                .MinimumLength(minimumLength)
                .WithMessage(ValidationMessages.LessThan(minimumLength))
                .MaximumLength(minimumLength)
                .WithMessage(ValidationMessages.GreaterThan(minimumLength));
            return options;
        }

        public static IRuleBuilder<T, string> MustBeOneOf<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            params string[] list
        )
        {
            IRuleBuilderOptions<T, string> options = ruleBuilder
                .Must(p => list.Contains(p))
                .WithMessage(ValidationMessages.IncorrectValue);
            return options;
        }

        public static IRuleBuilder<T, int> MustBeOneOf<T>(
            this IRuleBuilder<T, int> ruleBuilder,
            params int[] list
        )
        {
            IRuleBuilderOptions<T, int> options = ruleBuilder
                .Must(list.Contains)
                .WithMessage(ValidationMessages.IncorrectValue);
            return options;
        }

        public static IRuleBuilder<T, int> Max<T>(this IRuleBuilder<T, int> ruleBuilder, int max)
        {
            IRuleBuilderOptions<T, int> options = ruleBuilder
                .Must(p => p <= max)
                .WithMessage(ValidationMessages.GreaterThan(max));
            return options;
        }

        public static IRuleBuilder<T, double> Max<T>(
            this IRuleBuilder<T, double> ruleBuilder,
            int max
        )
        {
            IRuleBuilderOptions<T, double> options = ruleBuilder
                .Must(p => p <= max)
                .WithMessage(ValidationMessages.GreaterThan(max));
            return options;
        }

        public static IRuleBuilder<T, decimal> Max<T>(
            this IRuleBuilder<T, decimal> ruleBuilder,
            int max
        )
        {
            IRuleBuilderOptions<T, decimal> options = ruleBuilder
                .Must(p => p <= max)
                .WithMessage(ValidationMessages.GreaterThan(max));
            return options;
        }

        public static IRuleBuilder<T, double> Min<T>(
            this IRuleBuilder<T, double> ruleBuilder,
            double min
        )
        {
            IRuleBuilderOptions<T, double> options = ruleBuilder
                .Must(p => p >= min)
                .WithMessage(ValidationMessages.LessThan(min));
            return options;
        }

        public static IRuleBuilder<T, decimal> Min<T>(
            this IRuleBuilder<T, decimal> ruleBuilder,
            int min
        )
        {
            IRuleBuilderOptions<T, decimal> options = ruleBuilder
                .Must(p => p >= min)
                .WithMessage(ValidationMessages.LessThan(min));
            return options;
        }

        public static IRuleBuilder<T, float> Min<T>(
            this IRuleBuilder<T, float> ruleBuilder,
            int min
        )
        {
            IRuleBuilderOptions<T, float> options = ruleBuilder
                .Must(p => p >= min)
                .WithMessage(ValidationMessages.LessThan(min));
            return options;
        }

        public static IRuleBuilder<T, int> Min<T>(this IRuleBuilder<T, int> ruleBuilder, int min)
        {
            IRuleBuilderOptions<T, int> options = ruleBuilder
                .Must(p => p >= min)
                .WithMessage(ValidationMessages.LessThan(min));
            return options;
        }

        public static IRuleBuilder<T, string> Max<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            int max
        )
        {
            IRuleBuilderOptions<T, string> options = ruleBuilder
                .Must(p => string.IsNullOrWhiteSpace(p) || p.Length <= max)
                .WithMessage(ValidationMessages.MaxLengthError(max));
            return options;
        }

        public static IRuleBuilder<T, string> Min<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            uint min
        )
        {
            IRuleBuilderOptions<T, string> options = ruleBuilder
                .Must(p => string.IsNullOrWhiteSpace(p) || p.Length >= min)
                .WithMessage(ValidationMessages.MinLengthError((int)min));
            return options;
        }

        public static IRuleBuilder<T, string> Required<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            IRuleBuilderOptions<T, string> options = ruleBuilder
                .Must(p => !string.IsNullOrEmpty(p))
                .WithMessage(ValidationMessages.EmptyError);
            return options;
        }

        public static IRuleBuilder<T, string> HasNoSpecialCharacter<T>(
            this IRuleBuilder<T, string> ruleBuilder
        )
        {
            IRuleBuilderOptions<T, string> options = ruleBuilder
                .Must(p =>
                    p.All(c =>
                        (c is >= 'A' and <= 'Z')
                        || (c is >= 'a' and <= 'z')
                        || (c is >= '0' and <= '9')
                    )
                )
                .WithMessage(ValidationMessages.EmptyError);
            return options;
        }

        public static IRuleBuilder<T, Dictionary<string, string>> Required<T>(
            this IRuleBuilder<T, Dictionary<string, string>> ruleBuilder
        )
        {
            IRuleBuilderOptions<T, Dictionary<string, string>> options = ruleBuilder
                .Must(p => p.Count > 0)
                .WithMessage(ValidationMessages.EmptyError);
            return options;
        }

        public static IRuleBuilder<T, Dictionary<string, string>> KeyMustBeOneOf<T>(
            this IRuleBuilder<T, Dictionary<string, string>> ruleBuilder,
            params string[] keys
        )
        {
            IRuleBuilderOptions<T, Dictionary<string, string>> options = ruleBuilder
                .Must(p => p.Keys.All(keys.Contains))
                .WithMessage(ValidationMessages.EmptyError);
            return options;
        }

        public static IRuleBuilder<T, DateTime> Required<T>(
            this IRuleBuilder<T, DateTime> ruleBuilder
        )
        {
            IRuleBuilderOptions<T, DateTime> options = ruleBuilder
                .Must(p => p > DateTime.MinValue && p < DateTime.MaxValue)
                .WithMessage(ValidationMessages.EmptyError);
            return options;
        }

        public static IRuleBuilder<T, TimeSpan> Required<T>(
            this IRuleBuilder<T, TimeSpan> ruleBuilder
        )
        {
            IRuleBuilderOptions<T, TimeSpan> options = ruleBuilder
                .Must(p => p > TimeSpan.MinValue && p < TimeSpan.MaxValue)
                .WithMessage(ValidationMessages.EmptyError);
            return options;
        }

        public static IRuleBuilder<T, DateTime?> Required<T>(
            this IRuleBuilder<T, DateTime?> ruleBuilder
        )
        {
            IRuleBuilderOptions<T, DateTime?> options = ruleBuilder
                .Must(p => p > DateTime.MinValue && p < DateTime.MaxValue)
                .WithMessage(ValidationMessages.EmptyError);
            return options;
        }

        public static IRuleBuilder<T, DateOnly> Required<T>(
            this IRuleBuilder<T, DateOnly> ruleBuilder
        )
        {
            IRuleBuilderOptions<T, DateOnly> options = ruleBuilder
                .Must(p => p > DateOnly.MinValue && p < DateOnly.MaxValue)
                .WithMessage(ValidationMessages.EmptyError);
            return options;
        }

        public static IRuleBuilder<T, object[]> Required<T>(
            this IRuleBuilder<T, object[]> ruleBuilder
        )
        {
            IRuleBuilderOptions<T, object[]> options = ruleBuilder
                .Must(p => p.Length > 0 && p.All(pr => pr != null))
                .WithMessage(ValidationMessages.EmptyError);
            return options;
        }

        public static IRuleBuilder<T, string[]> Required<T>(
            this IRuleBuilder<T, string[]> ruleBuilder,
            bool isOptional = false
        )
        {
            IRuleBuilderOptions<T, string[]> options;
            if (isOptional)
            {
                options = ruleBuilder
                    .Must(p => p != null)
                    .WithMessage(ValidationMessages.EmptyError);
            }
            else
            {
                options = ruleBuilder
                    .Must(p => p != null && p.Length > 0 && !p.Any(string.IsNullOrEmpty))
                    .WithMessage(ValidationMessages.EmptyError);
            }

            return options;
        }

        public static IRuleBuilder<T, TEnum[]> MustBeOneOf<T, TEnum>(
            this IRuleBuilder<T, TEnum[]> ruleBuilder,
            params TEnum[] allowedValues
        )
            where TEnum : struct, Enum
        {
            return ruleBuilder
                .Must(values => values.All(allowedValues.Contains))
                .WithMessage(ValidationMessages.EmptyError);
        }

        public static IRuleBuilder<T, string[]> MustBeOneOf<T>(
            this IRuleBuilder<T, string[]> ruleBuilder,
            params string[] keys
        )
        {
            var options = ruleBuilder
                .Must(p => p.All(keys.Contains))
                .WithMessage(ValidationMessages.EmptyError);
            return options;
        }

        public static IRuleBuilder<T, int[]> Required<T>(this IRuleBuilder<T, int[]> ruleBuilder)
        {
            IRuleBuilderOptions<T, int[]> options = ruleBuilder
                .Must(p => p != null && p.Length > 0 && p.All(pr => pr > 0))
                .WithMessage(ValidationMessages.EmptyError);
            return options;
        }

        public static IRuleBuilder<T, int[]> Max<T>(
            this IRuleBuilder<T, int[]> ruleBuilder,
            int maxLength = 10
        )
        {
            IRuleBuilderOptions<T, int[]> options = ruleBuilder
                .Must(p => p != null && p.Length <= maxLength)
                .WithMessage(ValidationMessages.MaxLengthError(maxLength, "Items"));
            return options;
        }

        public static IRuleBuilder<T, string[]> Max<T>(
            this IRuleBuilder<T, string[]> ruleBuilder,
            int maxLength = 10
        )
        {
            IRuleBuilderOptions<T, string[]> options = ruleBuilder
                .Must(p => p != null && p.Length <= maxLength)
                .WithMessage(ValidationMessages.MaxLengthError(maxLength, "Items"));
            return options;
        }

        public static IRuleBuilder<T, double[]> Required<T>(
            this IRuleBuilder<T, double[]> ruleBuilder
        )
        {
            IRuleBuilderOptions<T, double[]> options = ruleBuilder
                .Must(p => p != null && p.Length > 0 && p.All(pr => pr > 0))
                .WithMessage(ValidationMessages.EmptyError);
            return options;
        }

        public static IRuleBuilder<T, ICollection<T2>> Required<T, T2>(
            this IRuleBuilder<T, ICollection<T2>> ruleBuilder
        )
            where T2 : class
        {
            var options = ruleBuilder
                .Must(p => p != null && p.Count > 0)
                .WithMessage(ValidationMessages.EmptyError);
            return options;
        }

        public static IRuleBuilder<T, string> Identification<T>(
            this IRuleBuilder<T, string> ruleBuilder
        )
        {
            IRuleBuilderOptions<T, string> options = ruleBuilder
                .Required()
                .Must(p => p.Length == 13 || p.Length == 17)
                .WithMessage(ValidationMessages.InvalidFormat);
            return options;
        }

        public static IRuleBuilder<T, int> Required<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            IRuleBuilder<T, int> options = ruleBuilder.Min(1);
            return options;
        }

        public static IRuleBuilder<T, double> Required<T>(this IRuleBuilder<T, double> ruleBuilder)
        {
            IRuleBuilder<T, double> options = ruleBuilder.Min(0.01);
            return options;
        }

        public static IRuleBuilder<T, decimal> Required<T>(
            this IRuleBuilder<T, decimal> ruleBuilder
        )
        {
            IRuleBuilder<T, decimal> options = ruleBuilder.Min(1);
            return options;
        }

        public static IRuleBuilder<T, float> Required<T>(this IRuleBuilder<T, float> ruleBuilder)
        {
            IRuleBuilder<T, float> options = ruleBuilder.Min(1);
            return options;
        }

        public static IRuleBuilder<T, double> Latitude<T>(this IRuleBuilder<T, double> ruleBuilder)
        {
            IRuleBuilder<T, double> options = ruleBuilder.Min(-90).Max(90);
            return options;
        }

        public static IRuleBuilder<T, double> Longitude<T>(this IRuleBuilder<T, double> ruleBuilder)
        {
            IRuleBuilder<T, double> options = ruleBuilder.Min(-180).Max(180);
            return options;
        }

        public static IRuleBuilder<T, string> Json<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            IRuleBuilderOptions<T, string> options = ruleBuilder
                .Required()
                .Must(p => JsonConvert.DeserializeObject(p) != null)
                .WithMessage(ValidationMessages.InvalidFormat);
            return options;
        }
    }
}

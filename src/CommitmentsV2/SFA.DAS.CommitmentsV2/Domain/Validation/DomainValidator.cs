using System.Linq;
using FluentValidation;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;

namespace SFA.DAS.CommitmentsV2.Domain.Validation
{
    public class DomainValidator : IDomainValidator
    {
        private static readonly DomainError[] ValidationOkay = new DomainError[0];

        private readonly IValidator[] _validators;

        public DomainValidator(IValidator[] validators)
        {
            _validators = validators;
        }

        public DomainError[] Validate<T>(T instance) where T : class
        {
            var validator = GetValidator(instance);

            if (validator == null)
            {
                return ValidationOkay;
            }

            var validationResult = validator.Validate(instance);

            if (validationResult.IsValid)
            {
                return ValidationOkay;
            }

            return validationResult.Errors.Select(e => new DomainError(e.PropertyName, e.ErrorMessage)).ToArray();
        }

        private IValidator<T> GetValidator<T>(T instance)
        {
            return _validators.OfType<IValidator<T>>().FirstOrDefault();
        }
    }
}
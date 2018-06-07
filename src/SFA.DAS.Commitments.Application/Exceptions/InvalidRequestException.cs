using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using FluentValidation.Results;

namespace SFA.DAS.Commitments.Application.Exceptions
{
    public class InvalidRequestException : Exception
    {
        public IDictionary<string, string> ErrorMessages { get; private set; }

        public InvalidRequestException()
            : this(new Dictionary<string, string>()) {}

        public InvalidRequestException(IDictionary<string, string> errorMessages)
            : base(BuildErrorMessage(errorMessages))
        {
            ErrorMessages = errorMessages;
        }

        public InvalidRequestException(IEnumerable<ValidationFailure> failures)
            : this(failures.ToDictionary(failure => failure.PropertyName, failure => failure.ErrorMessage)) {}

        protected InvalidRequestException(SerializationInfo info, StreamingContext context)
          : base(info, context) {}

        private static string BuildErrorMessage(IDictionary<string, string> errorMessages)
        {
            if (errorMessages.Count == 0)
                return "Request is invalid";

            return $"Request is invalid:\n{errorMessages.Select(kvp => $"{kvp.Key}: {kvp.Value}").Aggregate((x, y) => $"{x}\n{y}")}";
        }
    }
}

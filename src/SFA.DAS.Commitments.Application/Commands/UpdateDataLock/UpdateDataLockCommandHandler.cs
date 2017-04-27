using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

namespace SFA.DAS.Commitments.Application.Commands.UpdateDataLock
{
    public sealed class UpdateDataLockCommandHandler : AsyncRequestHandler<UpdateDataLockCommand>
    {
        private readonly AbstractValidator<UpdateDataLockCommand> _validator;

        public UpdateDataLockCommandHandler(AbstractValidator<UpdateDataLockCommand> validator)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));

            _validator = validator;
        }

        protected override async Task HandleCore(UpdateDataLockCommand message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }
    }
}

using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Commands.UpdateCustomProviderPaymentPriority;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement
{
    public sealed class UpdateProviderPaymentsPriorityCommandHandler : AsyncRequestHandler<UpdateProviderPaymentsPriorityCommand>
    {
        private AbstractValidator<UpdateProviderPaymentsPriorityCommand> _validator;

        public UpdateProviderPaymentsPriorityCommandHandler(AbstractValidator<UpdateProviderPaymentsPriorityCommand> validator)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));

            _validator = validator;
        }

        protected override Task HandleCore(UpdateProviderPaymentsPriorityCommand message)
        {
            _validator.ValidateAndThrow(message);

            return null;
        }
    }
}

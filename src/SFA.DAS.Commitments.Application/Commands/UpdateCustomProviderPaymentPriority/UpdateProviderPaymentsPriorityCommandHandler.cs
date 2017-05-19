using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Commands.UpdateCustomProviderPaymentPriority;
using System;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement
{
    public sealed class UpdateProviderPaymentsPriorityCommandHandler : AsyncRequestHandler<UpdateProviderPaymentsPriorityCommand>
    {
        private AbstractValidator<UpdateProviderPaymentsPriorityCommand> _validator;
        private IProviderPaymentRepository _providerPaymentRepository;

        public UpdateProviderPaymentsPriorityCommandHandler(AbstractValidator<UpdateProviderPaymentsPriorityCommand> validator)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));

            _validator = validator;
        }

        public UpdateProviderPaymentsPriorityCommandHandler(UpdateProviderPaymentsPriorityCommandValidator validator, IProviderPaymentRepository providerPaymentRepository)
        {
            _validator = validator;
            _providerPaymentRepository = providerPaymentRepository;
        }

        protected override Task HandleCore(UpdateProviderPaymentsPriorityCommand message)
        {
            _validator.ValidateAndThrow(message);

            // Save new order to the database

            // Re-prioritise the apprenticeships

            // Send update events to Events Api

            return null;
        }
    }
}

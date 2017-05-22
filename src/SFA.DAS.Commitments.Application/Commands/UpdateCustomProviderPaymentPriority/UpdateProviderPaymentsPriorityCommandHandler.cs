using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Commands.UpdateCustomProviderPaymentPriority;
using System;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Application.Commands.SetPaymentOrder;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement
{
    public sealed class UpdateProviderPaymentsPriorityCommandHandler : AsyncRequestHandler<UpdateProviderPaymentsPriorityCommand>
    {
        private readonly AbstractValidator<UpdateProviderPaymentsPriorityCommand> _validator;
        private readonly IProviderPaymentRepository _providerPaymentRepository;
        private readonly IMediator _mediator;

        public UpdateProviderPaymentsPriorityCommandHandler(AbstractValidator<UpdateProviderPaymentsPriorityCommand> validator, IProviderPaymentRepository providerPaymentRepository, IMediator mediator)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            if (providerPaymentRepository == null)
                throw new ArgumentNullException(nameof(providerPaymentRepository));
            if (mediator == null)
                throw new ArgumentNullException(nameof(mediator));

            _validator = validator;
            _providerPaymentRepository = providerPaymentRepository;
            _mediator = mediator;
        }

        protected override async Task HandleCore(UpdateProviderPaymentsPriorityCommand message)
        {
            _validator.ValidateAndThrow(message);

            // Save new order to the database
            await _providerPaymentRepository.UpdateProviderPaymentPriority(message.EmployerAccountId, message.ProviderPriorities);

            // Re-prioritise the apprenticeships & Send update events to Events Api
            await _mediator.SendAsync(new SetPaymentOrderCommand { AccountId = message.EmployerAccountId });
        }
    }
}

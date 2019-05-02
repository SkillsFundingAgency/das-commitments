using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Commands.SetPaymentOrder;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.Commitments.Application.Commands.UpdateCustomProviderPaymentPriority
{
    public sealed class UpdateProviderPaymentsPriorityCommandHandler : AsyncRequestHandler<UpdateProviderPaymentsPriorityCommand>
    {
        private readonly AbstractValidator<UpdateProviderPaymentsPriorityCommand> _validator;
        private readonly IProviderPaymentRepository _providerPaymentRepository;
        private readonly IMediator _mediator;
        private readonly IV2EventsPublisher _v2EventsPublisher;

        public UpdateProviderPaymentsPriorityCommandHandler(AbstractValidator<UpdateProviderPaymentsPriorityCommand> validator, 
            IProviderPaymentRepository providerPaymentRepository, 
            IMediator mediator,
            IV2EventsPublisher v2EventsPublisher)
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
            _v2EventsPublisher = v2EventsPublisher;
        }

        protected override async Task HandleCore(UpdateProviderPaymentsPriorityCommand message)
        {
            IEnumerable<ProviderPaymentOrder> MapToPaymentOrderForV2Event()
            {
                var providerPaymentOrders = message.ProviderPriorities.Select(x => new ProviderPaymentOrder
                    {Priority = x.PriorityOrder, ProviderId = x.ProviderId}).AsEnumerable();
                return providerPaymentOrders;
            }

            _validator.ValidateAndThrow(message);

            // Save new order to the database
            await _providerPaymentRepository.UpdateProviderPaymentPriority(message.EmployerAccountId, message.ProviderPriorities);

            // Re-prioritise the apprenticeships & Send update events to Events Api
            await Task.WhenAll(_mediator.SendAsync(new SetPaymentOrderCommand { AccountId = message.EmployerAccountId }),
                _v2EventsPublisher.PublishPaymentOrderChanged(message.EmployerAccountId, MapToPaymentOrderForV2Event()));
        }
    }
}

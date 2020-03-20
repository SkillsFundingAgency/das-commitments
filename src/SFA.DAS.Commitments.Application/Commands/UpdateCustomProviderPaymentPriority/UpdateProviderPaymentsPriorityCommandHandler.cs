using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Domain.Data;

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
            _validator = validator;
            _providerPaymentRepository = providerPaymentRepository;
            _mediator = mediator;
            _v2EventsPublisher = v2EventsPublisher;
        }

        protected override async Task HandleCore(UpdateProviderPaymentsPriorityCommand message)
        {
            IEnumerable<int> MapToPaymentOrderForV2Event()
            {
                return message.ProviderPriorities.OrderBy(x=>x.PriorityOrder).Select(x => (int)x.ProviderId);
            }

            _validator.ValidateAndThrow(message);

            await _providerPaymentRepository.UpdateProviderPaymentPriority(message.EmployerAccountId, message.ProviderPriorities);
            await _v2EventsPublisher.PublishPaymentOrderChanged(message.EmployerAccountId, MapToPaymentOrderForV2Event());
        }
    }
}

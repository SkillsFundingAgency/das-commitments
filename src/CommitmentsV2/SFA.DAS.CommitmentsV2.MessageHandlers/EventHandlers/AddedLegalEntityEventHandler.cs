using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.AddAccountLegalEntity;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class AddedLegalEntityEventHandler : IHandleMessages<AddedLegalEntityEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AddedLegalEntityEventHandler> _logger;

        public AddedLegalEntityEventHandler(IMediator mediator, ILogger<AddedLegalEntityEventHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(AddedLegalEntityEvent message, IMessageHandlerContext context)
        {
            try
            { 
                await _mediator.Send(new AddAccountLegalEntityCommand(message.AccountId, message.AccountLegalEntityId, message.LegalEntityId,
                    message.AccountLegalEntityPublicHashedId, message.OrganisationName,
                    (Models.OrganisationType)message.OrganisationType,
                    message.OrganisationReferenceNumber,
                    message.OrganisationAddress,
                    message.Created));

                _logger.LogInformation($"AddLegalEntity - successfully handled {nameof(AddedLegalEntityEvent)} with {nameof(message.AccountLegalEntityPublicHashedId)}: {message.AccountLegalEntityPublicHashedId}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"AddLegalEntity - failed to handle {nameof(AddedLegalEntityEvent)} with {nameof(message.AccountLegalEntityPublicHashedId)}: {message.AccountLegalEntityPublicHashedId}");
                throw;
            }
        }
    }
}
using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.RemoveAccountLegalEntity;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class RemovedLegalEntityEventHandler : IHandleMessages<RemovedLegalEntityEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RemovedLegalEntityEventHandler> _logger;

        public RemovedLegalEntityEventHandler(IMediator mediator, ILogger<RemovedLegalEntityEventHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(RemovedLegalEntityEvent message, IMessageHandlerContext context)
        {
            try
            {
                await _mediator.Send(new RemoveAccountLegalEntityCommand(message.AccountId,
                    message.AccountLegalEntityId, message.Created));

                _logger.LogInformation($"RemoveLegalEntity - successfully handled {nameof(RemovedLegalEntityEvent)} with created date: {message.Created}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"RemoveLegalEntity - failed to handle {nameof(RemovedLegalEntityEvent)} with created date: {message.Created}");
                throw;
            }
        }
    }
}
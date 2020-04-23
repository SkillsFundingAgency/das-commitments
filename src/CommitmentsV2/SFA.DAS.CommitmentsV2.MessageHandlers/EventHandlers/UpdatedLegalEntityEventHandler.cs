using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountLegalEntityName;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class UpdatedLegalEntityEventHandler : IHandleMessages<UpdatedLegalEntityEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdatedLegalEntityEventHandler> _logger;

        public UpdatedLegalEntityEventHandler(IMediator mediator, ILogger<UpdatedLegalEntityEventHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(UpdatedLegalEntityEvent message, IMessageHandlerContext context)
        {
            try
            {
                await _mediator.Send(new UpdateAccountLegalEntityNameCommand(message.AccountLegalEntityId,
                    message.Name, message.Created));

                _logger.LogInformation($"UpdateLegalEntity - successfully handled {nameof(UpdatedLegalEntityEvent)} with created date: {message.Created}");
            }
            catch (Exception e)
            {
                _logger.LogError(e,$"UpdateLegalEntity - failed to handle {nameof(UpdatedLegalEntityEvent)} with created date: {message.Created}");
                throw;
            }
        }
    }
}
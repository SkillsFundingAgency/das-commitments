using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Application.Commands.ApproveCohort;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class TransferRequestApprovedEventHandler : IHandleMessages<TransferRequestApprovedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TransferRequestCreatedEvent> _logger;

        public TransferRequestApprovedEventHandler(IMediator mediator, ILogger<TransferRequestCreatedEvent> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public Task Handle(TransferRequestApprovedEvent message, IMessageHandlerContext context)
        {
            try
            {
                return _mediator.Send(new ApproveCohortCommand(message.CohortId, null, message.UserInfo));
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error when trying to approve Cohort {message.CohortId} for TransferRequest {message.TransferRequestId}");
                throw;
            }
        }
    }
}
using System.Threading.Tasks;
using MediatR;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.AddHistory;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class EntityStateChangedEventHandler : IHandleMessages<EntityStateChangedEvent>
    {
        private IMediator _mediator;

        public EntityStateChangedEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Handle(EntityStateChangedEvent message, IMessageHandlerContext context)
        {
            await _mediator.Send(new AddHistoryCommand
            {
                StateChangeType = message.StateChangeType,
                EntityId = message.EntityId,
                InitialState = message.InitialState,
                UpdatedState = message.UpdatedState,
                UpdatedOn = message.UpdatedOn,
                UpdatingUserName = message.UpdatingUserName,
                UpdatingParty = message.UpdatingParty,
                UpdatingUserId = message.UpdatingUserId,
                EmployerAccountId = message.EmployerAccountId,
                ProviderId = message.ProviderId,
                EntityType = message.EntityType,
                Diff = message.Diff
            });
        }
    }
}

﻿using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Application.Commands.AddHistory;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class EntityStateChangedEventHandler(IMediator mediator, IDiffService diffService) : IHandleMessages<EntityStateChangedEvent>
{
    public async Task Handle(EntityStateChangedEvent message, IMessageHandlerContext context)
    {
        var initialState = message.InitialState == null ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(message.InitialState);
        var updatedState = message.UpdatedState == null ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(message.UpdatedState);
        var diff = diffService.GenerateDiff(initialState, updatedState);
        if (diff.Count == 0) return;

        await mediator.Send(new AddHistoryCommand
        {
            CorrelationId = message.CorrelationId,
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
            ApprenticeshipId = message.ApprenticeshipId,
            Diff = JsonConvert.SerializeObject(diff)
        });
    }
}
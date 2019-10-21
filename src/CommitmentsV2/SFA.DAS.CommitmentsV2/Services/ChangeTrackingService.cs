using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class ChangeTrackingService : IChangeTrackingService
    {
        private List<TrackedItem> _trackedItems;
        private IDiffGeneratorService _diffGenerator;

        private Guid _correlationId;
        private UserAction _userAction;
        private long _employerAccountId;
        private long _providerId;
        private Party _party;
        private UserInfo _userInfo;


        public ChangeTrackingService(IDiffGeneratorService diffGenerator)
        {
            _diffGenerator = diffGenerator;
            _trackedItems = new List<TrackedItem>();
        }

        public void BeginTrackingSession(UserAction userAction, Party party, long employerAccountId, long providerId, UserInfo userInfo)
        {
            _userAction = userAction;
            _party = party;
            _employerAccountId = employerAccountId;
            _providerId = providerId;
            _userInfo = userInfo;
            _correlationId = Guid.NewGuid();
            _trackedItems.Clear();
        }

        public void TrackInsert(IMementoCreator trackedObject)
        {
            _trackedItems.Add(new TrackedItem(trackedObject, ChangeTrackingOperation.Insert));
        }

        public void TrackUpdate(IMementoCreator trackedObject)
        {
            _trackedItems.Add(new TrackedItem(trackedObject, ChangeTrackingOperation.Update));
        }

        public void TrackDelete(IMementoCreator trackedObject)
        {
            _trackedItems.Add(new TrackedItem(trackedObject, ChangeTrackingOperation.Delete));
        }

        public void CompleteTrackingSession()
        {
            foreach (var item in _trackedItems)
            {
                var updated = item.Operation == ChangeTrackingOperation.Delete ? null : item.TrackedEntity.CreateMemento();
                var diff = _diffGenerator.GenerateDiff(item.InitialState, updated);
                if (!diff.Any()) continue;
                var diffJson = JsonConvert.SerializeObject(diff);

                UnitOfWorkContext.AddEvent(() => new EntityStateChangedEvent
                {
                    CorrelationId = _correlationId,
                    StateChangeType = _userAction,
                    EntityType = item.InitialState == null ? updated.EntityName : item.InitialState.EntityName,
                    EntityId = item.InitialState?.Id ?? updated.Id,
                    ProviderId = _providerId,
                    EmployerAccountId = _employerAccountId,
                    InitialState = item.InitialState == null ? null : JsonConvert.SerializeObject(item.InitialState),
                    UpdatedState = updated == null ? null : JsonConvert.SerializeObject(updated),
                    Diff = diffJson,
                    UpdatedOn = DateTime.UtcNow,
                    UpdatingParty = _party,
                    UpdatingUserId = _userInfo.UserId,
                    UpdatingUserName = _userInfo.UserDisplayName
                });
            }

            _trackedItems.Clear();
        }
    }

    public enum ChangeTrackingOperation
    {
        Insert, Delete, Update
    }
}

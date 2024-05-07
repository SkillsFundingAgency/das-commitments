using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class ChangeTrackingSession : IChangeTrackingSession
    {
        private readonly IStateService _stateService;
        private readonly List<TrackedItem> _trackedItems;
        private readonly Guid _correlationId;
        private readonly UserAction _userAction;
        private readonly long _employerAccountId;
        private readonly long _providerId;
        private readonly Party _party;
        private readonly UserInfo _userInfo;
        private readonly long? _apprenticeshipId;

        public ChangeTrackingSession(IStateService stateService, UserAction userAction, Party party, long employerAccountId, long providerId, UserInfo userInfo, long? apprenticeshipId = default(long?))
        {
            _stateService = stateService;
            _userAction = userAction;
            _party = party;
            _employerAccountId = employerAccountId;
            _providerId = providerId;
            _userInfo = userInfo;
            _correlationId = Guid.NewGuid();
            _trackedItems = new List<TrackedItem>();
            _apprenticeshipId = apprenticeshipId;
        }

        public IReadOnlyList<TrackedItem> TrackedItems => _trackedItems.AsReadOnly();

        public void TrackInsert(ITrackableEntity trackedObject)
        {
            _trackedItems.Add(TrackedItem.CreateInsertTrackedItem(trackedObject));
        }

        public void TrackUpdate(ITrackableEntity trackedObject)
        {
            var initialState = _stateService.GetState(trackedObject);
            _trackedItems.Add(TrackedItem.CreateUpdateTrackedItem(trackedObject, initialState));
        }

        public void TrackDelete(ITrackableEntity trackedObject)
        {
            var initialState = _stateService.GetState(trackedObject);
            _trackedItems.Add(TrackedItem.CreateDeleteTrackedItem(trackedObject, initialState));
        }

        public void CompleteTrackingSession()
        {
            foreach (var item in _trackedItems)
            {
                UnitOfWorkContext.AddEvent(() =>
                {
                    var updated = item.Operation == ChangeTrackingOperation.Delete ? null : _stateService.GetState(item.TrackedEntity);
                    
                    var result = new EntityStateChangedEvent
                    {
                        CorrelationId = _correlationId,
                        StateChangeType = _userAction,
                        EntityType = item.TrackedEntity.GetType().Name,
                        EntityId = item.TrackedEntity.Id,
                        ProviderId = _providerId,
                        EmployerAccountId = _employerAccountId,
                        InitialState = item.InitialState == null ? null : JsonConvert.SerializeObject(item.InitialState),
                        UpdatedState = updated == null ? null : JsonConvert.SerializeObject(updated),
                        UpdatedOn = DateTime.UtcNow,
                        UpdatingParty = _party,
                        UpdatingUserId = _userInfo?.UserId ?? "Unknown",
                        UpdatingUserName = _userInfo?.UserDisplayName ?? "Unknown",
                        ApprenticeshipId = _apprenticeshipId
                    };

                    return result;
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

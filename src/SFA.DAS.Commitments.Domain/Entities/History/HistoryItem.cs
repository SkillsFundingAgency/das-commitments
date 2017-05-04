using Newtonsoft.Json;

namespace SFA.DAS.Commitments.Domain.Entities.History
{
    public class HistoryItem
    {
        private readonly HistoryChangeType _historyChangeType;

        public HistoryItem(HistoryChangeType historyChangeType, object trackedObject, string entityType, long entityId, string userId, string updatedByRole, string changeType, string updatedByName)
        {
            _historyChangeType = historyChangeType;

            TrackedObject = trackedObject;
            EntityType = entityType;
            EntityId = entityId;
            UserId = userId;
            UpdatedByRole = updatedByRole;
            ChangeType = changeType;
            UpdatedByName = updatedByName;

            if (_historyChangeType != HistoryChangeType.Insert)
            {
                OriginalState = JsonConvert.SerializeObject(TrackedObject);
            }
        }

        public string EntityType { get; }
        public long EntityId { get; }
        public string UserId { get; }
        public string UpdatedByRole { get; }
        public string ChangeType { get; }
        public string UpdatedByName { get; }
        public string OriginalState { get; }

        public string UpdatedState
        {
            get
            {
                if (_historyChangeType == HistoryChangeType.Delete)
                {
                    return null;
                }

                return JsonConvert.SerializeObject(TrackedObject);
            }
        }

        public object TrackedObject { get; }
    }
}

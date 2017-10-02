using Newtonsoft.Json;

namespace SFA.DAS.Commitments.Domain.Entities.History
{
    public class HistoryItem
    {
        private readonly HistoryChangeType _historyChangeType;

        public HistoryItem(HistoryChangeType historyChangeType, object trackedObject, long? commitmentId, long? apprenticeshipId, string userId, string updatedByRole, string changeType, string updatedByName)
        {
            _historyChangeType = historyChangeType;

            TrackedObject = trackedObject;
            CommitmentId = commitmentId;
            ApprenticeshipId = apprenticeshipId;
            UserId = userId;
            UpdatedByRole = updatedByRole;
            ChangeType = changeType;
            UpdatedByName = updatedByName;

            if (_historyChangeType != HistoryChangeType.Insert)
            {
                OriginalState = JsonConvert.SerializeObject(TrackedObject);
            }
        }

        public long? CommitmentId { get; set; }
        public long? ApprenticeshipId { get; set; }
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

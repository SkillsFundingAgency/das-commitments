using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.History;

namespace SFA.DAS.Commitments.Application.Services
{
    internal class HistoryService
    {
        private readonly IHistoryRepository _repository;
        private readonly object _trackedObject;
        private readonly HistoryItem _historyItem;
        private readonly string _originalState;

        internal HistoryService(IHistoryRepository repository, object trackedObject, string changeType, long entityId, string entityType, CallerType updatedByRole, string userId)
        {
            _repository = repository;
            _trackedObject = trackedObject;

            _historyItem = new HistoryItem
            {
                ChangeType = changeType,
                EntityId = entityId,
                EntityType = entityType,
                UpdatedByRole = updatedByRole.ToString(),
                UserId = userId
            };

            _originalState = JsonConvert.SerializeObject(_trackedObject);
        }

        internal async Task CreateInsert()
        {
            _historyItem.UpdatedState = JsonConvert.SerializeObject(_trackedObject);
            await _repository.InsertHistory(_historyItem);
        }
    }
}

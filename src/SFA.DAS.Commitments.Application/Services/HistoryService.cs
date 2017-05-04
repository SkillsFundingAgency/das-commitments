using System.Collections.Generic;
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
        private readonly List<HistoryItem> _historyItems;
        
        internal HistoryService(IHistoryRepository repository)
        {
            _repository = repository;
            _historyItems = new List<HistoryItem>();
        }

        internal void TrackInsert(object trackedObject, string changeType, long entityId, string entityType, CallerType updatedByRole, string userId, string updatedByName)
        {
            AddHistoryItem(HistoryChangeType.Insert, trackedObject, changeType, entityId, entityType, updatedByRole, userId, updatedByName);
        }

        public void TrackDelete(object trackedObject, string changeType, long entityId, string entityType, CallerType updatedByRole, string userId, string updatedByName)
        {
            AddHistoryItem(HistoryChangeType.Delete, trackedObject, changeType, entityId, entityType, updatedByRole, userId, updatedByName);
        }

        public void TrackUpdate(object trackedObject, string changeType, long entityId, string entityType, CallerType updatedByRole, string userId, string updatedByName)
        {
            AddHistoryItem(HistoryChangeType.Update, trackedObject, changeType, entityId, entityType, updatedByRole, userId, updatedByName);
        }

        public async Task Save()
        {
            await _repository.InsertHistory(_historyItems);
        }

        private void AddHistoryItem(HistoryChangeType historyChangeType, object trackedObject, string changeType, long entityId, string entityType, CallerType updatedByRole, string userId, string updatedByName)
        {
            _historyItems.Add(new HistoryItem(historyChangeType, trackedObject, entityType, entityId, userId, updatedByRole.ToString(), changeType, updatedByName));
        }
    }
}

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
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

        internal void TrackInsert(object trackedObject, string changeType, long? commitmentId, long? apprenticeshipId, CallerType updatedByRole, string userId, long? providerId, long employerAccountId, string updatedByName)
        {
            AddHistoryItem(HistoryChangeType.Insert, trackedObject, changeType, commitmentId, apprenticeshipId, updatedByRole, userId, providerId, employerAccountId, updatedByName);
        }

        public void TrackDelete(object trackedObject, string changeType, long? commitmentId, long? apprenticeshipId, CallerType updatedByRole, string userId, long? providerId, long employerAccountId, string updatedByName)
        {
            AddHistoryItem(HistoryChangeType.Delete, trackedObject, changeType, commitmentId, apprenticeshipId, updatedByRole, userId, providerId, employerAccountId, updatedByName);
        }

        public void TrackUpdate(object trackedObject, string changeType, long? commitmentId, long? apprenticeshipId, CallerType updatedByRole, string userId, long? providerId, long employerAccountId, string updatedByName)
        {
            AddHistoryItem(HistoryChangeType.Update, trackedObject, changeType, commitmentId, apprenticeshipId, updatedByRole, userId, providerId, employerAccountId, updatedByName);
        }

        public async Task Save()
        {
            await _repository.InsertHistory(_historyItems);
        }

        public async Task Save(IDbConnection connection, IDbTransaction transaction)
        {
            await _repository.InsertHistory(connection, transaction, _historyItems);
        }

        private void AddHistoryItem(HistoryChangeType historyChangeType, object trackedObject, string changeType, long? commitmentId, long? apprenticeshipId, CallerType updatedByRole, string userId, long? providerId, long employerAccountId, string updatedByName)
        {
            _historyItems.Add(new HistoryItem(historyChangeType, trackedObject, commitmentId, apprenticeshipId, userId, updatedByRole.ToString(), changeType, providerId, employerAccountId, updatedByName));
        }
    }
}

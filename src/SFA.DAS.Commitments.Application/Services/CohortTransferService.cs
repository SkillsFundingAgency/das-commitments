using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Services
{
    public class CohortTransferService : ICohortTransferService
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IHistoryRepository _historyRepository;
        private readonly IApprenticeshipEventsList _apprenticeshipEventsList;
        private readonly IApprenticeshipEventsPublisher _apprenticeshipEventsPublisher;
        private readonly ICurrentDateTime _currentDateTime;

        public CohortTransferService(ICommitmentRepository commitmentRepository, IHistoryRepository historyRepository,
            IApprenticeshipEventsList apprenticeshipEventsList,
            IApprenticeshipEventsPublisher apprenticeshipEventsPublisher, ICurrentDateTime currentDateTime)
        {
            _commitmentRepository = commitmentRepository;
            _historyRepository = historyRepository;
            _apprenticeshipEventsList = apprenticeshipEventsList;
            _apprenticeshipEventsPublisher = apprenticeshipEventsPublisher;
            _currentDateTime = currentDateTime;
        }

        public async Task ResetCommitmentTransferRejection(Commitment commitment, string userId, string userName)
        {
            if (commitment.TransferApprovalStatus != TransferApprovalStatus.TransferRejected)
            {
                return;
            }

            var historyService = new HistoryService(_historyRepository);
            historyService.TrackUpdate(commitment, CommitmentChangeType.TransferApprovalReset.ToString(), commitment.Id, null, CallerType.TransferReceiver, userId, commitment.ProviderId, commitment.EmployerAccountId, userName);

            commitment.TransferApprovalStatus = TransferApprovalStatus.Pending;
            
            await _commitmentRepository.ResetTransferApprovalStatus(commitment.Id);

            await Task.WhenAll(
                historyService.Save(),
                PublishApprenticeshipEvents(commitment)
                );
        }

        private async Task PublishApprenticeshipEvents(Commitment commitment)
        {
            if (!commitment.Apprenticeships.Any())
            {
                return;
            }

            commitment.Apprenticeships.ForEach(apprenticeship =>
            {
                _apprenticeshipEventsList.Add(commitment, apprenticeship, "APPRENTICESHIP-UPDATED",
                    _currentDateTime.Now, null);
            });

            await _apprenticeshipEventsPublisher.Publish(_apprenticeshipEventsList);           
        }
    }
}

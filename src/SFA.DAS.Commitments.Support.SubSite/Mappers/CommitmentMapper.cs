using SFA.DAS.Commitments.Support.SubSite.Extentions;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Services;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.HashingService;
using System.Linq;

namespace SFA.DAS.Commitments.Support.SubSite.Mappers
{
    public class CommitmentMapper : ICommitmentMapper
    {
        private readonly IHashingService _hashingService;
        private readonly ICommitmentStatusCalculator _statusCalculator;
        private readonly ICommitmentRules _commitmentRules;
        private readonly IApprenticeshipMapper _apprenticeshipMapper;

        public CommitmentMapper(IHashingService hashingService,
                                ICommitmentStatusCalculator statusCalculator,
                                ICommitmentRules commitmentRules,
                                IApprenticeshipMapper apprenticeshipMapper)
        {
            _hashingService = hashingService;
            _statusCalculator = statusCalculator;
            _commitmentRules = commitmentRules;
            _apprenticeshipMapper = apprenticeshipMapper;
        }

        public CommitmentSummaryViewModel MapToCommitmentSummaryViewModel(GetCohortSummaryQueryResult commitment)
        {
            var agremmentStatus = _commitmentRules.DetermineAgreementStatus(commitment.Apprenticeships);

            var status = _statusCalculator.GetStatus(commitment.EditStatus,
                                                    commitment.NumberOfApprentices,
                                                    commitment.LastAction,
                                                    agremmentStatus,
                                                    commitment.TransferSenderId,
                                                    commitment.TransferApprovalStatus);

            return new CommitmentSummaryViewModel
            {
                CohortReference = _hashingService.HashValue(commitment.CohortId),
                HashedAccountId = _hashingService.HashValue(commitment.AccountId),
                EmployerName = commitment.LegalEntityName,
                ProviderName = commitment.ProviderName,
                ProviderUkprn = commitment.ProviderId,
                CohortStatusText = status.GetEnumDescription()
            };
        }

        public CommitmentDetailViewModel MapToCommitmentDetailViewModel(GetCohortSummaryQueryResult commitment)
        {
            return new CommitmentDetailViewModel
            {
                CommitmentSummary = MapToCommitmentSummaryViewModel(commitment),
                CommitmentApprenticeships = commitment.Apprenticeships.Select(o => _apprenticeshipMapper.MapToApprenticeshipSearchItemViewModel(o))
            };
        }
    }
}
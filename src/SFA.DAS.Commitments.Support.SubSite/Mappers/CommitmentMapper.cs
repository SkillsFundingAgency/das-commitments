using SFA.DAS.Commitments.Support.SubSite.Extentions;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Services;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.HashingService;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Commitments.Support.SubSite.Mappers
{
    public class CommitmentMapper : ICommitmentMapper
    {
        private readonly IHashingService _hashingService;
        private readonly ICommitmentStatusCalculator _statusCalculator;
        private readonly IApprenticeshipMapper _apprenticeshipMapper;

        public CommitmentMapper(IHashingService hashingService,
                                ICommitmentStatusCalculator statusCalculator,
                                IApprenticeshipMapper apprenticeshipMapper)
        {
            _hashingService = hashingService;
            _statusCalculator = statusCalculator;
            _apprenticeshipMapper = apprenticeshipMapper;
        }

        public CommitmentSummaryViewModel MapToCommitmentSummaryViewModel(GetSupportCohortSummaryQueryResult commitment, GetSupportApprenticeshipQueryResult apprenticeshipQueryResult)
        {
            var agremmentStatus = DetermineAgreementStatus(apprenticeshipQueryResult.Apprenticeships);

            var status = _statusCalculator.GetStatus(commitment.EditStatus,
                                                    apprenticeshipQueryResult.Apprenticeships.Count,
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

        public CommitmentDetailViewModel MapToCommitmentDetailViewModel(GetSupportCohortSummaryQueryResult commitment, GetSupportApprenticeshipQueryResult apprenticeshipQueryResult)
        {
            return new CommitmentDetailViewModel
            {
                CommitmentSummary = MapToCommitmentSummaryViewModel(commitment, apprenticeshipQueryResult),
                CommitmentApprenticeships = apprenticeshipQueryResult.Apprenticeships.Select(o => _apprenticeshipMapper.MapToApprenticeshipSearchItemViewModel(o))
            };
        }

        private AgreementStatus DetermineAgreementStatus(List<SupportApprenticeshipDetails> apprenticeships)
        {
            var first = apprenticeships?.FirstOrDefault();

            if (first == null)
            {
                return AgreementStatus.NotAgreed;
            }

            return first.AgreementStatus;
        }
    }
}
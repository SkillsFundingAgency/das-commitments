using SFA.DAS.Commitments.Support.SubSite.Extentions;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Services;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Commitments.Support.SubSite.Mappers
{
    public class CommitmentMapper : ICommitmentMapper
    {
        private readonly IEncodingService _encodingService;
        private readonly ICommitmentStatusCalculator _statusCalculator;
        private readonly IApprenticeshipMapper _apprenticeshipMapper;

        public CommitmentMapper(IEncodingService encodingService,
                                ICommitmentStatusCalculator statusCalculator,
                                IApprenticeshipMapper apprenticeshipMapper)
        {
            _encodingService = encodingService;
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
                CohortReference = _encodingService.Encode(commitment.CohortId, EncodingType.CohortReference),
                HashedAccountId = _encodingService.Encode(commitment.AccountId, EncodingType.AccountId),
                EmployerName = commitment.LegalEntityName,
                ProviderName = commitment.ProviderName,
                ProviderUkprn = commitment.ProviderId,
                CohortStatusText = status.GetEnumDescription()
            };
        }

        public CommitmentDetailViewModel MapToCommitmentDetailViewModel(GetSupportCohortSummaryQueryResult commitment, GetSupportApprenticeshipQueryResult apprenticeshipQueryResult)
        {
            var apprenticeships = apprenticeshipQueryResult.Apprenticeships.Select(o => _apprenticeshipMapper.MapToApprenticeshipSearchItemViewModel(o));

            return new CommitmentDetailViewModel
            {
                CommitmentSummary = MapToCommitmentSummaryViewModel(commitment, apprenticeshipQueryResult),
                CommitmentApprenticeships = apprenticeships
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
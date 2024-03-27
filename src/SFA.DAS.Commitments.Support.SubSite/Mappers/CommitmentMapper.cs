using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Services;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using SFA.DAS.Commitments.Support.SubSite.Application.Queries.GetSupportApprenticeship;
using SFA.DAS.Commitments.Support.SubSite.Application.Queries.GetSupportCohortSummary;
using SFA.DAS.Commitments.Support.SubSite.Extensions;

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
            var apprenticeships = apprenticeshipQueryResult.Apprenticeships.Select(_apprenticeshipMapper.MapToApprenticeshipSearchItemViewModel);

            return new CommitmentDetailViewModel
            {
                CommitmentSummary = MapToCommitmentSummaryViewModel(commitment, apprenticeshipQueryResult),
                CommitmentApprenticeships = apprenticeships
            };
        }

        private static AgreementStatus DetermineAgreementStatus(IEnumerable<SupportApprenticeshipDetails> apprenticeships)
        {
            var first = apprenticeships?.FirstOrDefault();

            return first?.AgreementStatus ?? AgreementStatus.NotAgreed;
        }
    }
}
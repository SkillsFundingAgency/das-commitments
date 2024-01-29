using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Application.Queries.GetSupportApprenticeship;
using SFA.DAS.Commitments.Support.SubSite.Application.Queries.GetSupportCohortSummary;

namespace SFA.DAS.Commitments.Support.SubSite.Mappers
{
    public interface ICommitmentMapper
    {
        CommitmentSummaryViewModel MapToCommitmentSummaryViewModel(GetSupportCohortSummaryQueryResult commitment, GetSupportApprenticeshipQueryResult apprenticeshipQueryResult);

        CommitmentDetailViewModel MapToCommitmentDetailViewModel(GetSupportCohortSummaryQueryResult commitment, GetSupportApprenticeshipQueryResult apprenticeshipQueryResult);
    }
}
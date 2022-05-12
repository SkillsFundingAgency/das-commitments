using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohorts;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SFA.DAS.Commitments.Support.SubSite.Mappers
{
    public interface ICommitmentMapper
    {
        CommitmentSummaryViewModel MapToCommitmentSummaryViewModel(GetSupportCohortSummaryQueryResult commitment, GetSupportApprenticeshipQueryResult apprenticeshipQueryResult);

        CommitmentDetailViewModel MapToCommitmentDetailViewModel(GetSupportCohortSummaryQueryResult commitment, GetSupportApprenticeshipQueryResult apprenticeshipQueryResult);
    }
}
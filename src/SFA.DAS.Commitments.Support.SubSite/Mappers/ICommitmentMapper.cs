using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohorts;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SFA.DAS.Commitments.Support.SubSite.Mappers
{
    public interface ICommitmentMapper
    {
        CommitmentSummaryViewModel MapToCommitmentSummaryViewModel(GetCohortSummaryQueryResult commitment);

        CommitmentDetailViewModel MapToCommitmentDetailViewModel(GetCohortSummaryQueryResult commitment);
    }
}
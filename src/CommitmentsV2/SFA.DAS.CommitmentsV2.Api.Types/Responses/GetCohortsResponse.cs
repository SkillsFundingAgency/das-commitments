using System.Collections.Generic;
using System.Linq;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public sealed class GetCohortsResponse
{
    public GetCohortsResponse(IEnumerable<CohortSummary> cohorts)
    {
        Cohorts = cohorts.ToArray();
    }
    public CohortSummary[] Cohorts { get; }
}
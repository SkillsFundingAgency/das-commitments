using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public sealed class GetCohortsResponse
    {
        public GetCohortsResponse(IEnumerable<CohortSummary> cohorts)
        {
            Cohorts = cohorts.ToList();
        }
        public IList<CohortSummary> Cohorts { get; }
    }
}
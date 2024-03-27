using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohorts
{
    public class GetCohortsResult
    {
        public CohortSummary[] Cohorts { get; }

        public GetCohortsResult(IEnumerable<CohortSummary> cohorts)
        {
            Cohorts = cohorts.ToArray();
        }
    }
}

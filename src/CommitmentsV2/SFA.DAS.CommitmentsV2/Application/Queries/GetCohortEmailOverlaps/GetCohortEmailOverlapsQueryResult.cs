using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortEmailOverlaps;

public class GetCohortEmailOverlapsQueryResult
{
    public GetCohortEmailOverlapsQueryResult()
    {
        Overlaps = new List<ApprenticeshipEmailOverlap>();
    }
    public List<ApprenticeshipEmailOverlap> Overlaps { get; set; }
}
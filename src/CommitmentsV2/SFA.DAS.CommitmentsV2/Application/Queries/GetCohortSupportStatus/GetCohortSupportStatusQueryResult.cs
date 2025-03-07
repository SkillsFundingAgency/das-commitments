namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSupportStatus;

public class GetCohortSupportStatusQueryResult
{
    public long CohortId { get; set; }
    public int NoOfApprentices { get; set; }
    public string CohortStatus { get; set; }
}
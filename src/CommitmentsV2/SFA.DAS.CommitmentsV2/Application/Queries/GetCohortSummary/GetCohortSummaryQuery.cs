namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;

public class GetCohortSummaryQuery : IRequest<GetCohortSummaryQueryResult>
{
    public long CohortId { get; }

    public GetCohortSummaryQuery(long cohortId)
    {
        CohortId = cohortId;
    }
}
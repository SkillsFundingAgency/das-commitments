namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSupportStatus;

public class GetCohortSupportStatusQuery : IRequest<GetCohortSupportStatusQueryResult>
{
    public long CohortId { get; }

    public GetCohortSupportStatusQuery(long cohortId)
    {
        CohortId = cohortId;
    }
}
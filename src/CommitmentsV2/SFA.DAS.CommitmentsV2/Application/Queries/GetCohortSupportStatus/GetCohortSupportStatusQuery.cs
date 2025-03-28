namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSupportStatus;

public class GetCohortSupportStatusQuery(long cohortId) : IRequest<GetCohortSupportStatusQueryResult>
{
    public long CohortId { get; } = cohortId;
}
namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPendingLearnerChangeCount;

public class GetPendingLearnerChangeCountsForEmployerQuery : IRequest<GetPendingLearnerChangeCountsForEmployerQueryResult>
{
    public long EmployerAccountId { get; }

    public GetPendingLearnerChangeCountsForEmployerQuery(long employerAccountId)
    {
        EmployerAccountId = employerAccountId;
    }
}
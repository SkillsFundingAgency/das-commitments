using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPendingApprenticeChanges
{
    public class GetPendingApprenticeChangesQuery : IRequest<GetApprenticeshipUpdateQueryResult>
    {
        public long EmployerAccountId { get; }

        public GetPendingApprenticeChangesQuery(long employerAccountId)
        {
            EmployerAccountId = employerAccountId;
        }
    }
}
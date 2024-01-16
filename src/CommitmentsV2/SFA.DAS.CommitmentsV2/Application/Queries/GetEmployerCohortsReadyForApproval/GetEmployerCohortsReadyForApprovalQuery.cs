using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetEmployerCohortsReadyForApproval
{
    public class GetEmployerCohortsReadyForApprovalQuery : IRequest<GetEmployerCohortsReadyForApprovalQueryResults>
    {
        public long EmployerAccountId { get; }

        public GetEmployerCohortsReadyForApprovalQuery(long employerAccountId)
        {
            EmployerAccountId = employerAccountId;
        }
    }
}
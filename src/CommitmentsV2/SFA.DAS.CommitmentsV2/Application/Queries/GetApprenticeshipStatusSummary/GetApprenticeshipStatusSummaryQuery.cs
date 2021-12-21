using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary
{
    public class GetApprenticeshipStatusSummaryQuery : IRequest<GetApprenticeshipStatusSummaryQueryResults>
    {
        public long EmployerAccountId { get; }

        public GetApprenticeshipStatusSummaryQuery(long employerAccountId)
        {
            EmployerAccountId = employerAccountId;
        }
    }
}

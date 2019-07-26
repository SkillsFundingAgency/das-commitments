using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary
{
    public class GetCohortSummaryQuery : IRequest<GetCohortSummaryQueryResult>
    {
        public long CohortId { get; set; }
    }
}

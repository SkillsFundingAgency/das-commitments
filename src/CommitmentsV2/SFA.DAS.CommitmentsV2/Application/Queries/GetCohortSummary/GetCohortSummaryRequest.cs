using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary
{
    public class GetCohortSummaryRequest : IRequest<GetCohortSummaryResponse>
    {
        public long CohortId { get; set; }
    }
}

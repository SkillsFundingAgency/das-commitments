using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortDetails
{
    public class GetCohortDetailsQuery : IRequest<GetCohortDetailsQueryResult>
    {
        public long CohortId { get; }

        public GetCohortDetailsQuery(long cohortId)
        {
            CohortId = cohortId;
        }
    }
}

using MediatR;

namespace SFA.DAS.Commitments.Support.SubSite.Application.Queries.GetSupportApprenticeship
{
    public class GetSupportApprenticeshipQuery : IRequest<GetSupportApprenticeshipQueryResult>
    {
        public long? ApprenticeshipId { get; set; }
        public string Uln { get; set; }
        public long? CohortId { get; set; }
    }
}
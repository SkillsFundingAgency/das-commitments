using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship
{
    public class GetSupportApprenticeshipQuery : IRequest<GetSupportApprenticeshipQueryResult>
    {
        public long? ApprenticeshipId { get; set; }
        public string Uln { get; set; }
        public long? CohortId { get; set; }
    }
}
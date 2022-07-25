using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingApprenticeshipDetails
{
    public class GetOverlappingApprenticeshipDetailsQuery : IRequest<GetOverlappingApprenticeshipDetailsQueryResult>
    {
        public long DraftApprenticeshipId { get; set; }
        public long ProviderId { get; set; }
    }

}

using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest
{
    public class GetOverlappingTrainingDateRequestQuery : IRequest<GetOverlappingTrainingDateRequestQueryResult>
    {
        public long? ApprenticeshipId { get; }
        public long? DraftApprenticeshipId { get; set; }

        public GetOverlappingTrainingDateRequestQuery(long? apprenticeshipId = null)
        {
            ApprenticeshipId = apprenticeshipId;
        }
    }
}
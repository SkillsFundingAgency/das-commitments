using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest
{
    public class GetOverlappingTrainingDateRequestQuery : IRequest<GetOverlappingTrainingDateRequestQueryResult>
    {
        public long ApprenticeshipId { get; }

        public GetOverlappingTrainingDateRequestQuery(long apprenticeshipId)
        {
            ApprenticeshipId = apprenticeshipId;
        }
    }
}
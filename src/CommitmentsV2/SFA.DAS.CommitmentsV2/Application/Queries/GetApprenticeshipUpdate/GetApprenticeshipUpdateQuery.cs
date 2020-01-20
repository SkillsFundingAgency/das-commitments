using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate
{
    public class GetApprenticeshipUpdateQuery : IRequest<GetApprenticeshipUpdateQueryResult>
    {
        public long ApprenticeshipId { get; }

        public GetApprenticeshipUpdateQuery(long apprenticeshipId)
        {
            ApprenticeshipId = apprenticeshipId;
        }
    }
}

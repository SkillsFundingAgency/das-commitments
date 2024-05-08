namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship
{
    public class GetApprenticeshipQuery : IRequest<GetApprenticeshipQueryResult>
    {
        public long ApprenticeshipId { get; }

        public GetApprenticeshipQuery(long apprenticeshipId)
        {
            ApprenticeshipId = apprenticeshipId;
        }
    }
}

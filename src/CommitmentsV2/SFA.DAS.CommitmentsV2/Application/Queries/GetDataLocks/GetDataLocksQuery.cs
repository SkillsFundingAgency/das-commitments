namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDataLocks
{
    public class GetDataLocksQuery : IRequest<GetDataLocksQueryResult>
    {
        public long ApprenticeshipId { get; }

        public GetDataLocksQuery(long apprenticeshipId)
        {
            ApprenticeshipId = apprenticeshipId;
        }
    }
}

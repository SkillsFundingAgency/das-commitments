namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfEmployerChain
{
    public class GetChangeOfEmployerChainQuery : IRequest<GetChangeOfEmployerChainQueryResult>
    {
        public long ApprenticeshipId { get; }

        public GetChangeOfEmployerChainQuery(long apprenticeshipId)
        {
            ApprenticeshipId = apprenticeshipId;
        }
    }
}

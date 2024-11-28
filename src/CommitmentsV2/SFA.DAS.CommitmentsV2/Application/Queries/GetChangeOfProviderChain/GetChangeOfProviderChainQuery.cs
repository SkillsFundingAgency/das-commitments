namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain;

public class GetChangeOfProviderChainQuery : IRequest<GetChangeOfProviderChainQueryResult>
{
    public long ApprenticeshipId { get; }

    public GetChangeOfProviderChainQuery(long apprenticeshipId)
    {
        ApprenticeshipId = apprenticeshipId;
    }
}
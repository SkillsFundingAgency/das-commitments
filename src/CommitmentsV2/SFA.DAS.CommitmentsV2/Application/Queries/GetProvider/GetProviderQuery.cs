namespace SFA.DAS.CommitmentsV2.Application.Queries.GetProvider;

public class GetProviderQuery : IRequest<GetProviderQueryResult>
{
    public long ProviderId { get; }

    public GetProviderQuery(long providerId)
    {
        ProviderId = providerId;
    }
}
namespace SFA.DAS.CommitmentsV2.Application.Queries.GetProvider;

public class GetProviderQueryResult
{
    public long ProviderId { get; }
    public string Name { get; }

    public GetProviderQueryResult(long providerId, string name)
    {
        ProviderId = providerId;
        Name = name;
    }
}
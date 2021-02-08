namespace SFA.DAS.Commitments.Api.Types
{
    public class GetProviderResponse
    {
        public ProviderResponse Provider { get; set; }
    }

    public class ProviderResponse
    {
        public long Ukprn { get; set; }
        public string Name { get; set; }

    }
}
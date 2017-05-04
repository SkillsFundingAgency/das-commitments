namespace SFA.DAS.Commitments.Api.Client.Configuration
{
    public interface ICommitmentsApiClientConfiguration
    {
        string BaseUrl { get; set; }
        string ClientToken { get; set; }
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        string IdentifierUri { get; set; }
        string Tenant { get; set; }
    }
}

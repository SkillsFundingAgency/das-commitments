using SFA.DAS.Http.Configuration;

namespace SFA.DAS.Commitments.Api.Client.Configuration
{
    public interface ICommitmentsApiClientConfiguration: IAzureActiveDirectoryClientConfiguration, IJwtClientConfiguration
    {
        string BaseUrl { get; set; }
    }
}

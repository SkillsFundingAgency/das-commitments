namespace SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Configuration
{
    public class ProviderApprenticeshipsServiceConfiguration
    {
        public bool UseFakeIdentity { get; set; }
        public ApiConfiguration Api { get; set; }
    }

    public class ApiConfiguration
    {
        public string BaseUrl { get; set; }
        public string ClientSecret { get; set; }
    }
}
using System;
using SFA.DAS.AutoConfiguration;
using SFA.DAS.CommitmentsV2.Configuration;

namespace SFA.DAS.CommitmentsV2.LinkGeneration;

public interface ILinkGenerator
{
    string CourseManagementLink(string path);
}

public class LinkGenerator : ILinkGenerator
{
    private readonly Lazy<ProviderUrlConfiguration> _lazyProviderConfiguration;

    public LinkGenerator(IAutoConfigurationService autoConfigurationService)
    {
        _lazyProviderConfiguration = new Lazy<ProviderUrlConfiguration>(() => LoadProviderUrlConfiguration(autoConfigurationService));
    }
    
    public string ProviderApprenticeshipServiceLink(string path)
    {
        var configuration = _lazyProviderConfiguration.Value;
        var baseUrl = configuration.ProviderApprenticeshipServiceBaseUrl;

        return Action(baseUrl, path);
    }
    
    public string CourseManagementLink(string path)
    {
        var configuration = _lazyProviderConfiguration.Value;
        var baseUrl = configuration.CourseManagementBaseUrl;
        
        return Action(baseUrl, path);
    }

    private static ProviderUrlConfiguration LoadProviderUrlConfiguration(IAutoConfigurationService autoConfigurationService)
    {
        return autoConfigurationService.Get<ProviderUrlConfiguration>();
    }

    private static string Action(string baseUrl, string path)
    {
        var trimmedBaseUrl = baseUrl.TrimEnd('/');
        var trimmedPath = path.Trim('/');

        return $"{trimmedBaseUrl}/{trimmedPath}";
    }
}
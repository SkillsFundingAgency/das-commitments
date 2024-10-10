using SFA.DAS.CommitmentsV2.Configuration;

namespace SFA.DAS.CommitmentsV2.LinkGeneration;

public interface ILinkGenerator
{
    string CourseManagementLink(string path);
}

public class LinkGenerator(ProviderUrlConfiguration providerConfiguration) : ILinkGenerator
{
    public string ProviderApprenticeshipServiceLink(string path)
    {
        var baseUrl = providerConfiguration.ProviderApprenticeshipServiceBaseUrl;
        return Action(baseUrl, path);
    }

    public string CourseManagementLink(string path)
    {
        var baseUrl = providerConfiguration.CourseManagementBaseUrl;
        return Action(baseUrl, path);
    }
       
    private static string Action(string baseUrl, string path)
    {
        var trimmedBaseUrl = baseUrl.TrimEnd('/');
        var trimmedPath = path.Trim('/');

        return $"{trimmedBaseUrl}/{trimmedPath}";
    }
}
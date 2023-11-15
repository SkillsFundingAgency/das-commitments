using SFA.DAS.CommitmentsV2.Configuration;

namespace SFA.DAS.CommitmentsV2.LinkGeneration
{
    public interface ILinkGenerator
    {
        string CourseManagementLink(string path);
    }

    public class LinkGenerator : ILinkGenerator
    {
        private readonly ProviderUrlConfiguration _providerConfiguration;

        public LinkGenerator(ProviderUrlConfiguration providerConfiguration)
        {
            _providerConfiguration = providerConfiguration;
        }

        public string ProviderApprenticeshipServiceLink(string path)
        {
            var baseUrl = _providerConfiguration.ProviderApprenticeshipServiceBaseUrl;
            return Action(baseUrl, path);
        }

        public string CourseManagementLink(string path)
        {
            var baseUrl = _providerConfiguration.CourseManagementBaseUrl;
            return Action(baseUrl, path);
        }
       
        private static string Action(string baseUrl, string path)
        {
            var trimmedBaseUrl = baseUrl.TrimEnd('/');
            var trimmedPath = path.Trim('/');

            return $"{trimmedBaseUrl}/{trimmedPath}";
        }
    }
}
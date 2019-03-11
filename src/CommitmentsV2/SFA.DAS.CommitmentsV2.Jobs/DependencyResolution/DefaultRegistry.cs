using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.Providers.Api.Client;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.Jobs.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
            For<IProviderApiClient>().Use(c =>
                new ProviderApiClient(
                    c.GetInstance<ApprenticeshipInfoServiceConfiguration>().BaseUrl));
            For<Functions>();
        }
    }
}
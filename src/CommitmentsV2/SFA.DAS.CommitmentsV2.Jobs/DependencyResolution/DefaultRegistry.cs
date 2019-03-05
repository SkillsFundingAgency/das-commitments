using Microsoft.Extensions.Options;
using SFA.DAS.CommitmentsV2.Jobs.Configuration;
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
                    c.GetInstance<IOptions<ApprenticeshipInfoServiceApiConfiguration>>().Value.BaseUrl));
            For<Functions>();
        }
    }
}
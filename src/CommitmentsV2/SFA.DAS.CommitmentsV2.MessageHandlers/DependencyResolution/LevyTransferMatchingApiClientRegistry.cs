using System.Net.Http;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Infrastructure;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.DependencyResolution
{
    public class LevyTransferMatchingApiClientRegistry : Registry
    {
        public LevyTransferMatchingApiClientRegistry()
        {
            For<IAccessTokenProvider>().Use<AccessTokenProvider>();

            For<ILevyTransferMatchingApiClient>().Use<LevyTransferMatchingClient>().Ctor<HttpClient>()
                .Is(new HttpClient()).Singleton();
        }
    }
}

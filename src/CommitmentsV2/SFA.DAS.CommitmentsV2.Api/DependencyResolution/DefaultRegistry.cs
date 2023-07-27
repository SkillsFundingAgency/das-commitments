using Microsoft.Extensions.Configuration;
using SFA.DAS.CommitmentsV2.Api.Authentication;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Services;
using SFA.DAS.ProviderRelationships.Api.Client;
using SFA.DAS.ProviderRelationships.Api.Client.Http;
using SFA.DAS.ProviderUrlHelper;
using StructureMap;
using StructureMap.Building.Interception;

namespace SFA.DAS.CommitmentsV2.Api.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
            For<IDbContextFactory>().Use<SynchronizedDbContextFactory>();
            For<IAuthenticationService>().Use<AuthenticationService>().Singleton();
            For<IModelMapper>().Use<ModelMapper>();
            For<ILinkGenerator>().Use<LinkGenerator>().Singleton();

            Toggle<IProviderRelationshipsApiClient, StubProviderRelationshipsApiClient>("UseStubProviderRelationships");
            Toggle<IProviderRelationshipsApiClientFactory, StubProviderRelationshipsApiClientFactory>("UseStubProviderRelationships");
        }

        private void Toggle<TPluginType, TConcreteType>(string configurationKey) where TConcreteType : TPluginType
        {
            For<TPluginType>().InterceptWith(new FuncInterceptor<TPluginType>((c, o) => c.GetInstance<IConfiguration>().GetValue<bool>(configurationKey) ? c.GetInstance<TConcreteType>() : o));
        }
    }
}
using SFA.DAS.CommitmentsV2.Api.Authentication;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Services;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.Api.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
            For<IDbContextFactory>().Use<SynchronizedDbContextFactory>();
            For<IAuthenticationService>().Use<AuthenticationService>().Singleton();
            For<IModelMapper>().Use<ModelMapper>();
        }
    }
}
using SFA.DAS.CommitmentsV2.Data;
using StructureMap;
using SFA.DAS.CommitmentsV2.Api.Authentication;
using SFA.DAS.CommitmentsV2.Authentication;

namespace SFA.DAS.CommitmentsV2.Api.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
            For<IDbContextFactory>().Use<SynchronizedDbContextFactory>();
            For<IAuthenticationService>().Use<AuthenticationService>().Singleton();
        }
    }
}
using SFA.DAS.CommitmentsV2.Api.Authentication;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.Api.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
            For<IDbContextFactory>().Use<SynchronizedDbContextFactory>();
            For<IAuthenticationService>().Use<AuthenticationService>().Singleton();
            For<IStateService>().Use<StateService>(); //todo: move this/resolve auto
            For<IChangeTrackingSessionFactory>().Use<ChangeTrackingSessionFactory>();
        }
    }
}
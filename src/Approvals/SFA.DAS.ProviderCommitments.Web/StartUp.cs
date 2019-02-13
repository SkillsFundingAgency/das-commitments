using Microsoft.Extensions.Logging;
using Owin;
using Microsoft.Owin;
using SFA.DAS.NLog.Logger;
using SFA.DAS.ProviderCommitments.Configuration;
using SFA.DAS.ProviderCommitments.Interfaces;
using SFA.DAS.ProviderCommitments.Web;

[assembly: OwinStartup(typeof(Startup))]

namespace SFA.DAS.ProviderCommitments.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var logger = StructuremapMvc.StructureMapDependencyScope.Container.GetInstance<ILog>();
            var configService = StructuremapMvc.StructureMapDependencyScope.Container.GetInstance<IProviderCommitmentsConfigurationService>();

            AuthConfig.RegisterAuth(app, configService);

            logger.Info("Starting Provider Relationships web application");
        }
    }
}
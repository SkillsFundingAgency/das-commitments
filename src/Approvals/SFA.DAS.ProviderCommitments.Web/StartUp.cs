using Microsoft.Extensions.Logging;
using Owin;
using Microsoft.Owin;
using SFA.DAS.NLog.Logger;
using SFA.DAS.ProviderCommitments.Web;

[assembly: OwinStartup(typeof(StartUp))]

namespace SFA.DAS.ProviderCommitments.Web
{
    public class StartUp
    {
        public void Configuration(IAppBuilder app)
        {
            var logger = StructuremapMvc.StructureMapDependencyScope.Container.GetInstance<ILog>();

            logger.Info("Starting Provider Relationships web application");
        }
    }
}


using System;
using Microsoft.Extensions.Logging;
using Owin;
using Microsoft.Owin;
using SFA.DAS.ProviderCommitments.Web;

[assembly: OwinStartup(typeof(StartUp))]

namespace SFA.DAS.ProviderCommitments.Web
{


    public class StartUp
    {
        public void Configuration(IAppBuilder app)
        {
            var container = StructuremapMvc.StructureMapDependencyScope.Container;
            var logger = container.GetInstance<ILogger<StartUp>>();

            logger.LogInformation("Starting Provider Relationships web application");
        }
    }
}


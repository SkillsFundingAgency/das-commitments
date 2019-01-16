using System;
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
            var a = 1;
            a++;
            //var container = StructuremapMvc.StructureMapDependencyScope.Container;
            //var logger = container.GetInstance<ILog>();

            //logger.Info("Starting Provider Relationships web application");
        }
    }
}


using System.Web.Http;
using System.Web.Http.Dispatcher;
using Owin;
using SFA.DAS.Commitments.Api.DependencyResolution;

namespace SFA.DAS.Commitments.Api.IntegrationTests.ApiHost
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();
            WebApiConfig.Register(config);

            config.Services.Replace(typeof(IAssembliesResolver), new TestWebApiResolver());

            var container = IoC.Initialize();

            container.Configure(c => c.AddRegistry<TestRegistry>());

            config.DependencyResolver = new StructureMapWebApiDependencyResolver(container);

            appBuilder.UseWebApi(config);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.Owin.Testing;
using Owin;
using SFA.DAS.Commitments.Api.Controllers;
//using SFA.DAS.Commitments.Api.DependencyResolution;
//using SFA.DAS.Commitments.Infrastructure.Configuration;
//using SFA.DAS.Messaging.AzureServiceBus;
//using SFA.DAS.Messaging.AzureServiceBus.StructureMap;
//using SFA.DAS.NLog.Logger;
//using WebApi.StructureMap;

namespace SFA.DAS.Commitments.Api.IntegrationTests.ApiHost
{
    class IntegrationTestServer
    {
        public static HttpClient Client => Instance.TestClient;

        private static readonly IntegrationTestServer Instance = new IntegrationTestServer();

        private static TestServer _server;

        private HttpClient TestClient { get; }

        protected IntegrationTestServer()
        {
            StartServer();
            //TestClient = StartClient();
            TestClient = _server.HttpClient; // todo: remove levels of indirection if not required
        }

        void StartServer()
        {
            _server = TestServer.Create<Startup>();
        }

        public static void Shutdown()
        {
            _server.Dispose();
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();
            WebApiConfig.Register(config);

            // we need to set up structure map, coz as is, it can't ctruct the ctor
            // after updating packages, and setting to same version as api, test will no longer run (inconclusive)
            // so have undone all the package changes and we'll start from the last known working
            //todo: this is not how the cut integrates structuremap!
            //const string ServiceName = "SFA.DAS.Commitments";
            //const string ServiceVersion = "1.0";

            //config.UseStructureMap(x =>
            //{
            //    //x.Policies.Add<LoggingPolicy>();
            //    x.AddRegistry<DefaultRegistry>();
            //    x.Policies.Add<CurrentDatePolicy>();
            //    x.Policies.Add(new TopicMessagePublisherPolicy<CommitmentsApiConfiguration>(ServiceName, ServiceVersion, new NLogLogger(typeof(TopicMessagePublisher))));
            //});

            //c.AddRegistry<DefaultRegistry>();
            //c.Policies.Add<CurrentDatePolicy>();
            //c.Policies.Add(new TopicMessagePublisherPolicy<CommitmentsApiConfiguration>(ServiceName, ServiceVersion, new NLogLogger(typeof(TopicMessagePublisher))));

            config.Services.Replace(typeof(IAssembliesResolver), new TestWebApiResolver());

            appBuilder.UseWebApi(config);
        }
    }

    public class TestWebApiResolver : DefaultAssembliesResolver
    {
        public override ICollection<Assembly> GetAssemblies()
        {
            return new List<Assembly> { typeof(EmployerController).Assembly };
        }
    }

    //HttpResponseMessage response = await server.HttpClient.GetAsync("/");
    //// TODO: Validate response
    //}

    //Requests can also be constructed and submitted with the following helper methods:

    //HttpResponseMessage response = await server.CreateRequest("/")
    //.AddHeader("header1", "headervalue1")
    //.GetAsync();
}

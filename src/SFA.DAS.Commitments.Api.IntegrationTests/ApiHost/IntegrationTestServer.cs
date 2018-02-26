using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.SessionState;
using Microsoft.Azure;
using Microsoft.Owin.Testing;
using Owin;
using SFA.DAS.ApiTokens.Client;
using SFA.DAS.Commitments.Api.App_Start;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.DependencyResolution;
using SFA.DAS.NLog.Logger;
using StructureMap;

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
            //var config = GlobalConfiguration.Configuration;
            WebApiConfig.Register(config);

            //var apiKeySecret = CloudConfigurationManager.GetSetting("ApiTokenSecret");
            //var apiIssuer = CloudConfigurationManager.GetSetting("ApiIssuer");
            //var apiAudiences = CloudConfigurationManager.GetSetting("ApiAudiences").Split(' ');

            //config.MessageHandlers.Clear(); //Remove(new ApiKeyHandler("Authorization", apiKeySecret, apiIssuer, apiAudiences));

            // we need to set up structure map, coz as is, it can't ctruct the ctor

            // after updating packages, and setting to same version as api, test will no longer run (inconclusive)
            // so have undone all the package changes and we'll start from the last known working

            // setting these 3 packages back to match the version throughout the solution stops the test from being able to be run!
            // from 5.2.4 -> 5.2.3
            // presumable the new 4.0 version of Microsoft.Owin et al requires the newer versions
            //< package id = "Microsoft.AspNet.WebApi.Client" version = "5.2.3" targetFramework = "net461" />
            //< package id = "Microsoft.AspNet.WebApi.Core" version = "5.2.3" targetFramework = "net461" />
            //< package id = "Microsoft.AspNet.WebApi.Owin" version = "5.2.3" targetFramework = "net461" />
            // unfortunatly, when the test had the latest versions it complained of assembly versioning mismatch
            // so we'll try with the later versions of the 3 above, and match all the other versions
            // if that doesn't work, we'll try version redirection in app.config
            // and if that doesn't work, we'll try downgrading owin and matching all the versions

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

            //todo: other tear down stuff
            //remove structuremap not used int

            //c.AddRegistry<DefaultRegistry>();
            //c.Policies.Add<CurrentDatePolicy>();
            //c.Policies.Add(new TopicMessagePublisherPolicy<CommitmentsApiConfiguration>(ServiceName, ServiceVersion, new NLogLogger(typeof(TopicMessagePublisher))));

            config.Services.Replace(typeof(IAssembliesResolver), new TestWebApiResolver());

            //todo: need to do contents of this, but with new config
            //StructuremapMvc.Start(); //todo: call end()

            var container = IoC.Initialize();

            container.Configure(c => c.AddRegistry<TestRegistry>());

            //StructuremapWebApi.Start();
            //var container = StructuremapMvc.StructureMapDependencyScope.Container;

            config.DependencyResolver = new StructureMapWebApiDependencyResolver(container);

            appBuilder.UseWebApi(config);


            //var xxx = GlobalConfiguration.Configuration;
        }
    }

    public class TestRegistry : Registry
    {
        public TestRegistry()
        {
            var httpContext = FakeHttpContext("http://localhost");
            var httpContextBase = new System.Web.HttpContextWrapper(httpContext);
            For<HttpContextBase>().Use(httpContextBase);

            //For<ILoggingContext>().ClearAll().Use(x => new WebLoggingContext(httpContextBase));
            For<ILoggingContext>().ClearAll().Use(x => new WebLoggingContext(null));
        }

        public static HttpContext FakeHttpContext(string url)
        {
            var uri = new Uri(url);
            var httpRequest = new HttpRequest(string.Empty, uri.ToString(),
                uri.Query.TrimStart('?'));
            var stringWriter = new StringWriter();
            var httpResponse = new HttpResponse(stringWriter);
            var httpContext = new HttpContext(httpRequest, httpResponse);

            var sessionContainer = new HttpSessionStateContainer("id",
                new SessionStateItemCollection(),
                new HttpStaticObjectsCollection(),
                10, true, HttpCookieMode.AutoDetect,
                SessionStateMode.InProc, false);

            SessionStateUtility.AddHttpSessionStateToContext(
                httpContext, sessionContainer);

            return httpContext;
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

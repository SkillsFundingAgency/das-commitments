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

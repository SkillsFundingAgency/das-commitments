using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.SessionState;
using Microsoft.Owin.Testing;
using Owin;
using StructureMap;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.DependencyResolution;
using SFA.DAS.NLog.Logger;

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
            _server = TestServer.Create<Startup>();
            TestClient = _server.HttpClient;
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

            var container = IoC.Initialize();

            container.Configure(c => c.AddRegistry<TestRegistry>());

            config.DependencyResolver = new StructureMapWebApiDependencyResolver(container);

            appBuilder.UseWebApi(config);
        }
    }

    public class TestRegistry : Registry
    {
        public TestRegistry()
        {
            // OWIN self-hosting doesn't have a HttpContext, but we need one because
            // StructureMapDependencyScope uses the HttpContext to stash a NestedContainer
            // if we get the authorization working, we could change StructureMapDependencyScope
            // (also WebLoggingContext uses it, but we replace the one in the container with a null htppcontext - which it handles fine)
            var httpContext = FakeHttpContext("http://localhost");
            var httpContextBase = new HttpContextWrapper(httpContext);
            For<HttpContextBase>().Use(httpContextBase);

            //For<ILoggingContext>().ClearAll().Use(x => new WebLoggingContext(httpContextBase));
            For<ILoggingContext>().ClearAll().Use(x => new WebLoggingContext(null));
        }

        public static HttpContext FakeHttpContext(string url)
        {
            var uri = new Uri(url);
            var httpRequest = new HttpRequest(string.Empty, uri.ToString(), uri.Query.TrimStart('?'));
            var stringWriter = new StringWriter();
            var httpResponse = new HttpResponse(stringWriter);
            var httpContext = new HttpContext(httpRequest, httpResponse);

            var sessionContainer = new HttpSessionStateContainer("id",
                new SessionStateItemCollection(),
                new HttpStaticObjectsCollection(),
                10, true, HttpCookieMode.AutoDetect,
                SessionStateMode.InProc, false);

            SessionStateUtility.AddHttpSessionStateToContext(httpContext, sessionContainer);

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

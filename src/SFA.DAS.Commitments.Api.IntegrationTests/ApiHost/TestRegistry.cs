using System;
using System.IO;
using System.Web;
using System.Web.SessionState;
using SFA.DAS.NLog.Logger;
using StructureMap;

namespace SFA.DAS.Commitments.Api.IntegrationTests.ApiHost
{
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
}

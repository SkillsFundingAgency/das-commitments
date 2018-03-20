using System.Net.Http;
using Microsoft.Owin.Testing;

namespace SFA.DAS.Commitments.Api.IntegrationTests.ApiHost
{
    public class IntegrationTestServer
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
}

using System.Net;
using System.Text.RegularExpressions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SFA.DAS.CommitmentsV2.Api.FakeServers
{
    public class ProviderRelationshipsAPIMockBuilder
    {

        private readonly WireMockServer _server;

        public ProviderRelationshipsAPIMockBuilder(int port)
        {
            _server = WireMockServer.StartWithAdminInterface(port, true);
        }

        public static ProviderRelationshipsAPIMockBuilder Create(int port)
        {
            return new ProviderRelationshipsAPIMockBuilder(port);
        }

        public MockApi Build()
        {
            return new MockApi(_server);
        }

        public ProviderRelationshipsAPIMockBuilder Setup()
        {

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/accountproviderlegalentities$"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("Responses/ProviderRelationships/account-provider-legal-entities.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/permissions/has$"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("Responses/ProviderRelationships/has-permission.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/permissions/has-relationship-with$"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("Responses/ProviderRelationships/has-relationship-with-permission.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/ping"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("Responses/ProviderRelationships/ping.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/permissions/ping$"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("Responses/ProviderRelationships/ping.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/permissions/revoke$"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("Responses/ProviderRelationships/revoke-permissions.json")
                );

            return this;
        }
    }
}
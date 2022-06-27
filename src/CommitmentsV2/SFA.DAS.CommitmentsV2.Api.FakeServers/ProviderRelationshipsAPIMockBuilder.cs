using System.Net;
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
                    .WithPath("/api/accountproviderlegalentities$")
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("responses/provider-relationships/account-provider-legal-entities.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath("/api/permissions/has$")
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("responses/provider-relationships/has-permission.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath("/api/permissions/has-relationship-with$")
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("responses/provider-relationships/has-relationship-with-permission.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath("/api/ping$")
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("responses/provider-relationships/ping.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath("/api/permissions/ping$")
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("responses/provider-relationships/ping.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath("/api/permissions/revoke$")
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("responses/provider-relationships/revoke-permissions.json")
                );

            return this;
        }
    }
}
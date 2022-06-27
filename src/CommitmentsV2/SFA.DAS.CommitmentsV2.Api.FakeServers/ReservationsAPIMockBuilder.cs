using System.Net;
using System.Text.RegularExpressions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SFA.DAS.CommitmentsV2.Api.FakeServers
{
    public class ReservationsAPIMockBuilder
    {
        private readonly WireMockServer _server;

        public ReservationsAPIMockBuilder(int port)
        {
            _server = WireMockServer.StartWithAdminInterface(port, true);
        }

        public static ReservationsAPIMockBuilder Create(int port)
        {
            return new ReservationsAPIMockBuilder(port);
        }

        public MockApi Build()
        {
            return new MockApi(_server);
        }

        public ReservationsAPIMockBuilder Setup()
        {

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/accounts/\\d+/reservations"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("responses/reservations/accounts-reservations.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/accounts/\\d+/reservations/\\d+/select"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("responses/reservations/reservations-select.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath("/ping")
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("responses/reservations/reservations-select.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/api/reservations/validate/\\d+"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("responses/reservations/reservations-validate.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/api/accounts/\\d+/status"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("responses/reservations/accounts-status.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/api/reservations/accounts/\\d+/bulk-create"))
                    .UsingPost()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("responses/reservations/accounts-bulk-create.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/api/reservations/\\d+/change"))
                    .UsingPost()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("responses/reservations/reservations-change.json")
                );

            return this;
        }
    }
}

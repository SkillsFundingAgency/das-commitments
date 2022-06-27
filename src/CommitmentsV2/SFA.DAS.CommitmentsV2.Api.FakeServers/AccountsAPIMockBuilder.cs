using System.Net;
using System.Text.RegularExpressions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SFA.DAS.CommitmentsV2.Api.FakeServers
{
    public class AccountsAPIMockBuilder
    {
        private readonly WireMockServer _server;

        public AccountsAPIMockBuilder(int port)
        {
            _server = WireMockServer.StartWithAdminInterface(port, true);
        }

        public static AccountsAPIMockBuilder Create(int port)
        {
            return new AccountsAPIMockBuilder(port);
        }

        public MockApi Build()
        {
            return new MockApi(_server);
        }

        public AccountsAPIMockBuilder Setup()
        {

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/8194/legalentities/2817$"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile($"Responses/Accounts/8194/LegalEntities/2817_get.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/8194/legalentities/2818$"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("Responses/Accounts/8194/LegalEntities/2818_get.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/8194/transfers/connections$"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("Responses/Accounts/8194/Transfers/connections_get.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/8194/legalentities$"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("Responses/Accounts/8194/legalentities_get.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/8194/users$"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("Responses/Accounts/8194/users_get.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/30060/legalentities/645$"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("Responses/Accounts/30060/LegalEntities/645_get.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/30060/transfers/connections$"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("Responses/Accounts/30060/Transfers/connections_get.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/30060/legalentities$"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("Responses/Accounts/30060/legalentities_get.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/30060/users$"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("Responses/Accounts/30060/users_get.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/36853/legalentities/701$"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("Responses/Accounts/36853/Transfers/connections_get.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/36853/legalentities$"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("Responses/Accounts/36853/legalentities_get.json")
                );

            _server.Given
                (
                    Request.Create()
                    .WithPath(s => Regex.IsMatch(s, "/36853/users$"))
                    .UsingGet()
                )
                .RespondWith
                (
                    Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyFromFile("Responses/Accounts/36853/users_get.json")
                );

            return this;
        }
    }
}

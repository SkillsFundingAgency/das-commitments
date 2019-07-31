namespace SFA.DAS.Reservations.Api.Types.Configuration
{
    public class ReservationsClientApiConfiguration //: IAzureActiveDirectoryClientConfiguration
    {
        public string ApiBaseUrl { get; set; }
        public string Tenant { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string IdentifierUri { get; set; }
        public bool UseStub { get; set; }
    }
}

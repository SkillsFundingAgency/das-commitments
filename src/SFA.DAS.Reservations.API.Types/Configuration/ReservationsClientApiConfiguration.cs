namespace SFA.DAS.Reservations.Api.Types.Configuration
{
    public class ReservationsClientApiConfiguration
    {
        //public const string StubBase = "https://sfa-stub-reservations.herokuapp.com";
        public const string StubBase = "https://localhost:5003";
        public string ApiBaseUrl { get; set; }
        public string Tenant { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string IdentifierUri { get; set; }
        public bool UseStub { get; set; }
        public string EffectiveApiBaseUrl => UseStub ? StubBase : ApiBaseUrl;
    }
}

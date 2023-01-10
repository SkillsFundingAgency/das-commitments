using SFA.DAS.Http.Configuration;
using SFA.DAS.Reservations.Api.Types.Configuration;

namespace SFA.DAS.ReservationsV2.Api.Client.DependencyResolution
{
    class ReservationsClientApiConfigurationMIAdapter : IManagedIdentityClientConfiguration
    {
        private readonly ReservationsClientApiConfiguration _config;

        public ReservationsClientApiConfigurationMIAdapter(ReservationsClientApiConfiguration config)
        {
            _config = config;
        }

        public string ApiBaseUrl => _config.ApiBaseUrl;
        public string IdentifierUri => _config.IdentifierUri;
    }
}

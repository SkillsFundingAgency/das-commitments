using SFA.DAS.Http.Configuration;
using SFA.DAS.Reservations.Api.Types.Configuration;

namespace SFA.DAS.Reservations.Api.Client.DependencyResolution
{
    /// <summary>
    ///     Adapts the configuration class defined in the common reservations client type library
    ///     to an IAzureADClientConfiguration, which has been removed from newer versions of the
    ///     SFA.DAS.Http package.
    /// </summary>
    public class ReservationsClientApiConfigurationADAdapter : IAzureActiveDirectoryClientConfiguration
    {
        private readonly ReservationsClientApiConfiguration _config;

        public ReservationsClientApiConfigurationADAdapter(ReservationsClientApiConfiguration config)
        {
            _config = config;
        }
        public string Tenant => _config.Tenant;
        public string ClientId => _config.ClientId;
        public string ClientSecret => _config.ClientSecret;
        public string IdentifierUri => _config.IdentifierUri;
        public string ApiBaseUrl => _config.ApiBaseUrl;
    }
}
using SFA.DAS.Http;
using SFA.DAS.Http.Configuration;
using SFA.DAS.Reservations.Api.Types.Configuration;

namespace SFA.DAS.Reservations.Api.Client.DependencyResolution
{
    /// <summary>
    ///     Adapts the configuration class defined in the common reservations client type library
    ///     to an IAzureActiveDirectoryClientConfiguration, which is in the newer versions of the
    ///     SFA.DAS.Http package.
    /// </summary>
    public class ReservationsClientApiConfigurationADAdapter : IAzureActiveDirectoryClientConfiguration, IManagedIdentityClientConfiguration
    {
        private readonly ReservationsClientApiConfiguration _config;

        public ReservationsClientApiConfigurationADAdapter(ReservationsClientApiConfiguration config)
        {
            _config = config;
        }

        public string ApiBaseUrl => _config.ApiBaseUrl;
        public string Tenant => _config.Tenant;
        public string ClientId => _config.ClientId;
        public string ClientSecret => _config.ClientSecret;
        public string IdentifierUri => _config.IdentifierUri;
    }
}
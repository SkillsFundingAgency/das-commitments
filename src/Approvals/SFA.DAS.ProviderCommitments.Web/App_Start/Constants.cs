using SFA.DAS.ProviderCommitments.Configuration;

namespace SFA.DAS.ProviderCommitments.Web.App_Start
{
    public class WellKnownUrls
    {
        private readonly string _baseUrl;
        public IdentityServerConfiguration Configuration { get; set; }
        public WellKnownUrls(IdentityServerConfiguration configuration)
        {
            this.Configuration = configuration;
            _baseUrl = configuration.ClaimIdentifierConfiguration.ClaimsBaseUrl;
        }

        public string AuthorizeEndpoint() => $"{Configuration.BaseAddress}{Configuration.AuthorizeEndPoint}";
        public string LogoutEndpoint => $"{Configuration.BaseAddress}{Configuration.LogoutEndpoint}";
        public string TokenEndpoint => $"{Configuration.BaseAddress}{Configuration.TokenEndpoint}";
        public string UserInfoEndpoint => $"{Configuration.BaseAddress}{Configuration.UserInfoEndpoint}";
        public string ChangePasswordLink => Configuration.BaseAddress.Replace("/identity", "") + string.Format(Configuration.ChangePasswordLink, Configuration.ClientId);
        public string ChangeEmailLink => Configuration.BaseAddress.Replace("/identity", "") + string.Format(Configuration.ChangeEmailLink, Configuration.ClientId);
        public string RegisterLink => Configuration.BaseAddress.Replace("/identity", "") + string.Format(Configuration.RegisterLink, Configuration.ClientId);


        public string Id => _baseUrl + Configuration.ClaimIdentifierConfiguration.Id;
        public string Email => _baseUrl + Configuration.ClaimIdentifierConfiguration.Email;
        public string GivenName => _baseUrl + Configuration.ClaimIdentifierConfiguration.GivenName;
        public string FamilyName => _baseUrl + Configuration.ClaimIdentifierConfiguration.FaimlyName;
        public string DisplayName => _baseUrl + Configuration.ClaimIdentifierConfiguration.DisplayName;
        public string RequiresVerification => _baseUrl + "requires_verification";
    }
}
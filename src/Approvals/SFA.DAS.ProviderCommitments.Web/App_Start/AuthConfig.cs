using System;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Web.Mvc;
using Microsoft.Azure;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using NLog;
using Owin;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Configuration.FileStorage;
using SFA.DAS.EmployerUsers.WebClientComponents;
using SFA.DAS.OidcMiddleware;
using SFA.DAS.ProviderCommitments.Configuration;
using SFA.DAS.ProviderCommitments.Interfaces;
using SFA.DAS.ProviderCommitments.Web.App_Start;
using SFA.DAS.ProviderCommitments.Web.Authentication;
using SFA.DAS.ProviderCommitments.Web.Models;

namespace SFA.DAS.ProviderCommitments.Web
{
    public class AuthConfig
    {
        public static void RegisterAuth(IAppBuilder app, IProviderCommitmentsConfigurationService providerCommitmentsConfigurationService)
        {
            var logger = LogManager.GetLogger(typeof(AuthConfig).Name);

            var config = providerCommitmentsConfigurationService.GetSecurityConfiguration();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies",
                ExpireTimeSpan = new TimeSpan(0, 10, 0),
                SlidingExpiration = true
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "TempState",
                AuthenticationMode = AuthenticationMode.Passive
            });

            var wellKnownUrls = new WellKnownUrls(config.Identity);

            var urlHelper = new UrlHelper();

            UserLinksViewModel.ChangePasswordLink =
                $"{wellKnownUrls.ChangePasswordLink}{urlHelper.Encode("https://" + config.DashboardUrl + "/service/password/change")}";
            UserLinksViewModel.ChangeEmailLink =
                $"{wellKnownUrls.ChangeEmailLink}{urlHelper.Encode("https://" + config.DashboardUrl + "/service/email/change")}";

            app.UseCodeFlowAuthentication(new OidcMiddlewareOptions
            {
                ClientId = config.Identity.ClientId,
                ClientSecret = config.Identity.ClientSecret,
                Scopes = config.Identity.Scopes,
                BaseUrl = wellKnownUrls.Configuration.BaseAddress,
                TokenEndpoint = wellKnownUrls.TokenEndpoint,
                UserInfoEndpoint = wellKnownUrls.UserInfoEndpoint,
                AuthorizeEndpoint = wellKnownUrls.AuthorizeEndpoint(),
                TokenValidationMethod = config.Identity.UseCertificate
                    ? TokenValidationMethod.SigningKey
                    : TokenValidationMethod.BinarySecret,
                TokenSigningCertificateLoader = GetSigningCertificate(config.Identity.UseCertificate),
                AuthenticatedCallback = identity => { PostAuthentiationAction(identity, logger, wellKnownUrls); }
            });

            ConfigurationFactory.Current = new IdentityServerConfigurationFactory(config);
        }

        private static Func<X509Certificate2> GetSigningCertificate(bool useCertificate)
        {
            if (!useCertificate)
            {
                return null;
            }

            return () =>
            {
                var store = new X509Store(StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);
                try
                {
                    var thumbprint = CloudConfigurationManager.GetSetting("TokenCertificateThumbprint");
                    var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

                    if (certificates.Count < 1)
                    {
                        throw new Exception(
                            $"Could not find certificate with thumbprint {thumbprint} in LocalMachine store");
                    }

                    return certificates[0];
                }
                finally
                {
                    store.Close();
                }
            };
        }

        private static void PostAuthentiationAction(ClaimsIdentity identity, ILogger logger, WellKnownUrls wellKnownUrls)
        {
            logger.Info("PostAuthenticationAction called");
            var userRef = GetClaimValue(identity, wellKnownUrls.Id);
            var email = GetClaimValue(identity, wellKnownUrls.Email);
            var firstName = GetClaimValue(identity, wellKnownUrls.GivenName);
            var lastName = GetClaimValue(identity, wellKnownUrls.FamilyName);

            logger.Info($"Claims retrieved from OIDC server {userRef}, {email}, {firstName}, {lastName}");

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, GetClaimValue(identity, wellKnownUrls.Id)));
            identity.AddClaim(new Claim(ClaimTypes.Name, GetClaimValue(identity, wellKnownUrls.DisplayName)));

            identity.AddClaim(new Claim("sub", GetClaimValue(identity, wellKnownUrls.Id)));
            identity.AddClaim(new Claim("email", GetClaimValue(identity, wellKnownUrls.Email)));
        }

        private static string GetClaimValue(ClaimsIdentity identity, string claimName)
        {
            var claim = identity.Claims.FirstOrDefault(c => c.Type == claimName);

            return claim?.Value;
        }
    }
}
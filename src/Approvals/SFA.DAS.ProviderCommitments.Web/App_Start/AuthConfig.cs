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
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Configuration.FileStorage;
using SFA.DAS.EmployerUsers.WebClientComponents;
using SFA.DAS.OidcMiddleware;
using SFA.DAS.ProviderCommitments.Web.App_Start;
using SFA.DAS.ProviderCommitments.Web.Authentication;
using SFA.DAS.ProviderCommitments.Web.Models;

namespace SFA.DAS.ProviderCommitments.Web
{
    public class AuthConfig
    {
        private const string ServiceName = "SFA.DAS.ProviderCommitments";

        public static void RegisterAuth(IAppBuilder app)
        {
            var logger = LogManager.GetLogger(typeof(AuthConfig).Name);

            var config = GetConfigurationObject();

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

            var constants = new Constants(config.Identity);

            var urlHelper = new UrlHelper();

            UserLinksViewModel.ChangePasswordLink =
                $"{constants.ChangePasswordLink}{urlHelper.Encode("https://" + config.DashboardUrl + "/service/password/change")}";
            UserLinksViewModel.ChangeEmailLink =
                $"{constants.ChangeEmailLink}{urlHelper.Encode("https://" + config.DashboardUrl + "/service/email/change")}";

            app.UseCodeFlowAuthentication(new OidcMiddlewareOptions
            {
                ClientId = config.Identity.ClientId,
                ClientSecret = config.Identity.ClientSecret,
                Scopes = config.Identity.Scopes,
                BaseUrl = constants.Configuration.BaseAddress,
                TokenEndpoint = constants.TokenEndpoint,
                UserInfoEndpoint = constants.UserInfoEndpoint,
                AuthorizeEndpoint = constants.AuthorizeEndpoint(),
                TokenValidationMethod = config.Identity.UseCertificate
                    ? TokenValidationMethod.SigningKey
                    : TokenValidationMethod.BinarySecret,
                TokenSigningCertificateLoader = GetSigningCertificate(config.Identity.UseCertificate),
                AuthenticatedCallback = identity => { PostAuthentiationAction(identity, logger, constants); }
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

        private static void PostAuthentiationAction(ClaimsIdentity identity, ILogger logger, Constants constants)
        {
            logger.Info("PostAuthenticationAction called");
            var userRef = GetClaimValue(identity, constants.Id);
            var email = GetClaimValue(identity, constants.Email);
            var firstName = GetClaimValue(identity, constants.GivenName);
            var lastName = GetClaimValue(identity, constants.FamilyName);

            logger.Info($"Claims retrieved from OIDC server {userRef}, {email}, {firstName}, {lastName}");

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, GetClaimValue(identity, constants.Id)));
            identity.AddClaim(new Claim(ClaimTypes.Name, GetClaimValue(identity, constants.DisplayName)));

            identity.AddClaim(new Claim("sub", GetClaimValue(identity, constants.Id)));
            identity.AddClaim(new Claim("email", GetClaimValue(identity, constants.Email)));
        }

        private static string GetClaimValue(ClaimsIdentity identity, string claimName)
        {
            var claim = identity.Claims.FirstOrDefault(c => c.Type == claimName);

            return claim?.Value;
        }

        private static ProviderCommitmentsSecurityConfiguration GetConfigurationObject()
        {
            var environment = Environment.GetEnvironmentVariable("DASENV");
            if (string.IsNullOrEmpty(environment))
            {
                environment = CloudConfigurationManager.GetSetting("EnvironmentName");
            }

            var configurationRepository = GetConfigurationRepository();
            var configurationService = new ConfigurationService(
                configurationRepository,
                new ConfigurationOptions(ServiceName, environment, "1.0"));

            var config = configurationService.Get<ProviderCommitmentsSecurityConfiguration>();

            return config;
        }

        private static IConfigurationRepository GetConfigurationRepository()
        {
            IConfigurationRepository configurationRepository;
            if (bool.Parse(ConfigurationManager.AppSettings["LocalConfig"]))
            {
                configurationRepository = new FileStorageConfigurationRepository();
            }
            else
            {
                configurationRepository =
                    new AzureTableStorageConfigurationRepository(
                        CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString"));
            }

            return configurationRepository;
        }
    }
}
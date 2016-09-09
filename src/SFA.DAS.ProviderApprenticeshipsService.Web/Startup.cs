using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Cookies;
using Owin;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Configuration;
using SFA.DAS.ProviderApprenticeshipsService.Web.App_Start;
using WebGrease;

[assembly: OwinStartup(typeof(SFA.DAS.ProviderApprenticeshipsService.Web.Startup))]

namespace SFA.DAS.ProviderApprenticeshipsService.Web
{
    public class Startup
    {
        private const string ServiceName = "SFA.DAS.ProviderApprenticeshipsService";

        public void Configuration(IAppBuilder app)
        {
            //var config = GetConfigurationObject();

            //if (config.UseFakeIdentity)
            //{
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = "Cookies",
                    CookieName = $"{ServiceName}_auth",
                    LoginPath = new PathString("/home/FakeUserSignIn")
                });
            //}
            //else
            //{
                //var authenticationOrchestrator = StructuremapMvc.StructureMapDependencyScope.Container.GetInstance<AuthenticationOrchestraor>();
                //var logger = LogManager.GetLogger("Startup");



                //JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

                //app.UseCookieAuthentication(new CookieAuthenticationOptions
                //{
                //    AuthenticationType = "Cookies"

                //});

                //app.UseCookieAuthentication(new CookieAuthenticationOptions
                //{
                //    AuthenticationType = "TempState",
                //    AuthenticationMode = AuthenticationMode.Passive
                //});

                //var constants = new Constants(config.Identity.BaseAddress);
                //app.UseCodeFlowAuthentication(new OidcMiddlewareOptions
                //{
                //    ClientId = config.Identity.ClientId,
                //    ClientSecret = config.Identity.ClientSecret,
                //    Scopes = "openid",
                //    BaseUrl = constants.BaseAddress,
                //    TokenEndpoint = constants.TokenEndpoint(),
                //    UserInfoEndpoint = constants.UserInfoEndpoint(),
                //    AuthorizeEndpoint = constants.AuthorizeEndpoint(),
                //    AuthenticatedCallback = identity =>
                //    {
                //        PostAuthentiationAction(identity, authenticationOrchestrator, logger);
                //    }
                //});
            //}
        }

        //private static void PostAuthentiationAction(ClaimsIdentity identity, AuthenticationOrchestraor authenticationOrchestrator, ILogger logger)
        //{
        //    logger.Info("PostAuthenticationAction called");
        //    var userRef = identity.Claims.FirstOrDefault(claim => claim.Type == @"sub")?.Value;
        //    var email = identity.Claims.FirstOrDefault(claim => claim.Type == @"email")?.Value;
        //    var firstName = identity.Claims.FirstOrDefault(claim => claim.Type == @"given_name")?.Value;
        //    var lastName = identity.Claims.FirstOrDefault(claim => claim.Type == @"family_name")?.Value;
        //    logger.Info("Claims retrieved from OIDC server {0}: {1} : {2} : {3}", userRef, email, firstName, lastName);

        //    Task.Run(async () =>
        //    {
        //        await authenticationOrchestrator.SaveIdentityAttributes(userRef, email, firstName, lastName);
        //    }).Wait();

        //    //HttpContext.Current.Response.Redirect(HttpContext.Current.Request.Path, true);

        //}

        private static ProviderApprenticeshipsServiceConfiguration GetConfigurationObject()
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

            var config = configurationService.Get<ProviderApprenticeshipsServiceConfiguration>();

            return config;
        }


        private static IConfigurationRepository GetConfigurationRepository()
        {
            var connectionString = CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString");

            return new AzureTableStorageConfigurationRepository(connectionString);
        }
    }


    public class Constants
    {

        public Constants(string baseAddress)
        {
            this.BaseAddress = baseAddress;
        }
        public string BaseAddress { get; set; }

        public string AuthorizeEndpoint() => BaseAddress + "/Login/dialog/appl/oidc/wflow/authorize";
        public string LogoutEndpoint() => BaseAddress + "/connect/endsession";
        public string TokenEndpoint() => BaseAddress + "/Login/rest/appl/oidc/wflow/token";
        public string UserInfoEndpoint() => BaseAddress + "/Login/rest/appl/oidc/wflow/userinfo";
        public string IdentityTokenValidationEndpoint() => BaseAddress + "/connect/identitytokenvalidation";
        public string TokenRevocationEndpoint() => BaseAddress + "/connect/revocation";
    }
}
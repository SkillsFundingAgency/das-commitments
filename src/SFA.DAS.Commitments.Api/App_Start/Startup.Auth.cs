using Owin;
using SFA.DAS.Authentication.Extensions;
using System.Configuration;

namespace SFA.DAS.Commitments.Api
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseMixedModeAuthentication(new MixedModeAuthenticationOptions
            {
                ValidIssuers = new string[] { "http://localhost:62596" }, // ConfigurationManager.AppSettings["ApiIssuers"].Split(' '),
                ValidAudiences = new string[] { "http://localhost:62571" }, // ConfigurationManager.AppSettings["ApiAudiences"].Split(' '),
                ApiTokenSecret = "Some Super Secret", //ConfigurationManager.AppSettings["ApiTokenSecret"],
                MetadataEndpoint = "https://login.microsoftonline.com/citizenazuresfabisgov.onmicrosoft.com/v2.0/.well-known/openid-configuration"   //ConfigurationManager.AppSettings["MetadataEndpoint"]
            });
        }
    }
    
}
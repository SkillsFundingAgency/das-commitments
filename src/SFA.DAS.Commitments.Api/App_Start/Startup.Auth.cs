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
                ValidIssuers = ConfigurationManager.AppSettings["ApiIssuers"].Split(' '),
                ValidAudiences = ConfigurationManager.AppSettings["ApiAudiences"].Split(' '),
                ApiTokenSecret = ConfigurationManager.AppSettings["ApiTokenSecret"],
                MetadataEndpoint = ConfigurationManager.AppSettings["MetadataEndpoint"]
            });
        }
    }
    
}
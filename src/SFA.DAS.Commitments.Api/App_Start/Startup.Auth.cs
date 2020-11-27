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
                ConfigurationManager.AppSettings["ApiIssuers"].Split(' '),
                ConfigurationManager.AppSettings["ApiAudiences"].Split(' '),
                ConfigurationManager.AppSettings["ApiTokenSecret"],
                ConfigurationManager.AppSettings["MetadataEndpoint"]
            });
        }
    }
    
}
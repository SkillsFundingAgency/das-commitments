using Microsoft.Azure;
using Owin;
using SFA.DAS.Authentication.Extensions;

namespace SFA.DAS.Commitments.Api
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseMixedModeAuthentication(new MixedModeAuthenticationOptions
            {
                ValidIssuers = CloudConfigurationManager.GetSetting("ApiIssuers").Split(' '),
                ValidAudiences = CloudConfigurationManager.GetSetting("ApiAudiences").Split(' '),
                ApiTokenSecret = CloudConfigurationManager.GetSetting("ApiTokenSecret"),
                MetadataEndpoint = CloudConfigurationManager.GetSetting("MetadataEndpoint")
            });
        }
    }
}
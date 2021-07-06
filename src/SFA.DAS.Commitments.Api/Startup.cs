using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(SFA.DAS.Commitments.Api.Startup))]

namespace SFA.DAS.Commitments.Api
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
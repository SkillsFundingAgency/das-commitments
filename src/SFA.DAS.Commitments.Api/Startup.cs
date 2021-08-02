using System;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(SFA.DAS.Commitments.Api.Startup))]

namespace SFA.DAS.Commitments.Api
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            if (!IsDevelopment(app))
            {
                ConfigureAuth(app);
            }
        }

        private bool IsDevelopment(IAppBuilder app)
        {
            const string appModeKey = "host.AppMode";
            if (app.Properties.ContainsKey(appModeKey))
            {
                var appMode = app.Properties[appModeKey] as string;
                if (!string.IsNullOrEmpty(appMode))
                {
                    return appMode.Equals("development", StringComparison.InvariantCultureIgnoreCase);
                }
            }

            return false;
        }
    }
}
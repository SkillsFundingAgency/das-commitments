using System.Configuration;
using Microsoft.Owin;
using Microsoft.Owin.Security.ActiveDirectory;
using Owin;

[assembly: OwinStartup(typeof(SFA.DAS.Commitments.Support.SubSite.Startup))]

namespace SFA.DAS.Commitments.Support.SubSite
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseWindowsAzureActiveDirectoryBearerAuthentication(new WindowsAzureActiveDirectoryBearerAuthenticationOptions
            {
                Tenant = ConfigurationManager.AppSettings["idaTenant"],
                TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
                {
                    RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                    ValidAudiences = ConfigurationManager.AppSettings["idaAudience"].Split(',')
                }
            });
        }
    }
}
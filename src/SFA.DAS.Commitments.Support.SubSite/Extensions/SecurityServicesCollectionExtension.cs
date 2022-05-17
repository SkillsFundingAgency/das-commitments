using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Commitments.Support.SubSite.Configuration;

namespace SFA.DAS.Commitments.Support.SubSite.Extensions
{
    public static class SecurityServicesCollectionExtension
    {
        public static void AddActiveDirectoryAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var CommitmentSupportSiteConfiguartion = configuration.GetSection(CommitmentsSupportConfigurationKeys.CommitmentsSupportSubSite).Get<CommitmentSupportSiteConfiguartion>();

            services.AddAuthorization(o =>
            {
                o.AddPolicy("default", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("Default");
                });
            });

            services.AddAuthentication(auth =>
            {
                auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(auth =>
            {
                auth.Authority = $"https://login.microsoftonline.com/{CommitmentSupportSiteConfiguartion.SiteValidator.Tenant}";
                auth.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidAudiences = CommitmentSupportSiteConfiguartion.SiteValidator.Audience.Split(','),
                };
            });
        }
    }
}
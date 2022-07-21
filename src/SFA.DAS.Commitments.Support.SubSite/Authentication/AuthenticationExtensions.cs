using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Commitments.Support.SubSite.Configuration;

namespace SFA.DAS.Commitments.Support.SubSite.Authentication
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration config, bool isDevelopment = false)
        {
            var authConfiguration = config.GetSection(CommitmentsSupportConfigurationKeys.CommitmentsSupportSubSite).Get<CommitmentSupportSiteConfiguartion>();

            services.AddAuthentication(auth =>
            {
                auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(auth =>
            {
                auth.Authority = $"https://login.microsoftonline.com/{authConfiguration.SiteValidator.Tenant}";
                auth.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidAudiences = authConfiguration.SiteValidator.Audience.Split(",")
                };
            });

            services.AddSingleton<IClaimsTransformation, AzureAdScopeClaimTransformation>();

            return services;
        }
    }
}
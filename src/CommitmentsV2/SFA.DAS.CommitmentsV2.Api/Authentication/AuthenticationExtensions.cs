using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.CommitmentsV2.Api.Configuration;

namespace SFA.DAS.CommitmentsV2.Api.Authentication
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddApiAuthentication(this IServiceCollection services)
        {
            var azureActiveDirectoryConfiguration = services.BuildServiceProvider().GetService<IOptions<AzureActiveDirectoryConfiguration>>().Value;

            services.AddAuthentication(auth =>
            {
                auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(auth =>
            {
                auth.Authority = azureActiveDirectoryConfiguration.Authority;
                auth.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidAudiences = new List<string>
                    {
                        azureActiveDirectoryConfiguration.AppIdUri,
                        azureActiveDirectoryConfiguration.ClientId
                    }
                };
            });

            services.AddSingleton<IClaimsTransformation, AzureAdScopeClaimTransformation>();
            return services;
        }
    }
}

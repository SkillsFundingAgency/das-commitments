using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

namespace SFA.DAS.CommitmentsV2.Api.Authorization
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddApiAuthorization(this IServiceCollection services, IHostingEnvironment environment)
        {
            var isDevelopment = environment.IsDevelopment();

            services.AddAuthorization(x =>
            {
                x.AddPolicy("default", policy =>
                {
                    if (isDevelopment)
                    {
                        policy.Requirements.Add(new IsDevelopmentRequirement(true));
                    }
                    else
                    {
                        policy.RequireAuthenticatedUser();
                    }
                });
            });
            services.AddSingleton<IAuthorizationHandler, LocalAuthorizationHandler>();
            return services;
        }
    }
}

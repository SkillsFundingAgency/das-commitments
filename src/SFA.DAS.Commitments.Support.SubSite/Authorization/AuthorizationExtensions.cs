using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.Commitments.Support.SubSite.Authorization
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddAuthorization(this IServiceCollection services, IWebHostEnvironment environment)
        {
            var isDevelopment = environment.IsDevelopment();

            services.AddAuthorization(x =>
            {
                {
                    x.AddPolicy("default", policy =>
                    {
                        if (isDevelopment)
                            policy.AllowAnonymousUser();
                        else
                            policy.RequireAuthenticatedUser();
                    });

                    x.DefaultPolicy = x.GetPolicy("default");
                }
            });
            if (isDevelopment)
                services.AddSingleton<IAuthorizationHandler, LocalAuthorizationHandler>();
            return services;
        }
    }
}
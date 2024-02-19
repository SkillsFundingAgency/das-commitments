using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.CommitmentsV2.Api.Authorization
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddApiAuthorization(this IServiceCollection services, IWebHostEnvironment environment)
        {
            var isDevelopment = environment.IsDevelopment();

            services.AddAuthorization(x =>
            {
                {
                    x.AddPolicy("default", policy =>
                    {
                        if(isDevelopment)
                            policy.AllowAnonymousUser();
                        else
                            policy.RequireAuthenticatedUser();

                    });

                    x.AddPolicy("Provider", policy =>
                    {
                        if (isDevelopment)
                            policy.AllowAnonymousUser();
                        else
                        {
                            policy.RequireAuthenticatedUser();
                            policy.RequireRole("Provider");
                        }
                    });

                    x.AddPolicy("Employer", policy =>
                    {
                        if (isDevelopment)
                            policy.AllowAnonymousUser();
                        else
                        {
                            policy.RequireAuthenticatedUser();
                            policy.RequireRole("Employer");
                        }
                    });

                    x.DefaultPolicy = x.GetPolicy("default");
                }
            });
            if(isDevelopment)
                services.AddSingleton<IAuthorizationHandler, LocalAuthorizationHandler>();
            return services;
        }
    }
}

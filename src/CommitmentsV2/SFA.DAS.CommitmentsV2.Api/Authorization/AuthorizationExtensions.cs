using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.CommitmentsV2.Api.Authorization;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddApiAuthorization(this IServiceCollection services, IWebHostEnvironment environment)
    {
        var isDevelopment = environment.IsDevelopment();

        services.AddAuthorization(options =>
        {
            AddDefaultPolicy(options, isDevelopment);

            AddProviderPolicies(options, isDevelopment);

            AddEmployerPolicies(options, isDevelopment);

            options.DefaultPolicy = options.GetPolicy("default");
        });
            
        if (isDevelopment)
        {
            services.AddSingleton<IAuthorizationHandler, LocalAuthorizationHandler>();
        }

        return services;
    }

    private static void AddEmployerPolicies(AuthorizationOptions options, bool isDevelopment)
    {
        options.AddPolicy("Employer", policy =>
        {
            if (isDevelopment)
                policy.AllowAnonymousUser();
            else
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("Employer");
            }
        });
    }

    private static void AddProviderPolicies(AuthorizationOptions options, bool isDevelopment)
    {
        options.AddPolicy("Provider", policy =>
        {
            if (isDevelopment)
                policy.AllowAnonymousUser();
            else
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("Provider");
            }
        });
    }

    private static void AddDefaultPolicy(AuthorizationOptions options, bool isDevelopment)
    {
        options.AddPolicy("default", policy =>
        {
            if (isDevelopment)
                policy.AllowAnonymousUser();
            else
                policy.RequireAuthenticatedUser();
        });
    }
}
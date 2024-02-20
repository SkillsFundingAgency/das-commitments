using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.CommitmentsV2.Api.Authorization;

public static class PolicyNames
{
    public const string Default = "default";
    public const string Employer = nameof(Employer);
    public const string Provider = nameof(Provider);
}

public static class RoleNames
{
    public const string Employer = nameof(Employer);
    public const string Provider = nameof(Provider);
}

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

            options.DefaultPolicy = options.GetPolicy(PolicyNames.Default);
        });
            
        if (isDevelopment)
        {
            services.AddSingleton<IAuthorizationHandler, LocalAuthorizationHandler>();
        }

        return services;
    }

    private static void AddEmployerPolicies(AuthorizationOptions options, bool isDevelopment)
    {
        options.AddPolicy(PolicyNames.Employer, policy =>
        {
            if (isDevelopment)
                policy.AllowAnonymousUser();
            else
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(RoleNames.Employer);
            }
        });
    }

    private static void AddProviderPolicies(AuthorizationOptions options, bool isDevelopment)
    {
        options.AddPolicy(PolicyNames.Provider, policy =>
        {
            if (isDevelopment)
                policy.AllowAnonymousUser();
            else
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(RoleNames.Provider);
            }
        });
    }

    private static void AddDefaultPolicy(AuthorizationOptions options, bool isDevelopment)
    {
        options.AddPolicy(PolicyNames.Default, policy =>
        {
            if (isDevelopment)
                policy.AllowAnonymousUser();
            else
                policy.RequireAuthenticatedUser();
        });
    }
}
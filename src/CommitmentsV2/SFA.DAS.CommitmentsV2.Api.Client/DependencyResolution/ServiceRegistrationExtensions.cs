using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.CommitmentsV2.Api.Client.DependencyResolution;
public static class ServiceRegistrationExtensions
{

    public static IServiceCollection AddApiClientServices(this IServiceCollection services)
    {
        services.AddTransient<ICommitmentsApiClientFactory, CommitmentsApiClientFactory>();
        services.AddSingleton(x=> x.GetService<ICommitmentsApiClientFactory>().CreateClient());

        return services;
    }

}


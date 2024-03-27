using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.ReservationsV2.Api.Client.DependencyResolution;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddEmployerAccountServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IReservationsApiClient>(s =>
            s.GetService<IReservationsApiClientFactory>().CreateClient());
        services.AddTransient<IReservationsApiClientFactory, ReservationsApiClientFactory>();

        return services;
    }
}
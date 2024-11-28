using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Api.Client;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.ModelBinding;
using SFA.DAS.CommitmentsV2.Shared.Services;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Encoding;
using SFA.DAS.ProviderRelationships.Api.Client;

namespace SFA.DAS.CommitmentsV2.Shared.DependencyInjection;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services.AddTransient<IAcademicYearDateProvider, AcademicYearDateProvider>();
        services.AddTransient(typeof(ICookieStorageService<>), typeof(CookieStorageService<>));
        services.AddSingleton<ICreateCsvService, CreateCsvService>();
        services.AddSingleton<ICurrentDateTime, CurrentDateTime>();
        services.AddTransient<IModelMapper, ModelMapper>();
        services.AddTransient<IAccountApiClient, AccountApiClient>();
        services.AddTransient<IProviderRelationshipsApiClient, StubProviderRelationshipsApiClient>();
        services.AddTransient<IModelBinder, StringModelBinder>();
        services.AddTransient<IModelBinderProvider, StringModelBinderProvider>();
        services.AddEncodingServices();

        services.AddTransient<ICommitmentsApiClientFactory, CommitmentsApiClientFactory>();
        services.AddSingleton(x => x.GetService<ICommitmentsApiClientFactory>().CreateClient());

        return services;
    }

    private static IServiceCollection AddEncodingServices(this IServiceCollection services)
    {
        services.AddSingleton<IEncodingService, EncodingService>();

        return services;
    }

    public static IServiceCollection AddEmployerAccountServices(this IServiceCollection services, IConfiguration config)
    {
        if (config["UseStubAccountApiClient"] != null && config["UseStubAccountApiClient"].Equals("TRUE", StringComparison.InvariantCultureIgnoreCase))
        {
            services.AddTransient<IAccountApiClient, StubAccountApiClient>();
        }
        else
        {
            services.AddTransient<IAccountApiClient>(s => new AccountApiClient(s.GetService<AccountApiConfiguration>()));
        }

        return services;
    }
}
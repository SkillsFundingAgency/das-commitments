using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.CommitmentsV2.Application.Commands.AddHistory;
using SFA.DAS.CommitmentsV2.Caching;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.DependencyResolution;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.ExternalHandlers.Extensions;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Startup;
using SFA.DAS.Encoding;
using SFA.DAS.UnitOfWork.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.NServiceBus.Features.ClientOutbox.DependencyResolution.Microsoft;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.DependencyResolution;

public static class ServiceRegistrationExtensions
{
    public static IHostBuilder ConfigureExternalHandlerServices(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            var commitmentV2Config = context.Configuration.GetSection(CommitmentsConfigurationKeys.CommitmentsV2)
                .Get<CommitmentsV2Configuration>();

            services.AddConfigurationSections(context.Configuration);
            services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(AddHistoryCommand).Assembly));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddSingleton<IEncodingService, EncodingService>();
            services.AddSingleton<IDiffService, DiffService>();

            services.AddDefaultExternalHandlerServices();

            services
                .AddUnitOfWork()
                .AddEntityFramework(commitmentV2Config)
                .AddEntityFrameworkUnitOfWork<ProviderCommitmentsDbContext>();

            services.AddNServiceBusClientUnitOfWork();

            services.AddDasDistributedMemoryCache(context.Configuration, context.HostingEnvironment.IsDevelopment())
                .AddMemoryCache()
                .AddNServiceBus();
        });

        return hostBuilder;
    }

    public static IServiceCollection AddDefaultExternalHandlerServices(this IServiceCollection services)
    {
        services.AddTransient<IDbContextFactory, SynchronizedDbContextFactory>();

        services.AddTransient<IResolveOverlappingTrainingDateRequestService, ResolveOverlappingTrainingDateRequestService>();
        services.AddTransient<IUlnUtilisationService, UlnUtilisationService>();
        services.AddTransient<IOverlapCheckService, OverlapCheckService>();
        services.AddTransient<IEmailOverlapService, EmailOverlapService>();

        return services;
    }
}
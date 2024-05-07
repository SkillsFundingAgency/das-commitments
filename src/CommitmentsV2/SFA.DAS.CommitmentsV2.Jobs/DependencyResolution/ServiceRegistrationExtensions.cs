using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SFA.DAS.CommitmentsV2.Application.Commands.AddHistory;
using SFA.DAS.CommitmentsV2.Caching;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.DependencyResolution;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Jobs.Extensions;
using SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Services.Shared;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Startup;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.UnitOfWork.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.NServiceBus.Features.ClientOutbox.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.NServiceBus.Services;

namespace SFA.DAS.CommitmentsV2.Jobs.DependencyResolution;

public static class ServiceRegistrationExtensions
{
    public static IHostBuilder ConfigureJobsServices(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            var commitmentV2Config = context.Configuration.GetSection(CommitmentsConfigurationKeys.CommitmentsV2)
                .Get<CommitmentsV2Configuration>();

            services.AddApprovalsOuterApiServiceServices();
            services.AddConfigurationSections(context.Configuration);
            services.AddDatabaseRegistration();
            services.AddDefaultJobsServices();

            services.AddDomainServices();
            services.AddSingleton<IEncodingService, EncodingService>();
            services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(AddHistoryCommand).Assembly));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddAcademicYearDateProviderServices();
            services.AddCurrentDateTimeService(context.Configuration);
            services.AddTransient<OverlappingTrainingDateRequestAutomaticStopAfter2WeeksJob>();

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

    private static IServiceCollection AddDefaultJobsServices(this IServiceCollection services)
    {
        services.AddTransient<IAcademicYearEndExpiryProcessorService, AcademicYearEndExpiryProcessorService>();
        services.AddTransient<IAcademicYearDateProvider, AcademicYearDateProvider>();
        services.AddTransient<IEventPublisher, EventPublisher>();
        services.AddTransient<ImportProvidersJobs>();
        services.AddTransient<ImportStandardsJob>();
        services.AddTransient<AcademicYearEndExpiryProcessorJob>();
        services.AddTransient<IDbContextFactory, DbContextFactory>();

        return services;
    }
}
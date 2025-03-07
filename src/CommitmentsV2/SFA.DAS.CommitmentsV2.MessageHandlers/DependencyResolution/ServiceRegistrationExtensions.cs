﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.CommitmentsV2.Application.Commands.AddHistory;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Caching;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.DependencyResolution;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.MessageHandlers.Extensions;
using SFA.DAS.CommitmentsV2.MessageHandlers.Services;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.DependencyInjection;
using SFA.DAS.CommitmentsV2.Startup;
using SFA.DAS.Encoding;
using SFA.DAS.UnitOfWork.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.NServiceBus.DependencyResolution.Microsoft;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.DependencyResolution;

public static class ServiceRegistrationExtensions
{
    public static IHostBuilder ConfigureMessageHandlerServices(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            var commitmentV2Config = context.Configuration.GetSection(CommitmentsConfigurationKeys.CommitmentsV2)
                .Get<CommitmentsV2Configuration>();

            services.AddConfigurationSections(context.Configuration);
            services.AddCurrentDateTimeService(context.Configuration);
            services
                .AddUnitOfWork()
                .AddEntityFramework(commitmentV2Config)
                .AddEntityFrameworkUnitOfWork<ProviderCommitmentsDbContext>();

            services.AddDatabaseRegistration();
            services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(AddHistoryCommand).Assembly));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddNServiceBusUnitOfWork();
            services.AddDomainServices();
            services.AddEmployerAccountServices(context.Configuration);
            services.AddSingleton<IEncodingService, EncodingService>();
            services.AddCurrentDateTimeService(context.Configuration);
            services.AddTransient<IDiffService, DiffService>();
            services.AddEmployerAccountServices(context.Configuration);
            services.AddApprovalsOuterApiServiceServices();
            services.AddReservationsApiClient();
            services.AddDefaultMessageHandlerServices();
            services.AddMappingServices();
            services.AddAcademicYearDateProviderServices();


            services.AddDasDistributedMemoryCache(context.Configuration, context.HostingEnvironment.IsDevelopment())
                .AddMemoryCache()
                .AddNServiceBus();
        });

        return hostBuilder;
    }

    private static IServiceCollection AddDefaultMessageHandlerServices(this IServiceCollection services)
    {
        services.AddTransient<IDbContextFactory, SynchronizedDbContextFactory>();
        services.AddTransient<IFundingCapService, FundingCapService>();
        services.AddTransient<ITrainingProgrammeLookup, TrainingProgrammeLookup>();
       
        services.AddTransient<IEmailOptionalService, EmailOptionalService>();
        services.AddTransient<IFilterOutAcademicYearRollOverDataLocks, FilterOutAcademicYearRollOverDataLocks>();

        services.AddSingleton<IAuthenticationService, MessageHandlerAuthenticationService>();

		return services;
    }
}
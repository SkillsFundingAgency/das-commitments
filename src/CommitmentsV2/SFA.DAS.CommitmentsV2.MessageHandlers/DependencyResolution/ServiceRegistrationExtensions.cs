using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.Authorization.Features.DependencyResolution.Microsoft;
using SFA.DAS.CommitmentsV2.Application.Commands.AcceptApprenticeshipUpdates;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.DependencyResolution;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.DependencyInjection;
using SFA.DAS.CommitmentsV2.Startup;
using SFA.DAS.Encoding;
using SFA.DAS.UnitOfWork.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.NServiceBus.Features.ClientOutbox.DependencyResolution.Microsoft;

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
            //services.AddTransient<IMediator, Mediator>();
            //services.AddMediatR(serviceConfiguration => serviceConfiguration.RegisterServicesFromAssemblies(
            //    typeof(ApprenticeshipResendInvitationCommandHandler).Assembly,
            //    typeof(AcceptApprenticeshipUpdatesCommand).Assembly)
            //);
            //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            //services.
            services.AddNServiceBusClientUnitOfWork();

            services.AddEmployerAccountServices(context.Configuration);
            services.AddFeaturesAuthorization();
            services.AddSingleton<IEncodingService, EncodingService>();
            services.AddCurrentDateTimeService(context.Configuration);

            services.AddTransient<IDiffService, DiffService>();
            services.AddEmployerAccountServices(context.Configuration);
            services.AddReservationsApiClient();
            services.AddDomainServices();
            services.AddApprovalsOuterApiServiceServices();

            // todo wireup IPasAccountApiClient via outerAPI
            //services.AddTransient<IPasAccountApiClient, ...);

            services.AddDefaultMessageHandlerServices();

        });

        return hostBuilder;
    }

    public static IServiceCollection AddDefaultMessageHandlerServices(this IServiceCollection services)
    {
        services.AddTransient<IDbContextFactory, SynchronizedDbContextFactory>();
        services.AddTransient<IFundingCapService, FundingCapService>();
        services.AddTransient<ITrainingProgrammeLookup, TrainingProgrammeLookup>();
        services.AddTransient<ITopicClientFactory, TopicClientFactory>();
        services.AddTransient<ILegacyTopicMessagePublisher>(s =>
        {
            var config = s.GetService<CommitmentsV2Configuration>();
            return new LegacyTopicMessagePublisher(s.GetService<ITopicClientFactory>(),
                s.GetService<ILogger<LegacyTopicMessagePublisher>>(), config.MessageServiceBusConnectionString);
        });
        services.AddTransient<IEmailOptionalService, EmailOptionalService>();
        services.AddTransient<IFilterOutAcademicYearRollOverDataLocks, FilterOutAcademicYearRollOverDataLocks>();

        return services;
    }



}
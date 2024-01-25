using System.Reflection;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Authorization.Features.DependencyResolution.Microsoft;
using SFA.DAS.CommitmentsV2.Application.Commands.AddHistory;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.DependencyResolution;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.DependencyInjection;
using SFA.DAS.CommitmentsV2.Startup;
using SFA.DAS.Encoding;
using SFA.DAS.PAS.Account.Api.ClientV2.Configuration;
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
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssemblies(typeof(AddHistoryCommand).GetTypeInfo().Assembly));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
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

        });

        return hostBuilder;
    }
}
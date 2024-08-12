using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.OpenApi.Models;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.Authorization.Features.DependencyResolution.Microsoft;
using SFA.DAS.Authorization.Mvc.Extensions;
using SFA.DAS.Authorization.ProviderPermissions.DependencyResolution.Microsoft;
using SFA.DAS.CommitmentsV2.Api.Authentication;
using SFA.DAS.CommitmentsV2.Api.Authorization;
using SFA.DAS.CommitmentsV2.Api.Configuration;
using SFA.DAS.CommitmentsV2.Api.DependencyResolution;
using SFA.DAS.CommitmentsV2.Api.ErrorHandler;
using SFA.DAS.CommitmentsV2.Api.Extensions;
using SFA.DAS.CommitmentsV2.Api.Filters;
using SFA.DAS.CommitmentsV2.Api.HealthChecks;
using SFA.DAS.CommitmentsV2.Application.Commands.AddHistory;
using SFA.DAS.CommitmentsV2.Caching;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.DependencyResolution;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Infrastructure;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.DependencyInjection;
using SFA.DAS.CommitmentsV2.Startup;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Features.ClientOutbox.Data;
using SFA.DAS.Telemetry.Startup;
using SFA.DAS.UnitOfWork.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.Mvc.Extensions;
using SFA.DAS.UnitOfWork.NServiceBus.Features.ClientOutbox.DependencyResolution.Microsoft;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace SFA.DAS.CommitmentsV2.Api;
public class Startup
{
    private readonly IWebHostEnvironment _env;

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        _env = env;
        _configuration = configuration.BuildDasConfiguration();
    }

    private readonly IConfiguration _configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        var commitmentsConfiguration = _configuration.Get<CommitmentsV2Configuration>();

        services.AddLogging(builder =>
        {
            builder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
            builder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);
        });

        services.AddApiConfigurationSections(_configuration)
            .AddApiAuthentication(_configuration, _env.IsDevelopment())
            .AddApiAuthorization(_env)
            .AddMvc(o =>
            {
                o.AddAuthorization();
                o.Filters.Add<StopwatchFilterAttribute>();
            });

        services.AddFluentValidationAutoValidation();
        services.AddApiRequestValidators();
        services.AddTransient<IFluentValidationAutoValidationResultFactory, FluentValidationToApiErrorResultFactory>();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Commitments v2 API"
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        services.AddDasDistributedMemoryCache(_configuration, _env.IsDevelopment());
        services.AddDasHealthChecks(_configuration);
        services.AddMemoryCache();
        services.AddApiClients(_configuration);
        services.AddApplicationInsightsTelemetry();
        services.AddTelemetryUriRedaction("firstName,lastName,dateOfBirth,email");

        services.AddAcademicYearDateProviderServices();
        services.AddApprovalsOuterApiServiceServices();
        DAS.Authorization.DependencyResolution.Microsoft.ServiceCollectionExtensions.AddAuthorization(services);

        services.AddApprenticeshipSearchServices();
        services.AddConfigurationSections(_configuration);
        services.AddDomainServices();
        services
            .AddUnitOfWork()
            .AddEntityFramework(commitmentsConfiguration)
            .AddEntityFrameworkUnitOfWork<ProviderCommitmentsDbContext>();
        services.AddEmployerAccountServices(_configuration);
        services.AddFeaturesAuthorization();
        services.AddSingleton<IEncodingService, EncodingService>();
        services.AddDatabaseRegistration();
        services.AddCurrentDateTimeService(_configuration);
        
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(AddHistoryCommand).GetTypeInfo().Assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddReservationsApiClient();
        services.AddTransient<IStateService,StateService>();
        services.AddTransient<ICacheStorageService, CacheStorageService>();

        services.AddMappingServices();
        services.AddDefaultServices(_configuration);
        services.AddNServiceBusClientUnitOfWork();
        services.AddProviderPermissionsAuthorization();
    }

    public void ConfigureContainer(UpdateableServiceProvider serviceProvider)
    {
        serviceProvider.StartNServiceBus(_configuration.IsDevOrLocal());
        var serviceDescriptor = serviceProvider.FirstOrDefault(serv => serv.ServiceType == typeof(IClientOutboxStorageV2));
        serviceProvider.Remove(serviceDescriptor);
        serviceProvider.AddScoped<IClientOutboxStorageV2, ClientOutboxPersisterV2>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection()
            .UseApiGlobalExceptionHandler(loggerFactory.CreateLogger("Startup"))
            .UseUnauthorizedAccessExceptionHandler()
            .UseStaticFiles()
            .UseDasHealthChecks()
            .UseAuthentication()
            .UseUnitOfWork()
            .UseRouting()
            .UseAuthorization()
            .UseEndpoints(builder =>
            {
                builder.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            })
            .UseSwagger()
            .UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Commitments v2 API");
                c.RoutePrefix = string.Empty;
            });
    }
}

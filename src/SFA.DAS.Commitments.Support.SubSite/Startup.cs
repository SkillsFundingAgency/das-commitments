using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using SFA.DAS.Commitments.Support.SubSite.Application.Queries.GetSupportApprenticeship;
using SFA.DAS.Commitments.Support.SubSite.Caching;
using SFA.DAS.Commitments.Support.SubSite.DependencyResolution;
using SFA.DAS.Commitments.Support.SubSite.Extensions;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes;
using SFA.DAS.CommitmentsV2.DependencyResolution;

namespace SFA.DAS.Commitments.Support.SubSite;

public class Startup
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
            builder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);
        });

        services.AddActiveDirectoryAuthentication(_configuration);
        services.AddMvc(options =>
        {
            if (!_env.IsDevelopment())
            {
                options.Filters.Add(new AuthorizeFilter("default"));
            }
        });

        services.AddDasDistributedMemoryCache(_configuration, _env.IsDevelopment());
        services.AddMemoryCache();
        services.AddHealthChecks();

        services.AddSupportConfigurationSections(_configuration);
        services.AddDatabaseRegistration();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(GetSupportApprenticeshipQuery).GetTypeInfo().Assembly));
        services.AddTransient<IRequestHandler<GetApprenticeshipUpdateQuery, GetApprenticeshipUpdateQueryResult>, GetApprenticeshipUpdateQueryHandler>();
        services.AddTransient<IRequestHandler<GetChangeOfProviderChainQuery, GetChangeOfProviderChainQueryResult>, GetChangeOfProviderChainQueryHandler>();
        services.AddTransient<IRequestHandler<GetOverlappingTrainingDateRequestQuery, GetOverlappingTrainingDateRequestQueryResult>, GetOverlappingTrainingDateRequestQueryHandler>();
        services.AddTransient<IRequestHandler<GetPriceEpisodesQuery, GetPriceEpisodesQueryResult>, GetPriceEpisodesQueryHandler>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddSupportSiteDefaultServices(_configuration);

        services.AddApplicationInsightsTelemetry();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
            app.UseAuthentication();
        }

        app.UseStaticFiles()
            .UseRouting()
            .UseAuthorization()
            .UseEndpoints(endpoints => { endpoints.MapControllers(); })
            .UseHealthChecks("/health");
    }
}
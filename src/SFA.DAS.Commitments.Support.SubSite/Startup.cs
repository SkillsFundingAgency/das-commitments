using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        Configuration = configuration;
        _env = env;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRazorPages();

        services.AddActiveDirectoryAuthentication(Configuration);
        services.AddMvc(options =>
        {
            if (!_env.IsDevelopment())
            {
                options.Filters.Add(new AuthorizeFilter("default"));
            }
        });

        services.AddDasDistributedMemoryCache(Configuration, _env.IsDevelopment());
        services.AddMemoryCache();
        services.AddHealthChecks();
        services.AddApplicationInsightsTelemetry();

        services.AddSupportConfigurationSections(Configuration);
        services.AddDatabaseRegistration();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(GetSupportApprenticeshipQuery).GetTypeInfo().Assembly));
        services.AddTransient<IRequestHandler<GetApprenticeshipUpdateQuery, GetApprenticeshipUpdateQueryResult>, GetApprenticeshipUpdateQueryHandler>();
        services.AddTransient<IRequestHandler<GetChangeOfProviderChainQuery, GetChangeOfProviderChainQueryResult>, GetChangeOfProviderChainQueryHandler>();
        services.AddTransient<IRequestHandler<GetOverlappingTrainingDateRequestQuery, GetOverlappingTrainingDateRequestQueryResult>, GetOverlappingTrainingDateRequestQueryHandler>();
        services.AddTransient<IRequestHandler<GetPriceEpisodesQuery, GetPriceEpisodesQueryResult>, GetPriceEpisodesQueryHandler>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddSupportSiteDefaultServices(Configuration);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapControllers();
        });

        app.UseHealthChecks("/health");
    }
}
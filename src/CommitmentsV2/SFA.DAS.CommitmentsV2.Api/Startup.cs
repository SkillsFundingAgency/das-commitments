using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Authentication;
using SFA.DAS.CommitmentsV2.Api.Authorization;
using SFA.DAS.CommitmentsV2.Api.Configuration;
using SFA.DAS.CommitmentsV2.Api.DependencyResolution;
using SFA.DAS.CommitmentsV2.Api.ErrorHandler;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.Api
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            _env = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddApiConfigurationSections(Configuration)
                .AddApiAuthentication()
                .AddApiAuthorization(_env);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddHealthChecks();

            var container = CreateStructureMapContainer(services);

            return container.GetInstance<IServiceProvider>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
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
                .UseAuthentication()
                .UseMvc()
                .UseHealthChecks("/api/health-check");
        }
        private static Container CreateStructureMapContainer(IServiceCollection services)
        {
            var container = new Container();
            container.Configure(config =>
            {
                config.AddRegistry(new DefaultRegistry());
                config.Populate(services);
            });

            return container;
        }
    }
}
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.Authorization.Mvc;
using SFA.DAS.CommitmentsV2.Api.Attributes;
using SFA.DAS.CommitmentsV2.Api.Authentication;
using SFA.DAS.CommitmentsV2.Api.Authorization;
using SFA.DAS.CommitmentsV2.Api.Configuration;
using SFA.DAS.CommitmentsV2.Api.DependencyResolution;
using SFA.DAS.CommitmentsV2.Api.ErrorHandler;
using SFA.DAS.CommitmentsV2.Api.NServiceBus;
using SFA.DAS.CommitmentsV2.Validators;
using SFA.DAS.UnitOfWork.Mvc;
using StructureMap;
using Swashbuckle.AspNetCore.Swagger;

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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApiConfigurationSections(Configuration)
                .AddApiAuthentication(Configuration)
                .AddApiAuthorization(_env)
                .Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; })
                .AddMvc(o =>
                {
                    o.AddAuthorization();
                    o.Filters.Add<ValidateModelAttribute>();
                })
                .AddFluentValidation(c => c.RegisterValidatorsFromAssemblyContaining<CreateCohortRequestValidator>())
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Commitments v2 API"
                });
            });
            services.AddHealthChecks();
            services.AddMemoryCache();
            services.AddNServiceBus();
        }

        public void ConfigureContainer(Registry registry)
        {
            IoC.Initialize(registry);
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
                .UseUnauthorizedAccessExceptionHandler()
                .UseAuthentication()
                .UseHealthChecks("/api/health-check")
                .UseUnitOfWork()
                .UseMvc()
                .UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Commitments v2 API");
                c.RoutePrefix = string.Empty;
            });
        }
    }
}
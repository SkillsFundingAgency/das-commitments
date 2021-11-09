using System;
using System.IO;
using System.Reflection;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.Authorization.Mvc.Extensions;
using SFA.DAS.CommitmentsV2.Api.Authentication;
using SFA.DAS.CommitmentsV2.Api.Authorization;
using SFA.DAS.CommitmentsV2.Api.Configuration;
using SFA.DAS.CommitmentsV2.Api.DependencyResolution;
using SFA.DAS.CommitmentsV2.Api.ErrorHandler;
using SFA.DAS.CommitmentsV2.Api.Filters;
using SFA.DAS.CommitmentsV2.Api.HealthChecks;
using SFA.DAS.CommitmentsV2.Api.NServiceBus;
using SFA.DAS.CommitmentsV2.Caching;
using SFA.DAS.CommitmentsV2.Validators;
using SFA.DAS.UnitOfWork.Mvc.Extensions;
using StructureMap;
using Swashbuckle.AspNetCore.Swagger;

namespace SFA.DAS.CommitmentsV2.Api
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
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
                    o.Filters.Add<ValidateModelStateFilter>();
                    o.Filters.Add<StopwatchFilter>();
                })
                .AddFluentValidation(c => c.RegisterValidatorsFromAssemblyContaining<CreateCohortRequestValidator>())
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Commitments v2 API"
                });

                c.DescribeAllEnumsAsStrings();

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddDasDistributedMemoryCache(Configuration, _env.IsDevelopment());
            services.AddDasHealthChecks(Configuration);
            services.AddMemoryCache();
            services.AddNServiceBus();
            services.AddApiClients(Configuration);
        }

        public void ConfigureContainer(Registry registry)
        {
            IoC.Initialize(registry);
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
                .UseEndpoints(builder =>
                {
                    builder.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");
                })
                .UseSwagger(c =>
                {
                    c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
                })
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Commitments v2 API");
                    c.RoutePrefix = string.Empty;
                });
        }
    }
}
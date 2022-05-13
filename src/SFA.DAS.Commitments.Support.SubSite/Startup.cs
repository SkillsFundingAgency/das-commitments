using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Commitments.Support.SubSite.Authentication;
using SFA.DAS.Commitments.Support.SubSite.DependencyResolution;
using SFA.DAS.Commitments.Support.SubSite.Filters;
using SFA.DAS.Authorization.Mvc.Extensions;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Support.SubSite.Authorization;
using SFA.DAS.Commitments.Support.SubSite.Caching;
using FluentValidation.AspNetCore;
using SFA.DAS.CommitmentsV2.Validators;
using SFA.DAS.Commitments.Support.SubSite.Validation;
using SFA.DAS.Commitments.Support.SubSite.Configuration;

namespace SFA.DAS.Commitments.Support.SubSite
{
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
            services.AddApplicationInsightsTelemetry();
            services.AddRazorPages();

            services
               .AddConfigurationSections(Configuration)
               .AddAuthentication(Configuration, _env.IsDevelopment())
               .AddAuthorization(_env)
               .Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; })
               .AddMvc(o =>
               {
                   o.AddAuthorization();
                   o.Filters.Add<ValidateModelStateFilter>();
                   o.Filters.Add<StopwatchFilter>();
               })
               .AddFluentValidation(c => c.RegisterValidatorsFromAssemblyContaining<CreateCohortRequestValidator>())
               .AddFluentValidation(c => c.RegisterValidatorsFromAssemblyContaining<ApprenticeshipsSearchQueryValidator>())
               .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddDasDistributedMemoryCache(Configuration, _env.IsDevelopment());
            services.AddMemoryCache();
        }

        public void ConfigureContainer(Registry registry)
        {
            IoC.Initialize(registry);
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
            }

            app.UseHttpsRedirection()
               //.UseApiGlobalExceptionHandler(loggerFactory.CreateLogger("Startup"))
               // .UseUnauthorizedAccessExceptionHandler()
               .UseStaticFiles()
               //.UseDasHealthChecks()
               .UseAuthentication()
               .UseRouting()
               .UseAuthorization()
               .UseEndpoints(builder =>
               {
                   builder.MapControllerRoute(
                       name: "default",
                       pattern: "{controller=Status}/{action=Get}/{id?}");
               });

            //app.UseStaticFiles();

            //app.UseRouting();

            //app.UseAuthorization();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapRazorPages();
            //    endpoints.MapControllers();
            //});
        }
    }
}
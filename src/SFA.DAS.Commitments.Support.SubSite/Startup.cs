using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Commitments.Support.SubSite.Authentication;
using SFA.DAS.Commitments.Support.SubSite.DependencyResolution;
using SFA.DAS.Authorization.Mvc.Extensions;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Support.SubSite.Caching;
using FluentValidation.AspNetCore;
using SFA.DAS.CommitmentsV2.Validators;
using SFA.DAS.Commitments.Support.SubSite.Validation;
using SFA.DAS.Commitments.Support.SubSite.Configuration;
using SFA.DAS.Commitments.Support.SubSite.Extensions;
using Microsoft.AspNetCore.Mvc.Authorization;

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
}
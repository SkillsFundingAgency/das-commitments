using System;
using System.Data.Common;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Commitments.Support.SubSite.Core.Configuration;
using SFA.DAS.Commitments.Support.SubSite.Core.Models;
using SFA.DAS.Commitments.Support.SubSite.Core.Orchestrators;
using SFA.DAS.Commitments.Support.SubSite.Core.Validation;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping.Apprenticeships;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Services;
using SFA.DAS.Configuration;
using SFA.DAS.Encoding;

namespace SFA.DAS.Commitments.Support.SubSite.Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            RegisterServices(services);

            services
                .AddFluentValidation(c => c.RegisterValidatorsFromAssemblyContaining<ApprenticeshipsSearchQueryValidator>())
                //.AddAuthorization()
                .AddControllersWithViews();
            //services.AddMediatR(typeof(GetApprenticeshipsQuery).Assembly);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IUlnValidator, UlnValidator>();
            //todo: below line doesn't belong here. ideally, push this into a registry in the package itself, or an extension thereof
            services.AddSingleton<Learners.Validators.IUlnValidator, Learners.Validators.UlnValidator>();
            services.AddScoped<IApprenticeshipsOrchestrator, ApprenticeshipsOrchestrator>();
            services.AddSingleton<IValidator<ApprenticeshipSearchQuery>, ApprenticeshipsSearchQueryValidator>();

            services.AddSingleton<EncodingConfig>(c => c.GetService<IConfiguration>().GetSection(CommitmentsConfigurationKeys.EncodingConfiguration).Get<EncodingConfig>());
            services.AddSingleton<CommitmentSupportSiteConfiguartion>(c => c.GetService<IConfiguration>().GetSection("SFA.DAS.Support.Commitments").Get<CommitmentSupportSiteConfiguartion>());
            services.AddSingleton<IEncodingService, EncodingService>();
            services.AddScoped<IMediator, Mediator>();

            services.AddScoped<ServiceFactory>(ctx => ctx.GetService);

            services.AddScoped<IRequestHandler<GetApprenticeshipQuery, GetApprenticeshipQueryResult>, GetApprenticeshipQueryHandler>();
            services.AddScoped<IRequestHandler<GetApprenticeshipsQuery, GetApprenticeshipsQueryResult>, GetApprenticeshipsQueryHandler>();
            services.AddScoped<IRequestHandler<GetCohortSummaryQuery, GetCohortSummaryQueryResult>, GetCohortSummaryQueryHandler>();

            const string AzureResource = "https://database.windows.net/";

            var environmentName = "LOCAL";

            services.AddTransient<DbConnection>(c =>
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();

                return environmentName.Equals("l", StringComparison.CurrentCultureIgnoreCase)
                    ? new SqlConnection(GetConnectionString(c))
                    : new SqlConnection
                    {
                        ConnectionString = GetConnectionString(c),
                        AccessToken = azureServiceTokenProvider.GetAccessTokenAsync(AzureResource).Result
                    };
            });

            services.AddTransient<IDbContextFactory, DbContextFactory>();
            services.AddTransient<ProviderCommitmentsDbContext>(c => c.GetService<IDbContextFactory>().CreateDbContext());
            services.AddTransient<IProviderCommitmentsDbContext>(c => c.GetService<IDbContextFactory>().CreateDbContext());

            services.AddTransient(typeof(Lazy<>), typeof(Lazy<>));

            services.AddScoped<IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails>, ApprenticeshipToApprenticeshipDetailsMapper>();
            services.AddSingleton<ICurrentDateTime, CurrentDateTime>();
            services.AddTransient<IApprenticeshipSearch, ApprenticeshipSearch>();
            services.AddTransient<IApprenticeshipSearchService<ApprenticeshipSearchParameters>, ApprenticeshipSearchService>();
            services.AddTransient<IEmailOptionalService, EmailOptionalService>(c => new EmailOptionalService(new EmailOptionalConfiguration()));
        }

        private string GetConnectionString(IServiceProvider context)
        {
            return context.GetService<CommitmentSupportSiteConfiguartion>().DatabaseConnectionString;
        }
    }
}
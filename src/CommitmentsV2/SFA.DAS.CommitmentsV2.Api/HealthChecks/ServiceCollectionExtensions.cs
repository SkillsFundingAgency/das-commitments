using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Api.Extensions;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.Configuration;

namespace SFA.DAS.CommitmentsV2.Api.HealthChecks
{
    public static class ServiceCollectionExtensions
    {
        private const string AzureResource = "https://database.windows.net/";

        public static IServiceCollection AddDasHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var environmentName = Environment.GetEnvironmentVariable(EnvironmentVariableNames.EnvironmentName);
            var databaseConnectionString = configuration.GetValue<string>(CommitmentsConfigurationKeys.DatabaseConnectionString);
            
            void BeforeOpen(SqlConnection connection)
            {
                if (!environmentName.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
                {
                    var azureServiceTokenProvider = new AzureServiceTokenProvider();
                    connection.AccessToken = azureServiceTokenProvider.GetAccessTokenAsync(AzureResource).Result;
                }
            }

            services.AddHealthChecks()
                .AddCheck<NServiceBusHealthCheck>("Service Bus Health Check")
                .AddCheck<ReservationsApiHealthCheck>("Reservations API Health Check")
                .AddSqlServer(databaseConnectionString, name: "Commitments DB Health Check", beforeOpenConnectionConfigurer: BeforeOpen);

            return services;
        }
        
        public static IApplicationBuilder UseDasHealthChecks(this IApplicationBuilder app)
        {
            return app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = (c, r) => c.Response.WriteJsonAsync(new
                {
                    r.Status,
                    r.TotalDuration,
                    Results = r.Entries.ToDictionary(
                        e => e.Key,
                        e => new
                        {
                            e.Value.Status,
                            e.Value.Duration,
                            e.Value.Description,
                            e.Value.Data
                        })
                })
            });
        }
    }
}
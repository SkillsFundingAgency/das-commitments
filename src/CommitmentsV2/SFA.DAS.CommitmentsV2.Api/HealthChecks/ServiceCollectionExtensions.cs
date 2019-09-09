using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Api.Extensions;
using SFA.DAS.CommitmentsV2.Configuration;

namespace SFA.DAS.CommitmentsV2.Api.HealthChecks
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDasHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var databaseConnectionString = configuration.GetValue<string>(CommitmentsConfigurationKeys.DatabaseConnectionString);
            
            services.AddHealthChecks()
                .AddCheck<NServiceBusHealthCheck>("Service Bus Health Check")
                .AddCheck<ReservationsApiHealthCheck>("Reservations API Health Check")
                .AddSqlServer(databaseConnectionString, name: "Commitments DB Health Check");

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
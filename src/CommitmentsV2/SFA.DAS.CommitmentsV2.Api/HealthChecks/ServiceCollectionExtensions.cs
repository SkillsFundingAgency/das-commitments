using System.Data.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using SFA.DAS.CommitmentsV2.Api.Extensions;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Extensions;

namespace SFA.DAS.CommitmentsV2.Api.HealthChecks;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDasHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseConnectionString = configuration.GetValue<string>(CommitmentsConfigurationKeys.DatabaseConnectionString);

        void BeforeOpen(DbConnection connection)
        {
            {
                connection = DatabaseExtensions.GetSqlConnection(databaseConnectionString);
            }
        }

        services.AddHealthChecks()
            .AddCheck<NServiceBusHealthCheck>("Service Bus Health Check")
            .AddCheck<ReservationsApiHealthCheck>("Reservations API Health Check")
            .AddSqlServer(databaseConnectionString, name: "Commitments DB Health Check", configure: BeforeOpen);

        return services;
    }
        
    public static IApplicationBuilder UseDasHealthChecks(this IApplicationBuilder app)
    {
        return app.UseHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = (httpContext, healthReport) => httpContext.Response.WriteJsonAsync(new
            {
                healthReport.Status,
                healthReport.TotalDuration,
                Results = healthReport.Entries.ToDictionary(
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
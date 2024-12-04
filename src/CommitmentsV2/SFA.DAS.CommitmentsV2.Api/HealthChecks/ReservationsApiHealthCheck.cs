using Microsoft.Extensions.Diagnostics.HealthChecks;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.CommitmentsV2.Api.HealthChecks;

public class ReservationsApiHealthCheck(IReservationsApiClient reservationsApiClient) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await reservationsApiClient.Ping(cancellationToken);
                
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Degraded(exception.Message);
        }
    }
}
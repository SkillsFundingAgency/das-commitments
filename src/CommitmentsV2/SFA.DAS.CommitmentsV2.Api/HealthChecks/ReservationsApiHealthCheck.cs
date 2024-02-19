using Microsoft.Extensions.Diagnostics.HealthChecks;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.CommitmentsV2.Api.HealthChecks
{
    public class ReservationsApiHealthCheck : IHealthCheck
    {
        private readonly IReservationsApiClient _reservationsApiClient;

        public ReservationsApiHealthCheck(IReservationsApiClient reservationsApiClient)
        {
            _reservationsApiClient = reservationsApiClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _reservationsApiClient.Ping(cancellationToken);
                
                return HealthCheckResult.Healthy();
            }
            catch (Exception exception)
            {
                return HealthCheckResult.Degraded(exception.Message);
            }
        }
    }
}
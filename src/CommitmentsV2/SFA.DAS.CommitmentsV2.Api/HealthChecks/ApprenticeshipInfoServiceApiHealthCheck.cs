using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SFA.DAS.Providers.Api.Client;

namespace SFA.DAS.CommitmentsV2.Api.HealthChecks
{
    public class ApprenticeshipInfoServiceApiHealthCheck : IHealthCheck
    {
        private readonly IProviderApiClient _providerApiClient;

        public ApprenticeshipInfoServiceApiHealthCheck(IProviderApiClient providerApiClient)
        {
            _providerApiClient = providerApiClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _providerApiClient.Ping();
                
                return HealthCheckResult.Healthy();
            }
            catch (Exception exception)
            {
                return HealthCheckResult.Degraded(exception.Message);
            }
        }
    }
}
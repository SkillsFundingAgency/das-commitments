using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Api.Types.Providers;
using SFA.DAS.CommitmentsV2.Api.HealthChecks;
using SFA.DAS.Providers.Api.Client;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.HealthChecks
{
    [TestFixture]
    [Parallelizable]
    public class ApprenticeshipInfoServiceHealthCheckTests
    {
        private ApprenticeshipInfoServiceHealthCheckTestsFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new ApprenticeshipInfoServiceHealthCheckTestsFixture();
        }
        
        [Test]
        public async Task CheckHealthAsync_WhenFindAllAsyncSucceeds_ThenShouldReturnHealthyStatus()
        {
            var healthCheckResult = await _fixture.SetFindAllAsyncSuccess().CheckHealthAsync();
            
            Assert.AreEqual(HealthStatus.Healthy, healthCheckResult.Status);
        }
        
        [Test]
        public async Task CheckHealthAsync_WhenFindAllAsyncFails_ThenShouldReturnDegradedStatus()
        {
            var healthCheckResult = await _fixture.SetFindAllAsyncFailure().CheckHealthAsync();
            
            Assert.AreEqual(HealthStatus.Degraded, healthCheckResult.Status);
            Assert.AreEqual(_fixture.Exception.Message, healthCheckResult.Description);
        }

        private class ApprenticeshipInfoServiceHealthCheckTestsFixture
        {
            public HealthCheckContext HealthCheckContext { get; set; }
            public CancellationToken CancellationToken { get; set; }
            public Mock<IProviderApiClient> ProviderApiClient { get; set; }
            public ApprenticeshipInfoServiceApiHealthCheck HealthCheck { get; set; }
            public Exception Exception { get; set; }

            public ApprenticeshipInfoServiceHealthCheckTestsFixture()
            {
                HealthCheckContext = new HealthCheckContext
                {
                    Registration = new HealthCheckRegistration("Foo", Mock.Of<IHealthCheck>(), null, null)
                };
                
                ProviderApiClient = new Mock<IProviderApiClient>();
                HealthCheck = new ApprenticeshipInfoServiceApiHealthCheck(ProviderApiClient.Object);
                Exception = new Exception("Foobar");
            }

            public Task<HealthCheckResult> CheckHealthAsync()
            {
                return HealthCheck.CheckHealthAsync(HealthCheckContext, CancellationToken);
            }

            public ApprenticeshipInfoServiceHealthCheckTestsFixture SetFindAllAsyncSuccess()
            {
                ProviderApiClient.Setup(c => c.FindAllAsync()).ReturnsAsync(new List<ProviderSummary>());
                
                return this;
            }

            public ApprenticeshipInfoServiceHealthCheckTestsFixture SetFindAllAsyncFailure()
            {
                ProviderApiClient.Setup(c => c.FindAllAsync()).ThrowsAsync(Exception);
                
                return this;
            }
        }
    }
}
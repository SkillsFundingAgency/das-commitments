using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.HealthChecks;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.HealthChecks
{
    [TestFixture]
    [Parallelizable]
    public class ReservationsApiHealthCheckTests
    {
        private ReservationsApiHealthCheckTestsFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new ReservationsApiHealthCheckTestsFixture();
        }

        [Test]
        public async Task CheckHealthAsync_WhenPingSucceeds_ThenShouldReturnHealthyStatus()
        {
            var healthCheckResult = await _fixture.SetPingSuccess().CheckHealthAsync();

            Assert.That(healthCheckResult.Status, Is.EqualTo(HealthStatus.Healthy));
        }
        
        [Test]
        public async Task CheckHealthAsync_WhenPingFails_ThenShouldReturnDegradedStatus()
        {
            var healthCheckResult = await _fixture.SetPingFailure().CheckHealthAsync();

            Assert.That(healthCheckResult.Status, Is.EqualTo(HealthStatus.Degraded));
            Assert.That(healthCheckResult.Description, Is.EqualTo(_fixture.Exception.Message));
        }

        private class ReservationsApiHealthCheckTestsFixture
        {
            public HealthCheckContext HealthCheckContext { get; set; }
            public CancellationToken CancellationToken { get; set; }
            public Mock<IReservationsApiClient> ReservationsApiClient { get; set; }
            public ReservationsApiHealthCheck HealthCheck { get; set; }
            public Exception Exception { get; set; }

            public ReservationsApiHealthCheckTestsFixture()
            {
                HealthCheckContext = new HealthCheckContext
                {
                    Registration = new HealthCheckRegistration("Foo", Mock.Of<IHealthCheck>(), null, null)
                };
                
                ReservationsApiClient = new Mock<IReservationsApiClient>();
                HealthCheck = new ReservationsApiHealthCheck(ReservationsApiClient.Object);
                Exception = new Exception("Foobar");
            }

            public Task<HealthCheckResult> CheckHealthAsync()
            {
                return HealthCheck.CheckHealthAsync(HealthCheckContext, CancellationToken);
            }

            public ReservationsApiHealthCheckTestsFixture SetPingSuccess()
            {
                ReservationsApiClient.Setup(c => c.Ping(CancellationToken)).Returns(Task.CompletedTask);
                
                return this;
            }

            public ReservationsApiHealthCheckTestsFixture SetPingFailure()
            {
                ReservationsApiClient.Setup(c => c.Ping(CancellationToken)).ThrowsAsync(Exception);
                
                return this;
            }
        }
    }
}
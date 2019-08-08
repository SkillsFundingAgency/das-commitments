using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.HealthChecks;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.HealthChecks
{
    [TestFixture]
    [Parallelizable]
    public class NServiceBusHealthCheckTests : FluentTest<NServiceBusHealthCheckTestsFixture>
    {
        [Test]
        public Task CheckHealthAsync_WhenSendSucceeds_ThenShouldShouldReturnHealthyStatus()
        {
            return TestAsync(
                f => f.SetSendSuccess(),
                f => f.CheckHealthAsync(),
                (f, r) =>
                {
                    r.Should().NotBeNull();
                    r.Status.Should().Be(HealthStatus.Healthy);
                });
        }

        [Test]
        public Task CheckHealthAsync_WhenSendFails_ThenShouldThrowException()
        {
            return TestExceptionAsync(
                f => f.SetSendFailure(),
                f => f.CheckHealthAsync(),
                (f, r) => r.Should().Throw<Exception>().Which.Should().Be(f.Exception));
        }
    }

    public class NServiceBusHealthCheckTestsFixture
    {
        public HealthCheckContext HealthCheckContext { get; set; }
        public Mock<IMessageSession> MessageSession { get; set; }
        public NServiceBusHealthCheck HealthCheck { get; set; }
        public Exception Exception { get; set; }

        public NServiceBusHealthCheckTestsFixture()
        {
            HealthCheckContext = new HealthCheckContext
            {
                Registration = new HealthCheckRegistration("Foo", Mock.Of<IHealthCheck>(), null, null)
            };
            
            MessageSession = new Mock<IMessageSession>();
            HealthCheck = new NServiceBusHealthCheck(MessageSession.Object);
            Exception = new Exception("Foobar");
        }

        public Task<HealthCheckResult> CheckHealthAsync()
        {
            return HealthCheck.CheckHealthAsync(HealthCheckContext);
        }

        public NServiceBusHealthCheckTestsFixture SetSendSuccess()
        {
            MessageSession.Setup(s => s.Send(It.IsAny<object>(), It.IsAny<SendOptions>())).Returns(Task.CompletedTask);
            
            return this;
        }

        public NServiceBusHealthCheckTestsFixture SetSendFailure()
        {
            MessageSession.Setup(s => s.Send(It.IsAny<object>(), It.IsAny<SendOptions>())).ThrowsAsync(Exception);
            
            return this;
        }
    }
}
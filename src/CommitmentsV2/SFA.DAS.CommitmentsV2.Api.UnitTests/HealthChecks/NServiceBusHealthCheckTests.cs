using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.HealthChecks;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.HealthChecks
{
    [TestFixture]
    [Parallelizable]
    public class NServiceBusHealthCheckTests : FluentTest<NServiceBusHealthCheckTestsFixture>
    {
        [Test]
        public async Task CheckHealthAsync_WhenReceiveSucceeds_ThenShouldPollDistributedCacheOnce()
        {
            await TestAsync(
                f => f.SetSendSuccess().SetReceiveSuccess(),
                f => f.CheckHealthAsync(),
                (f, r) => f.DistributedCache.Verify(c => c.GetAsync(f.MessageId, f.CancellationToken), Times.Once));
        }
        
        [Test]
        public async Task CheckHealthAsync_WhenReceiveSucceeds_ThenShouldReturnHealthyStatus()
        {
            await TestAsync(
                f => f.SetSendSuccess().SetReceiveSuccess(),
                f => f.CheckHealthAsync(),
                (f, r) =>
                {
                    r.Should().NotBeNull();
                    r.Status.Should().Be(HealthStatus.Healthy);
                });
        }

        [Test]
        public async Task CheckHealthAsync_WhenSendFails_ThenShouldThrowException()
        {
            await TestExceptionAsync(
                f => f.SetSendFailure(),
                f => f.CheckHealthAsync(),
                (f, r) => r.Should().ThrowAsync<Exception>());
        }
        
        [Test]
        public async Task CheckHealthAsync_WhenReceiveFails_ThenShouldContinuePollingDistributedCache()
        {
            await TestAsync(
                f => f.SetSendSuccess().SetReceiveFailure(),
                f => f.CheckHealthAsync(),
                (f, r) => f.DistributedCache.Verify(c => c.GetAsync(f.MessageId, f.CancellationToken), Times.AtLeast(2)));
        }
        
        [Test]
        public async Task CheckHealthAsync_WhenTimeoutExpires_ThenShouldReturnDegradedStatus()
        {
            await TestAsync(
                f => f.SetSendSuccess().SetReceiveFailure(),
                f => f.CheckHealthAsync(),
                (f, r) =>
                {
                    r.Should().NotBeNull();
                    r.Status.Should().Be(HealthStatus.Degraded);
                    f.Stopwatch.Elapsed.Should().BeGreaterOrEqualTo(f.Timeout);
                });
        }
        
        [Test]
        public async Task CheckHealthAsync_WhenCancellationRequested_ThenShouldThrowException()
        {
            await TestExceptionAsync(
                f => f.SetSendSuccess().SetCancellationRequested(),
                f => f.CheckHealthAsync(),
                (f, r) => r.Should().ThrowAsync<OperationCanceledException>());
        }
    }

    public class NServiceBusHealthCheckTestsFixture
    {
        public HealthCheckContext HealthCheckContext { get; set; }
        public Stopwatch Stopwatch { get; set; }
        public TimeSpan Interval { get; set; }
        public TimeSpan Timeout { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public Mock<IMessageSession> MessageSession { get; set; }
        public Mock<IDistributedCache> DistributedCache { get; set; }
        public NServiceBusHealthCheck HealthCheck { get; set; }
        public Exception Exception { get; set; }
        public string MessageId { get; set; }

        public NServiceBusHealthCheckTestsFixture()
        {
            HealthCheckContext = new HealthCheckContext
            {
                Registration = new HealthCheckRegistration("Foo", Mock.Of<IHealthCheck>(), null, null)
            };

            Interval = TimeSpan.FromMilliseconds(100);
            Timeout = TimeSpan.FromMilliseconds(500);
            CancellationToken = new CancellationToken();
            MessageSession = new Mock<IMessageSession>();
            DistributedCache = new Mock<IDistributedCache>();
            
            HealthCheck = new NServiceBusHealthCheck(MessageSession.Object, DistributedCache.Object)
            {
                Interval = Interval,
                Timeout = Timeout
            };
            
            Exception = new Exception("Foobar");
        }

        public Task<HealthCheckResult> CheckHealthAsync()
        {
            Stopwatch = Stopwatch.StartNew();
            
            return HealthCheck.CheckHealthAsync(HealthCheckContext, CancellationToken);
        }

        public NServiceBusHealthCheckTestsFixture SetSendSuccess()
        {
            MessageSession.Setup(s => s.Send(It.IsAny<RunHealthCheckCommand>(), It.IsAny<SendOptions>()))
                .Returns<RunHealthCheckCommand, SendOptions>((c, o) =>
                {
                    MessageId = o.GetMessageId();

                    if (string.IsNullOrWhiteSpace(MessageId))
                    {
                        throw new ArgumentNullException(nameof(MessageId));
                    }
                    
                    return Task.CompletedTask;
                });
            
            return this;
        }

        public NServiceBusHealthCheckTestsFixture SetSendFailure()
        {
            MessageSession.Setup(s => s.Send(It.IsAny<RunHealthCheckCommand>(), It.IsAny<SendOptions>())).ThrowsAsync(Exception);
            
            return this;
        }

        public NServiceBusHealthCheckTestsFixture SetReceiveSuccess()
        {
            DistributedCache.Setup(s => s.GetAsync(It.Is<string>(k => k == MessageId), CancellationToken))
                .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("OK"));
            
            return this;
        }

        public NServiceBusHealthCheckTestsFixture SetReceiveFailure()
        {
            DistributedCache.Setup(s => s.GetAsync(It.Is<string>(k => k == MessageId), CancellationToken))
                .ReturnsAsync((byte[])null);
            
            return this;
        }

        public NServiceBusHealthCheckTestsFixture SetCancellationRequested()
        {
            CancellationToken = new CancellationToken(true);
            
            return this;
        }
    }
}
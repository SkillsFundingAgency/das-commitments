using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Api.HealthChecks;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.HealthChecks
{
    [TestFixture]
    [Parallelizable]
    public class NServiceBusHealthCheckTests
    {
        [Test]
        public async Task CheckHealthAsync_WhenReceiveSucceeds_ThenShouldPollDistributedCacheOnce()
        {
            var fixture = new NServiceBusHealthCheckTestsFixture();
            fixture.SetSendSuccess().SetReceiveSuccess();

            await fixture.CheckHealthAsync();

            fixture.DistributedCache.Verify(c => c.GetAsync(fixture.MessageId, fixture.CancellationToken), Times.Once);
        }
        
        [Test]
        public async Task CheckHealthAsync_WhenReceiveSucceeds_ThenShouldReturnHealthyStatus()
        {
            var fixture = new NServiceBusHealthCheckTestsFixture();
            fixture.SetSendSuccess().SetReceiveSuccess();

           var result =  await fixture.CheckHealthAsync();
           
           result.Should().NotBeNull();
           result.Status.Should().Be(HealthStatus.Healthy);
        }

        [Test]
        public async Task CheckHealthAsync_WhenSendFails_ThenShouldThrowException()
        {
            var fixture = new NServiceBusHealthCheckTestsFixture();
            fixture.SetSendFailure();
            
            Func<Task> result = () => fixture.CheckHealthAsync();

            await result.Should().ThrowAsync<Exception>();
        }
        
        [Test]
        public async Task CheckHealthAsync_WhenReceiveFails_ThenShouldContinuePollingDistributedCache()
        {
            var fixture = new NServiceBusHealthCheckTestsFixture();
            fixture.SetSendSuccess().SetReceiveFailure();
            await fixture.CheckHealthAsync();

            fixture.DistributedCache.Verify(c => c.GetAsync(fixture.MessageId, fixture.CancellationToken),
                Times.AtLeast(2));
        }
        
        [Test]
        public async Task CheckHealthAsync_WhenTimeoutExpires_ThenShouldReturnDegradedStatus()
        {
            var fixture = new NServiceBusHealthCheckTestsFixture();
            fixture.SetSendSuccess().SetReceiveFailure();

            var result = await fixture.CheckHealthAsync();
            
            result.Should().NotBeNull();
            result.Status.Should().Be(HealthStatus.Degraded);
            fixture.Stopwatch.Elapsed.Should().BeGreaterOrEqualTo(fixture.Timeout);
        }
        
        [Test]
        public async Task CheckHealthAsync_WhenCancellationRequested_ThenShouldThrowException()
        {
            var fixture = new NServiceBusHealthCheckTestsFixture();
            fixture.SetSendSuccess().SetCancellationRequested();
            
            Func<Task> result = () => fixture.CheckHealthAsync();

            await result.Should().ThrowAsync<OperationCanceledException>();
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
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NServiceBus.Testing;
using SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.CommandHandlers
{
    [TestFixture]
    [Parallelizable]
    public class RunHealthCheckCommandHandlerTests
    {
        [Test]
        public async Task Handle_WhenHandlingCommand_ThenShouldLogInformation()
        {
            var fixture = new RunHealthCheckCommandHandlerTestsFixture();
            await fixture.Handle();

            fixture.Logger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>())
            );
        }

        [Test]
        public async Task Handle_WhenHandlingCommand_ThenShouldAddMessageIdToDistributedCache()
        {
            var fixture = new RunHealthCheckCommandHandlerTestsFixture();
            await fixture.Handle();
            
            fixture.DistributedCache.Verify(c => c.SetAsync(fixture.MessageId, It.Is<byte[]>(v => System.Text.Encoding.UTF8.GetString(v) == "OK"), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()));
        }
    }

    public class RunHealthCheckCommandHandlerTestsFixture
    {
        public Mock<IDistributedCache> DistributedCache { get; set; }
        public Mock<ILogger<RunHealthCheckCommandHandler>> Logger { get; set; }
        public string MessageId { get; set; }
        public RunHealthCheckCommand Command { get; set; }
        public TestableMessageHandlerContext MessageHandlerContext { get; set; }
        public IHandleMessages<RunHealthCheckCommand> CommandHandler { get; set; }

        public RunHealthCheckCommandHandlerTestsFixture()
        {
            DistributedCache = new Mock<IDistributedCache>();
            Logger = new Mock<ILogger<RunHealthCheckCommandHandler>>();
            MessageId = Guid.NewGuid().ToString();
            Command = new RunHealthCheckCommand();
            MessageHandlerContext = new TestableMessageHandlerContext { MessageId = MessageId };
            CommandHandler = new RunHealthCheckCommandHandler(DistributedCache.Object, Logger.Object);
        }

        public Task Handle()
        {
            return CommandHandler.Handle(Command, MessageHandlerContext);
        }
    }
}
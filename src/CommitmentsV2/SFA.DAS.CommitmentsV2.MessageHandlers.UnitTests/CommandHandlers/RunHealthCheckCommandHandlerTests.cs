using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Moq;
using NServiceBus;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.CommandHandlers
{
    [TestFixture]
    [Parallelizable]
    public class RunHealthCheckCommandHandlerTests : FluentTest<RunHealthCheckCommandHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenHandlingCommand_ThenShouldLogInformation()
        {
            return TestAsync(
                f => f.Handle(),
                f => f.Logger.Verify(l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<FormattedLogValues>(v => v.ToString().Equals($"Handled {nameof(RunHealthCheckCommand)} with MessageId '{f.MessageId}'")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>())));
        }
        
        [Test]
        public Task Handle_WhenHandlingCommand_ThenShouldAddMessageIdToDistributedCache()
        {
            return TestAsync(
                f => f.Handle(),
                f => f.DistributedCache.Verify(c => c.SetAsync(f.MessageId, It.Is<byte[]>(v => System.Text.Encoding.UTF8.GetString(v) == "OK"), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>())));
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
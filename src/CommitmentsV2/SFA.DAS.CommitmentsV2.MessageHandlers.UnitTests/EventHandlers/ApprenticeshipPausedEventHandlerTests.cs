using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class ApprenticeshipPausedEventHandlerTests
    {
        private ApprenticeshipPausedEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ApprenticeshipPausedEventHandlerTestsFixture();

            _fixture.Mediator.Setup(m => m.Send(It.IsAny<GetApprenticeshipQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetTestApprenticeshipQueryResult());
        }

        [Test]
        public async Task WhenHandlingApprenticeshipPauseEvent_ThenGetApprenticeshipQueryIsCalled()
        {
            await _fixture.Handler.Handle(new ApprenticeshipPausedEvent { }, _fixture.MessageHandlerContext.Object);

            _fixture.Mediator.Verify(c => c.Send(It.IsAny<GetApprenticeshipQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task WhenHandlingApprenticeshipPauseEvent_ThenSendEmailToProviderIsCalled()
        {
            await _fixture.Handler.Handle(new ApprenticeshipPausedEvent { }, _fixture.MessageHandlerContext.Object);

            _fixture.MessageHandlerContext.Verify(m => m.Send(It.IsAny<SendEmailToProviderCommand>(), It.IsAny<SendOptions>()), Times.Once);
        }


        private GetApprenticeshipQueryResult GetTestApprenticeshipQueryResult()
        {
            return new GetApprenticeshipQueryResult
            {
                ProviderId = 12345678,
                FirstName = "FirstName",
                LastName = "LastName",
                PauseDate = DateTime.UtcNow
            };
        }
    }

    public class ApprenticeshipPausedEventHandlerTestsFixture : EventHandlerTestsFixture<ApprenticeshipPausedEvent, ApprenticeshipPausedEventHandler>
    {
        public Mock<ILogger<ApprenticeshipPausedEventHandler>> Logger { get; set; }

        public ApprenticeshipPausedEventHandlerTestsFixture() : base((m) => null)
        {
            Logger = new Mock<ILogger<ApprenticeshipPausedEventHandler>>();

            Handler = new ApprenticeshipPausedEventHandler(Mediator.Object, Logger.Object);
        }
    }
}

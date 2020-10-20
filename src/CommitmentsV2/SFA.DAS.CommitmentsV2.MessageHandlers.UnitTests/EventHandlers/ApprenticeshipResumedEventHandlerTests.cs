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
    public class ApprenticeshipResumedEventHandlerTests
    {
        private ApprenticeshipResumedEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ApprenticeshipResumedEventHandlerTestsFixture();

            _fixture.Mediator.Setup(m => m.Send(It.IsAny<GetApprenticeshipQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetTestApprenticeshipQueryResult());
        }

        [Test]
        public async Task WhenHandlingApprenticeshipResumeEvent_ThenGetApprenticeshipQueryIsCalled()
        {
            await _fixture.Handler.Handle(new ApprenticeshipResumedEvent { }, _fixture.MessageHandlerContext.Object);

            _fixture.Mediator.Verify(c => c.Send(It.IsAny<GetApprenticeshipQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task WhenHandlingApprenticeshipResumeEvent_ThenSendEmailToProviderIsCalled()
        {
            await _fixture.Handler.Handle(new ApprenticeshipResumedEvent { }, _fixture.MessageHandlerContext.Object);

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

    public class ApprenticeshipResumedEventHandlerTestsFixture : EventHandlerTestsFixture<ApprenticeshipResumedEvent, ApprenticeshipResumedEventHandler>
    {
        public Mock<ILogger<ApprenticeshipResumedEventHandler>> Logger { get; set; }

        public ApprenticeshipResumedEventHandlerTestsFixture() : base((m) => null)
        {
            Logger = new Mock<ILogger<ApprenticeshipResumedEventHandler>>();

            Handler = new ApprenticeshipResumedEventHandler(Mediator.Object, Logger.Object);
        }
    }
}

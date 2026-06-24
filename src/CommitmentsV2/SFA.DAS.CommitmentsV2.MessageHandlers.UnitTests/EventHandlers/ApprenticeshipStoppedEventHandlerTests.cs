using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.MessageHandlers.Services.Interface;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers;

[TestFixture]
public class ApprenticeshipStoppedEventHandlerTests
{
    public ApprenticeshipStoppedEventHandlerTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new ApprenticeshipStoppedEventHandlerTestsFixture();
    }

    [Test]
    public async Task When_HandlingEvent_EmailIsSent()
    {
        await _fixture.Handle(true);
        _fixture.VerifyEmailSent();
    }

    [Test]
    public async Task When_HandlingEvent_IsNotWithdrawalFromIlr_EmailIsNotSent()
    {
        await _fixture.Handle(false);
        _fixture.VerifyEmailNotSent();
    }

    public class ApprenticeshipStoppedEventHandlerTestsFixture
    {
        private ApprenticeshipStoppedEventHandler _handler;
        public ApprenticeshipStoppedEvent _event;
        private readonly Fixture _autoFixture;
        private Mock<IMessageHandlerContext> _messageHandlerContext;
        private readonly Mock<IWithDrawalNotificationToEmployerService> _notificationService;
        private readonly Mock<ILogger<ApprenticeshipStoppedEventHandler>> _logger;

        public ApprenticeshipStoppedEventHandlerTestsFixture()
        {
            _autoFixture = new Fixture();

            _messageHandlerContext = new Mock<IMessageHandlerContext>();
            _notificationService = new Mock<IWithDrawalNotificationToEmployerService>();
            _logger = new Mock<ILogger<ApprenticeshipStoppedEventHandler>>();

            _notificationService.Setup(x => x.SendWithdrawalNotificationToEmployer(It.Is<long>(t => t == _event.ApprenticeshipId), It.IsAny<IMessageHandlerContext>()))
                .Returns(Task.CompletedTask);

            _handler = new ApprenticeshipStoppedEventHandler(_notificationService.Object, _logger.Object);

            _event = _autoFixture.Create<ApprenticeshipStoppedEvent>();
        }

        public async Task Handle(bool isWithdrawalFromIlr)
        {
            _event.IsWithdrawnViaIlr = isWithdrawalFromIlr;
            await _handler.Handle(_event, _messageHandlerContext.Object);
        }

        public void VerifyEmailSent()
        {
            _notificationService.Verify(x => x.SendWithdrawalNotificationToEmployer(It.Is<long>(e => e == _event.ApprenticeshipId), It.IsAny<IMessageHandlerContext>()), Times.Once);
        }

        public void VerifyEmailNotSent()
        {
            _notificationService.Verify(x => x.SendWithdrawalNotificationToEmployer(It.Is<long>(e => e == _event.ApprenticeshipId), It.IsAny<IMessageHandlerContext>()), Times.Never);
        }
    }
}
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.MessageHandlers.Services.Interface;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers;

[TestFixture]
public class ApprenticeshipStopDateChangedEventHandlerTests
{
    public ApprenticeshipStopDateChangedEventHanlderTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new ApprenticeshipStopDateChangedEventHanlderTestsFixture();
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

    public class ApprenticeshipStopDateChangedEventHanlderTestsFixture
    {
        private ApprenticeshipStopDateChangedEventHandler _handler;
        public ApprenticeshipStopDateChangedEvent _event;
        private readonly Fixture _autoFixture;
        private Mock<IMessageHandlerContext> _messageHandlerContext;
        private readonly Mock<IWithDrawalNotificationToEmployerService> _notificationService;
        private readonly Mock<ILogger<ApprenticeshipStopDateChangedEventHandler>> _logger;

        public ApprenticeshipStopDateChangedEventHanlderTestsFixture()
        {
            _autoFixture = new Fixture();

            _event = _autoFixture.Create<ApprenticeshipStopDateChangedEvent>();
            _messageHandlerContext = new Mock<IMessageHandlerContext>();
            _notificationService = new Mock<IWithDrawalNotificationToEmployerService>();
            _logger = new Mock<ILogger<ApprenticeshipStopDateChangedEventHandler>>();

            _notificationService.Setup(x => x.SendWithdrawalNotificationToEmployer(It.Is<long>(t => t == _event.ApprenticeshipId), It.IsAny<IMessageHandlerContext>()))
                .Returns(Task.CompletedTask);

            _handler = new ApprenticeshipStopDateChangedEventHandler(_notificationService.Object, _logger.Object);
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
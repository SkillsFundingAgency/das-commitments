using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.MessageHandlers.Services.Interface;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers;

[TestFixture]
public class LearnerStoppedDateUpdatedNotificationEventHandlerTests
{
    public LearnerStoppedDateUpdatedNotificationEventHandlerTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new LearnerStoppedDateUpdatedNotificationEventHandlerTestsFixture();
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

    public class LearnerStoppedDateUpdatedNotificationEventHandlerTestsFixture
    {
        private LearnerStoppedDateUpdatedNotificationEventHandler _handler;
        public LearnerWithdrawalNotificationEvent _event;
        private readonly Fixture _autoFixture;
        private Mock<IMessageHandlerContext> _messageHandlerContext;
        private readonly Mock<IWithDrawalNotificationToEmployerService> _notificationService;
        private readonly Mock<ILogger<LearnerStoppedDateUpdatedNotificationEventHandler>> _logger;

        public LearnerStoppedDateUpdatedNotificationEventHandlerTestsFixture()
        {
            _autoFixture = new Fixture();

            _messageHandlerContext = new Mock<IMessageHandlerContext>();
            _notificationService = new Mock<IWithDrawalNotificationToEmployerService>();
            _logger = new Mock<ILogger<LearnerStoppedDateUpdatedNotificationEventHandler>>();

            _notificationService.Setup(x => x.SendWithdrawalNotificationToEmployer(It.IsAny<LearnerWithdrawalNotificationEvent>(), It.IsAny<IMessageHandlerContext>()))
                .Returns(Task.CompletedTask);

            _handler = new LearnerStoppedDateUpdatedNotificationEventHandler(_notificationService.Object, _logger.Object);

            _event = _autoFixture.Create<LearnerWithdrawalNotificationEvent>();
        }

        public async Task Handle(bool isWithdrawalFromIlr)
        {
            _event.IsWithdrawalFromIlr = isWithdrawalFromIlr;
            await _handler.Handle(_event, _messageHandlerContext.Object);
        }

        public void VerifyEmailSent()
        {
            _notificationService.Verify(x => x.SendWithdrawalNotificationToEmployer(It.Is<LearnerWithdrawalNotificationEvent>(e => e == _event), It.IsAny<IMessageHandlerContext>()), Times.Once);
        }

        public void VerifyEmailNotSent()
        {
            _notificationService.Verify(x => x.SendWithdrawalNotificationToEmployer(It.Is<LearnerWithdrawalNotificationEvent>(e => e == _event), It.IsAny<IMessageHandlerContext>()), Times.Never);
        }
    }
}
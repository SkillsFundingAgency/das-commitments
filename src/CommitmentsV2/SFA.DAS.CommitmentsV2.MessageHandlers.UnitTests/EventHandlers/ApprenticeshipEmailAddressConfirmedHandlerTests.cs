using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipEmailAddressConfirmed;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class ApprenticeshipEmailAddressConfirmedEventHandlerTests
    {
        private ApprenticeshipEmailAddressConfirmedEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ApprenticeshipEmailAddressConfirmedEventHandlerTestsFixture();

        }

        [Test]
        public async Task WhenHandlingApprenticeshipConfirmationCommencedEvent_ThenCommandIsSentToMediator()
        {
            await _fixture.Handle();
            _fixture.VerifySend<ApprenticeshipEmailAddressConfirmedCommand>((c, e) =>
                c.ApprenticeshipId == e.CommitmentsApprenticeshipId &&
                c.ApprenticeId == e.ApprenticeId);
        }
    }

    public class ApprenticeshipEmailAddressConfirmedEventHandlerTestsFixture : EventHandlerTestsFixture<ApprenticeshipEmailAddressConfirmedEvent, ApprenticeshipEmailAddressConfirmedEventHandler>
    {
        public ApprenticeshipEmailAddressConfirmedEventHandlerTestsFixture() : base(m=> new ApprenticeshipEmailAddressConfirmedEventHandler(m, Mock.Of<ILogger<ApprenticeshipEmailAddressConfirmedEventHandler>>()))
        { }
    }
}
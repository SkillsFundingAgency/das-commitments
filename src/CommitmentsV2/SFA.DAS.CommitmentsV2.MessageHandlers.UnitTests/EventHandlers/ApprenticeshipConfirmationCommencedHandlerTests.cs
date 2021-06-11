using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipConfirmationCommenced;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class ApprenticeshipConfirmationCommencedHandlerTests
    {
        private ApprenticeshipConfirmationCommencedHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ApprenticeshipConfirmationCommencedHandlerTestsFixture();

        }

        [Test]
        public async Task WhenHandlingApprenticeshipPauseEvent_ThenEncodingServiceIsCalled()
        {
            await _fixture.Handle();
            _fixture.VerifySend<ApprenticeshipConfirmationCommencedCommand>((c, e) =>
                c.ApprenticeshipId == e.CommitmentsApprenticeshipId &&
                c.CommitmentsApprovedOn == e.CommitmentsApprovedOn &&
                c.ConfirmationOverdueOn == e.ApprenticeshipConfirmationOverdueOn);
        }
    }

    public class ApprenticeshipConfirmationCommencedHandlerTestsFixture : EventHandlerTestsFixture<ApprenticeshipConfirmationCommencedEvent, ApprenticeshipConfirmationCommencedEventHandler>
    {
    }
}
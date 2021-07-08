using NUnit.Framework;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipConfirmed;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class ApprenticeshipConfirmedHandlerTests
    {
        private ApprenticeshipConfirmedHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ApprenticeshipConfirmedHandlerTestsFixture();
        }

        [Test]
        public async Task WhenHandlingApprenticeshipConfirmedEvent_ThenCommandIsSentToMediator()
        {
            await _fixture.Handle();
            _fixture.VerifySend<ApprenticeshipConfirmedCommand>((c, e) =>
                c.ApprenticeshipId == e.CommitmentsApprenticeshipId &&
                c.CommitmentsApprovedOn == e.CommitmentsApprovedOn &&
                c.ConfirmedOn == e.ConfirmedOn);
        }
    }

    public class ApprenticeshipConfirmedHandlerTestsFixture : EventHandlerTestsFixture<ApprenticeshipConfirmationConfirmedEvent, ApprenticeshipConfirmedEventHandler>
    {
        public ApprenticeshipConfirmedHandlerTestsFixture() : base(m => new ApprenticeshipConfirmedEventHandler(m, Mock.Of<ILogger<ApprenticeshipConfirmedEventHandler>>()))
        {}
    }
}
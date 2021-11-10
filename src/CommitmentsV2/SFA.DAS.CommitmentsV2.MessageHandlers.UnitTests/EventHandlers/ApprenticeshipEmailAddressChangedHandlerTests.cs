using NUnit.Framework;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using System.Threading.Tasks;
using Moq;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipEmailAddressChangedByApprentice;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class ApprenticeshipEmailAddressChangedHandlerTests
    {
        private ApprenticeshipEmailAddressChangedHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ApprenticeshipEmailAddressChangedHandlerTestsFixture();

        }

        [Test]
        public async Task WhenHandlingApprenticeshipEmailChangedEvent_ThenCommandIsSentToMediator()
        {
            await _fixture.Handle();
            _fixture.VerifySend<ApprenticeshipEmailAddressChangedByApprenticeCommand>((c, e) =>
                c.ApprenticeshipId == e.CommitmentsApprenticeshipId &&
                c.ApprenticeId == e.ApprenticeId);
        }
    }

    public class ApprenticeshipEmailAddressChangedHandlerTestsFixture : EventHandlerTestsFixture<ApprenticeshipEmailAddressChangedEvent, ApprenticeshipEmailAddressChangedEventHandler>
    {
        public ApprenticeshipEmailAddressChangedHandlerTestsFixture() : base(m=> new ApprenticeshipEmailAddressChangedEventHandler(m, Mock.Of<ILogger<ApprenticeshipEmailAddressChangedEventHandler>>()))
        { }
    }
}
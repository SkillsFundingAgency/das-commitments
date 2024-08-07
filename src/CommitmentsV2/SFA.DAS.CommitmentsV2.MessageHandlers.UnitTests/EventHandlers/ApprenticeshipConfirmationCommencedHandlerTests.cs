﻿using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipConfirmationCommenced;
using Microsoft.Extensions.Logging;

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
        public async Task WhenHandlingApprenticeshipConfirmationCommencedEvent_ThenCommandIsSentToMediator()
        {
            await _fixture.Handle();
            _fixture.VerifySend<ApprenticeshipConfirmationCommencedCommand>((c, e) =>
                c.ApprenticeshipId == e.CommitmentsApprenticeshipId &&
                c.CommitmentsApprovedOn == e.CommitmentsApprovedOn &&
                c.ConfirmationOverdueOn == e.ConfirmationOverdueOn);
        }
    }

    public class ApprenticeshipConfirmationCommencedHandlerTestsFixture : EventHandlerTestsFixture<ApprenticeshipConfirmationCommencedEvent, ApprenticeshipConfirmationCommencedEventHandler>
    {
        public ApprenticeshipConfirmationCommencedHandlerTestsFixture() : base(m=> new ApprenticeshipConfirmationCommencedEventHandler(m, Mock.Of<ILogger<ApprenticeshipConfirmationCommencedEventHandler>>()))
        { }
    }
}
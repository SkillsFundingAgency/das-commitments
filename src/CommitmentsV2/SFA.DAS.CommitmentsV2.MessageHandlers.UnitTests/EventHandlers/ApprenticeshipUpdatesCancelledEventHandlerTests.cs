using Microsoft.Extensions.Logging;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class ApprenticeshipUpdatesCancelledEventHandlerTests
    {
        [Test]
        public async Task Handle_WhenApprenticeshipUpdateCancelledEventIsReceived_ThenShouldRelayAnyMessagesToAzureServiceBus()
        {
            var fixture = new ApprenticeshipUpdatesCancelledEventHandlerTestsFixture()
                .WithApprenticeshipUpdateCancelledEvent();
            
            await fixture.Handle();
            
            fixture.VerifyPropertiesAreMappedCorrectlyWhenRelayingMessage();
        }
    }

    public class ApprenticeshipUpdatesCancelledEventHandlerTestsFixture
    {
        public Mock<ILegacyTopicMessagePublisher> LegacyTopicMessagePublisher;
        public ApprenticeshipUpdateCancelledEventHandler Sut;
        public ApprenticeshipUpdateCancelledEvent ApprenticeshipUpdateCancelledEvent;
        public long ApprenticeshipId;
        public long ProviderId;
        public long AccountId;

        public ApprenticeshipUpdatesCancelledEventHandlerTestsFixture()
        {
            var autoFixture = new Fixture();

            ApprenticeshipId = autoFixture.Create<long>();
            ProviderId = autoFixture.Create<long>();
            AccountId = autoFixture.Create<long>();
            
            LegacyTopicMessagePublisher = new Mock<ILegacyTopicMessagePublisher>();

            Sut = new ApprenticeshipUpdateCancelledEventHandler(LegacyTopicMessagePublisher.Object, Mock.Of<ILogger<ApprenticeshipUpdateCancelledEventHandler>>());
        }

        public Task Handle()
        {
            return Sut.Handle(ApprenticeshipUpdateCancelledEvent, Mock.Of<IMessageHandlerContext>());
        }

        public ApprenticeshipUpdatesCancelledEventHandlerTestsFixture WithApprenticeshipUpdateCancelledEvent()
        {
            ApprenticeshipUpdateCancelledEvent = new ApprenticeshipUpdateCancelledEvent { AccountId = AccountId, ApprenticeshipId = ApprenticeshipId, ProviderId = ProviderId };
            return this;
        }

        public void VerifyPropertiesAreMappedCorrectlyWhenRelayingMessage()
        {
            LegacyTopicMessagePublisher.Verify(x => x.PublishAsync(It.Is<ApprenticeshipUpdateCancelled>(p =>
                p.AccountId == AccountId &&
                p.ProviderId == ProviderId &&
                p.ApprenticeshipId == ApprenticeshipId)));
        }
    }
}

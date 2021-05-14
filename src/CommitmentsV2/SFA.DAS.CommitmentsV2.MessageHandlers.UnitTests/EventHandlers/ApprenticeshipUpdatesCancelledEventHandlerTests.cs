using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
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
            var f = new ApprenticeshipUpdatesCancelledEventHandlerTestsFixture().WithApprenticeshipUpdateCancelledEvent();
            await f.Handle();
            f.VerifyPropertiesAreMappedCorrectlyWhenRelayingMessage();
        }
    }

    public class ApprenticeshipUpdatesCancelledEventHandlerTestsFixture
    {
        public Mock<IMessageHandlerContext> MessageHandlerContext;
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

            MessageHandlerContext = new Mock<IMessageHandlerContext>();
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

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
    public class ApprenticeshipUpdatesRejectedEventHandlerTests
    {
        [Test]
        public async Task Handle_WhenApprenticeshipUpdateRejectedEventIsReceived_ThenShouldRelayAnyMessagesToAzureServiceBus()
        {
            var fixture = new ApprenticeshipUpdatesRejectedEventHandlerTestsFixture().WithApprenticeshipUpdateRejectedEvent();
            await fixture.Handle();
            fixture.VerifyPropertiesAreMappedCorrectlyWhenRelayingMessage();
        }
    }

    public class ApprenticeshipUpdatesRejectedEventHandlerTestsFixture
    {
        public Mock<ILegacyTopicMessagePublisher> LegacyTopicMessagePublisher;
        public ApprenticeshipUpdateRejectedEventHandler Sut;
        public ApprenticeshipUpdateRejectedEvent ApprenticeshipUpdateRejectedEvent;
        public long ApprenticeshipId;
        public long ProviderId;
        public long AccountId;

        public ApprenticeshipUpdatesRejectedEventHandlerTestsFixture()
        {
            var autoFixture = new Fixture();

            ApprenticeshipId = autoFixture.Create<long>();
            ProviderId = autoFixture.Create<long>();
            AccountId = autoFixture.Create<long>();
            
            LegacyTopicMessagePublisher = new Mock<ILegacyTopicMessagePublisher>();

            Sut = new ApprenticeshipUpdateRejectedEventHandler(LegacyTopicMessagePublisher.Object, Mock.Of<ILogger<ApprenticeshipUpdateRejectedEventHandler>>());
        }

        public Task Handle()
        {
            return Sut.Handle(ApprenticeshipUpdateRejectedEvent, Mock.Of<IMessageHandlerContext>());
        }

        public ApprenticeshipUpdatesRejectedEventHandlerTestsFixture WithApprenticeshipUpdateRejectedEvent()
        {
            ApprenticeshipUpdateRejectedEvent = new ApprenticeshipUpdateRejectedEvent { AccountId = AccountId, ApprenticeshipId = ApprenticeshipId, ProviderId = ProviderId };
            return this;
        }

        public void VerifyPropertiesAreMappedCorrectlyWhenRelayingMessage()
        {
            LegacyTopicMessagePublisher.Verify(x => x.PublishAsync(It.Is<ApprenticeshipUpdateRejected>(p =>
                p.AccountId == AccountId &&
                p.ProviderId == ProviderId &&
                p.ApprenticeshipId == ApprenticeshipId)));
        }
    }
}

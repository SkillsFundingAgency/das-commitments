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
    public class ApprenticeshipUpdateCreatedEventHandlerTests
    {
        [Test]

        public async Task Handle_ApprenticeshipUpdateCreatedEvent_ThenShouldRelayMessageToAzureServiceBus()
        {
            var f = new ApprenticeshipUpdateCreatedEventHandlerTestsFixture();
            await f.Handle();
            f.VerifyPropertiesAreMappedCorrectlyWhenRelayingMessage();
        }
    }

    public class ApprenticeshipUpdateCreatedEventHandlerTestsFixture
    {
        public Mock<IMessageHandlerContext> MessageHandlerContext;
        public Mock<ILegacyTopicMessagePublisher> LegacyTopicMessagePublisher;
        public ApprenticeshipUpdateCreatedEventHandler Sut;
        public ApprenticeshipUpdateCreatedEvent ApprenticeshipUpdateCreatedEvent;

        public ApprenticeshipUpdateCreatedEventHandlerTestsFixture()
        {
            var autoFixture = new Fixture();
            MessageHandlerContext = new Mock<IMessageHandlerContext>();
            LegacyTopicMessagePublisher = new Mock<ILegacyTopicMessagePublisher>();

            Sut = new ApprenticeshipUpdateCreatedEventHandler(LegacyTopicMessagePublisher.Object, Mock.Of<ILogger<ApprenticeshipUpdateCreatedEventHandler>>());
            ApprenticeshipUpdateCreatedEvent = autoFixture.Create<ApprenticeshipUpdateCreatedEvent>();
        }

        public Task Handle()
        {
            return Sut.Handle(ApprenticeshipUpdateCreatedEvent, Mock.Of<IMessageHandlerContext>());
        }

        public void VerifyPropertiesAreMappedCorrectlyWhenRelayingMessage()
        {
            LegacyTopicMessagePublisher.Verify(x => x.PublishAsync(It.Is<ApprenticeshipUpdateCreated>(p =>
                p.AccountId == ApprenticeshipUpdateCreatedEvent.AccountId &&
                p.ProviderId == ApprenticeshipUpdateCreatedEvent.ProviderId &&
                p.ApprenticeshipId == ApprenticeshipUpdateCreatedEvent.ApprenticeshipId)));
        }
    }
}

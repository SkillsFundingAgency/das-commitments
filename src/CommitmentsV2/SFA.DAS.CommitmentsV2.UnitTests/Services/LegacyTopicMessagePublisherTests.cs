using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Services;


namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class LegacyTopicMessagePublisherTests
    {
        [Test]
        public async Task PublishAsync_ShouldCreateTopicClientPassingInCorrectParameters()
        {
            var f = new LegacyTopicMessagePublisherTestsFixture();
            await f.Sut.PublishAsync(f.ApprovedCohortReturnedToProvider);
            f.VerifyTopicClientFactoryReceivesConnectionStringAndMessageGroupName("approved_cohort_returned_to_provider");
        }

        [Test]
        public async Task PublishAsync_ShouldCreateTopicClientPassingInNameOfClassAsMessageGroupName()
        {
            var f = new LegacyTopicMessagePublisherTestsFixture();
            var @event = new ApprovedCohortReturnedToProviderEvent(1, DateTime.Now);
            await f.Sut.PublishAsync(@event);
            f.VerifyTopicClientFactoryReceivesConnectionStringAndMessageGroupName(@event.GetType().Name);
        }

        [Test]
        public async Task PublishAsync_ShouldNotCloseTopicClientConnectionIfAlreadyClosing()
        {
            var f = new LegacyTopicMessagePublisherTestsFixture();
            f.TopicClient.Setup(x => x.IsClosedOrClosing).Returns(true);
            await f.Sut.PublishAsync(f.ApprovedCohortReturnedToProvider);
            f.VerifyTopicClientDoesNotCloseConnection();
        }

        [Test]
        public void PublishAsync_ShouldNotCloseTopicClientConnectionWhenClientIsNull()
        {
            var f = new LegacyTopicMessagePublisherTestsFixture().ThrowInvalidOperationExceptionWhenCreatingTopicClient();
            Assert.ThrowsAsync<InvalidOperationException>(() => f.Sut.PublishAsync(f.ApprovedCohortReturnedToProvider));
            f.VerifyTopicClientDoesNotCloseConnection();
        }

        [Test]
        public void PublishAsync_ShouldLogAndReThrowException()
        {
            var f = new LegacyTopicMessagePublisherTestsFixture().ThrowInvalidOperationExceptionWhenCallingTopicClientSendAsync();
            Assert.ThrowsAsync<InvalidOperationException>(() => f.Sut.PublishAsync(f.ApprovedCohortReturnedToProvider));
        }

        [Test]
        public async Task PublishAsync_ShouldSendMessageToTopicClient()
        {
            var f = new LegacyTopicMessagePublisherTestsFixture();
            await f.Sut.PublishAsync(f.ApprovedCohortReturnedToProvider);
            f.VerifyTopicClientIsCalledWithMessage();
        }

        private class LegacyTopicMessagePublisherTestsFixture
        {
            public Mock<ITopicClientFactory> TopicClientFactory;
            public Mock<ILogger<LegacyTopicMessagePublisher>> Logger;
            public Mock<ITopicClient> TopicClient;
            public LegacyTopicMessagePublisher Sut;
            public ApprovedCohortReturnedToProvider ApprovedCohortReturnedToProvider;
            public string ConnectionString;

            private Fixture _fixture;

            public LegacyTopicMessagePublisherTestsFixture()
            {
                _fixture = new Fixture();

                ConnectionString = "XXXXX";
                TopicClientFactory = new Mock<ITopicClientFactory>();
                TopicClient = new Mock<ITopicClient>();
                TopicClientFactory.Setup(x => x.CreateClient(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(TopicClient.Object);
                Logger = new Mock<ILogger<LegacyTopicMessagePublisher>>();
                ApprovedCohortReturnedToProvider = _fixture.Create<ApprovedCohortReturnedToProvider>();

                Sut = new LegacyTopicMessagePublisher(TopicClientFactory.Object, Logger.Object, ConnectionString);
            }

            public LegacyTopicMessagePublisherTestsFixture ThrowInvalidOperationExceptionWhenCallingTopicClientSendAsync()
            {
                TopicClient.As<ISenderClient>().Setup(x => x.SendAsync(It.IsAny<Message>()))
                    .ThrowsAsync(new InvalidOperationException());
                return this;
            }

            public LegacyTopicMessagePublisherTestsFixture ThrowInvalidOperationExceptionWhenCreatingTopicClient()
            {
                TopicClientFactory.Setup(x => x.CreateClient(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws<InvalidOperationException>();
                return this;
            }

            public void VerifyTopicClientFactoryReceivesConnectionStringAndMessageGroupName(string groupName)
            {
                TopicClientFactory.Verify(x=>x.CreateClient(ConnectionString, groupName));
            }

            public void VerifyTopicClientIsCalledWithMessage()
            {
                TopicClient.As<ISenderClient>().Verify(x=>x.SendAsync(It.Is<Message>(m=>m != null)), Times.Once);
            }

            public void VerifyTopicClientDoesNotCloseConnection()
            {
                TopicClient.As<IClientEntity>().Verify(x=>x.CloseAsync(), Times.Never);
            }
        }
    }
}


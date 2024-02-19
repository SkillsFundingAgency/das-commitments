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
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class LegacyTopicMessagePublisherTests
    {
        [Test]
        public async Task PublishAsync_ShouldCreateTopicClientPassingCorrectParameters()
        {
            var fixture = new LegacyTopicMessagePublisherTestsFixture();
            await fixture.Sut.PublishAsync(fixture.ApprovedCohortReturnedToProvider);
            fixture.VerifyTopicClientFactoryReceivesConnectionStringAndMessageGroupName("approved_cohort_returned_to_provider");
        }

        [Test]
        public async Task PublishAsync_ShouldCreateTopicClientPassingInNameOfClassAsMessageGroupName()
        {
            var fixture = new LegacyTopicMessagePublisherTestsFixture();
            var @event = new SimpleTestObject();
            await fixture.Sut.PublishAsync(@event);
            fixture.VerifyTopicClientFactoryReceivesConnectionStringAndMessageGroupName(@event.GetType().Name);
        }

        [Test]
        public async Task PublishAsync_ShouldNotCloseTopicClientConnectionIfAlreadyClosing()
        {
            var fixture = new LegacyTopicMessagePublisherTestsFixture();
            fixture.TopicClient.Setup(x => x.IsClosedOrClosing).Returns(true);
            await fixture.Sut.PublishAsync(fixture.ApprovedCohortReturnedToProvider);
            fixture.VerifyTopicClientDoesNotCloseConnection();
        }

        [Test]
        public void PublishAsync_ShouldNotCloseTopicClientConnectionWhenClientIsNull()
        {
            var fixture = new LegacyTopicMessagePublisherTestsFixture().ThrowInvalidOperationExceptionWhenCreatingTopicClient();
            Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Sut.PublishAsync(fixture.ApprovedCohortReturnedToProvider));
            fixture.VerifyTopicClientDoesNotCloseConnection();
        }

        [Test]
        public void PublishAsync_ShouldLogAndReThrowException()
        {
            var fixture = new LegacyTopicMessagePublisherTestsFixture().ThrowInvalidOperationExceptionWhenCallingTopicClientSendAsync();
            Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Sut.PublishAsync(fixture.ApprovedCohortReturnedToProvider));
        }

        [Test]
        public async Task PublishAsync_ShouldSendMessageToTopicClient()
        {
            var fixture = new LegacyTopicMessagePublisherTestsFixture();
            await fixture.Sut.PublishAsync(fixture.ApprovedCohortReturnedToProvider);
            fixture.VerifyTopicClientIsCalledWithMessage();
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

    public class SimpleTestObject
    {
        public SimpleTestObject()
        {
        }
    }
}
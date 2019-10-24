using System;
using System.Threading.Tasks;
using AutoFixture;
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

        private class LegacyTopicMessagePublisherTestsFixture
        {
            public Mock<ITopicClientFactory> TopicClientFactory;
            public Mock<ILogger<LegacyTopicMessagePublisher>> Logger;
            public LegacyTopicMessagePublisher Sut;
            public ApprovedCohortReturnedToProvider ApprovedCohortReturnedToProvider;
            public string ConnectionString;

            private Fixture _fixture;

            public LegacyTopicMessagePublisherTestsFixture()
            {
                _fixture = new Fixture();

                ConnectionString = "XXXXX";
                TopicClientFactory = new Mock<ITopicClientFactory>();
                Logger = new Mock<ILogger<LegacyTopicMessagePublisher>>();
                ApprovedCohortReturnedToProvider = _fixture.Create<ApprovedCohortReturnedToProvider>();

                Sut = new LegacyTopicMessagePublisher(TopicClientFactory.Object, Logger.Object, ConnectionString);

            }

            public void VerifyTopicClientFactoryReceivesConnectionStringAndMessageGroupName(string groupName)
            {
                TopicClientFactory.Verify(x=>x.CreateClient(ConnectionString, groupName));
            }
        }
    }
}


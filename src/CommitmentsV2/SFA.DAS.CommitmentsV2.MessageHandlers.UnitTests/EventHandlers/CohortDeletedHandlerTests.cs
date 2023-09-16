using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class CohortDeletedHandlerTests
    {
        [TestCase(Party.None)]
        public async Task Handle_WhenCohortDeletedEventIsRaisedAndProviderHasNotApprovedIt_ThenShouldRelayAnyMessagesToAzureServiceBus(Party approvedBy)
        {
            var fixture = new CohortDeletedHandlerTestsFixture()
                .WithCohortDeletedEvent(approvedBy);
            
            await fixture.Handle();
            
            fixture.VerifyNoRelayingMessageIsSent();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.None | Party.Provider)]
        [TestCase(Party.Employer | Party.Provider)]
        [TestCase(Party.TransferSender | Party.Provider)]
        public async Task Handle_WhenCohortDeletedEventIsRaisedAndProviderHasApprovedIt_ThenShouldRelayMessageToAzureServiceBus(Party approvedBy)
        {
            var fixture = new CohortDeletedHandlerTestsFixture()
                .WithCohortDeletedEvent(approvedBy);
            
            await fixture.Handle();
            
            fixture.VerifyPropertiesAreMappedCorrectlyWhenRelayingMessage();
        }
    }

    public class CohortDeletedHandlerTestsFixture
    {
        public Mock<ILegacyTopicMessagePublisher> LegacyTopicMessagePublisher;
        public CohortDeletedEventHandler Sut;
        public CohortDeletedEvent CohortDeletedEvent;
        public long CohortId;
        public long ProviderId;
        public long AccountId;

        public CohortDeletedHandlerTestsFixture()
        {
            var autoFixture = new Fixture();

            CohortId = autoFixture.Create<long>();
            ProviderId = autoFixture.Create<long>();
            AccountId = autoFixture.Create<long>();

            LegacyTopicMessagePublisher = new Mock<ILegacyTopicMessagePublisher>();

            Sut = new CohortDeletedEventHandler(LegacyTopicMessagePublisher.Object, Mock.Of<ILogger<CohortDeletedEventHandler>>());
        }

        public Task Handle()
        {
            return Sut.Handle(CohortDeletedEvent, Mock.Of<IMessageHandlerContext>());
        }

        public CohortDeletedHandlerTestsFixture WithCohortDeletedEvent(Party approvedBy)
        {
            CohortDeletedEvent = new CohortDeletedEvent(CohortId, AccountId, ProviderId, approvedBy, DateTime.Now);
            return this;
        }

        public void VerifyPropertiesAreMappedCorrectlyWhenRelayingMessage()
        {
            LegacyTopicMessagePublisher.Verify(x => x.PublishAsync(It.Is<ProviderCohortApprovalUndoneByEmployerUpdate>(p =>
                p.AccountId == AccountId &&
                p.ProviderId == ProviderId &&
                p.CommitmentId == CohortId)));
        }

        public void VerifyNoRelayingMessageIsSent()
        {
            LegacyTopicMessagePublisher.Verify(x => x.PublishAsync(It.IsAny<ProviderCohortApprovalUndoneByEmployerUpdate>()), Times.Never);
        }
    }
}

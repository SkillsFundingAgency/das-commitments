﻿using Microsoft.Extensions.Logging;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class ApprenticeshipUpdatesAcceptedEventHandlerTests
    {
        [Test]
        public async Task Handle_WhenApprenticeshipUpdatedApprovedEventIsReceived_ThenShouldRelayAnyMessagesToAzureServiceBus()
        {
            var fixture = new ApprenticeshipUpdatesAcceptedEventHandlerTestsFixture()
                .WithApprenticeshipUpdatedApprovedEvent();
            
            await fixture.Handle();
            
            fixture.VerifyPropertiesAreMappedCorrectlyWhenRelayingMessage();
        }
    }

    public class ApprenticeshipUpdatesAcceptedEventHandlerTestsFixture
    {
        public Mock<ILegacyTopicMessagePublisher> LegacyTopicMessagePublisher;
        public ApprenticeshipUpdatedApprovedEventHandler Sut;
        public ApprenticeshipUpdatedApprovedEvent ApprenticeshipUpdatedApprovedEvent;
        public long ApprenticeshipId;
        public long ProviderId;
        public long AccountId;

        public ApprenticeshipUpdatesAcceptedEventHandlerTestsFixture()
        {
            var autoFixture = new Fixture();

            ApprenticeshipId = autoFixture.Create<long>();
            ProviderId = autoFixture.Create<long>();
            AccountId = autoFixture.Create<long>();
            
            LegacyTopicMessagePublisher = new Mock<ILegacyTopicMessagePublisher>();

            var fixture = new Fixture();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var cohort = new Cohort()
              .Set(c => c.Id, 111)
              .Set(c => c.EmployerAccountId, AccountId)
              .Set(c => c.ProviderId, ProviderId)
              .Set(c => c.AccountLegalEntity, new AccountLegalEntity());

           var apprenticeshipDetails = fixture.Build<Apprenticeship>()
             .With(s => s.Id, ApprenticeshipId)
             .With(s => s.Cohort, cohort)
             .With(s => s.EndDate, DateTime.UtcNow)
             .With(s => s.CompletionDate, DateTime.UtcNow.AddDays(10))
             .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
             .Without(s => s.PriceHistory)
             .Without(s => s.ApprenticeshipUpdate)
             .Without(s => s.DataLockStatus)
             .Without(s => s.EpaOrg)
             .Without(s => s.Continuation)
             .Without(s => s.PreviousApprenticeship)
             .Without(s => s.ApprenticeshipConfirmationStatus)
             .Create();

         var db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

            db.Apprenticeships.Add(apprenticeshipDetails);
            db.SaveChanges();

            Sut = new ApprenticeshipUpdatedApprovedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => db), LegacyTopicMessagePublisher.Object, Mock.Of<ILogger<ApprenticeshipUpdatedApprovedEventHandler>>());
        }

        public Task Handle()
        {
            return Sut.Handle(ApprenticeshipUpdatedApprovedEvent, Mock.Of<IMessageHandlerContext>());
        }

        public ApprenticeshipUpdatesAcceptedEventHandlerTestsFixture WithApprenticeshipUpdatedApprovedEvent()
        {
            ApprenticeshipUpdatedApprovedEvent = new ApprenticeshipUpdatedApprovedEvent { ApprenticeshipId = ApprenticeshipId };
            return this;
        }

        public void VerifyPropertiesAreMappedCorrectlyWhenRelayingMessage()
        {
            LegacyTopicMessagePublisher.Verify(x => x.PublishAsync(It.Is<ApprenticeshipUpdateAccepted>(p =>
                p.AccountId == AccountId &&
                p.ProviderId == ProviderId &&
                p.ApprenticeshipId == ApprenticeshipId)));
        }
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.OverlappingTrainingDateRequestNotificationToServiceDesk;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.Notifications.Messages.Commands;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.OverlappingTrainingDate
{
    [TestFixture]
    public class OverlappingTrainingDateRequestNotificationToServiceDeskTests
    {
        [Test]
        public async Task NotifiedServiceDeskOn_Updated_Successfully()
        {
            using var fixture = new OverlappingTrainingDateRequestNotificationToServiceDeskTestsFixture();
            await fixture.Handle();

            fixture.Verify_NotifiedServiceDeskOn_Updated();
        }

        [Test]
        public async Task Verify_EmailCommandSent()
        {
            using var fixture = new OverlappingTrainingDateRequestNotificationToServiceDeskTestsFixture();
            await fixture.Handle();

            fixture.Verify_EmailCommandSent();
        }

        [TestCase(Types.OverlappingTrainingDateRequestStatus.Rejected)]
        [TestCase(Types.OverlappingTrainingDateRequestStatus.Resolved)]
        public async Task Verify_EmailIsSentOnlyForPendingRequests(Types.OverlappingTrainingDateRequestStatus status)
        {
            using var fixture = new OverlappingTrainingDateRequestNotificationToServiceDeskTestsFixture();
            fixture.SetStatus(status);
            await fixture.Handle();

            fixture.Verify_EmailCommandIsNotSent();
        }

        [TestCase(Types.PaymentStatus.Withdrawn)]
        [TestCase(Types.PaymentStatus.Completed)]
        public async Task Verify_EmailIsSentForStoppedOrCompletedApprenticeships(Types.PaymentStatus status)
        {
            using var fixture = new OverlappingTrainingDateRequestNotificationToServiceDeskTestsFixture();
            fixture.SetPaymentStatus(status);
            await fixture.Handle();

            fixture.Verify_EmailCommandSent();
        }

        [TestCase(Types.PaymentStatus.Active)]
        [TestCase(Types.PaymentStatus.Paused)]
        public async Task Verify_Email_NotSentForLiveOrPausedApprenticeships(Types.PaymentStatus status)
        {
            using var fixture = new OverlappingTrainingDateRequestNotificationToServiceDeskTestsFixture();
            fixture.SetPaymentStatus(status);

            await fixture.Handle();

            fixture.Verify_EmailCommandIsNotSent();
        }

        [Test]
        public async Task Verify_SecondEmailIsNotTriggered()
        {
            using var fixture = new OverlappingTrainingDateRequestNotificationToServiceDeskTestsFixture();
            fixture.SetNotifiedServiceDeskOn();
            await fixture.Handle();

            fixture.Verify_EmailCommandIsNotSent();
        }

        [Test]
        public async Task Verify_EmailIsSentOnlyForExpiredRecords()
        {
            using var fixture = new OverlappingTrainingDateRequestNotificationToServiceDeskTestsFixture();
            fixture.SetCreatedOn(-5);
            await fixture.Handle();

            fixture.Verify_EmailCommandIsNotSent();
        }

        public class OverlappingTrainingDateRequestNotificationToServiceDeskTestsFixture : IDisposable
        {
            OverlappingTrainingDateRequestNotificationToServiceDeskCommandHandler _sut;
            OverlappingTrainingDateRequestNotificationToServiceDeskCommand _command;
            ProviderCommitmentsDbContext Db;
            Mock<ICurrentDateTime> _currentDateTime;
            Mock<IMessageSession> _messageSession;
            DateTime currentProxyDateTime;
            CommitmentsV2Configuration _configuration;

            public OverlappingTrainingDateRequestNotificationToServiceDeskTestsFixture()
            {

                Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                          .UseInMemoryDatabase(Guid.NewGuid().ToString())
                          .EnableSensitiveDataLogging()
                          .Options);

                _command = new OverlappingTrainingDateRequestNotificationToServiceDeskCommand();
                currentProxyDateTime = new DateTime(2022, 2, 1);
                _currentDateTime = new Mock<ICurrentDateTime>();
                _currentDateTime.Setup(x => x.UtcNow).Returns(currentProxyDateTime);
                _messageSession = new Mock<IMessageSession>();

                _configuration = new CommitmentsV2Configuration()
                {
                    ZenDeskEmailAddress = "abc@zendesk.com"
                };

                _sut = new OverlappingTrainingDateRequestNotificationToServiceDeskCommandHandler(
                     new Lazy<ProviderCommitmentsDbContext>(() => Db),
                     _currentDateTime.Object,
                     _messageSession.Object,
                     _configuration,
                     Mock.Of<ILogger<OverlappingTrainingDateRequestNotificationToServiceDeskCommandHandler>>()
                    );

                SeedData();
            }

            public async Task Handle()
            {
                await _sut.Handle(_command, CancellationToken.None);
            }

            internal void SetStatus(Types.OverlappingTrainingDateRequestStatus status)
            {
                var x = Db.OverlappingTrainingDateRequests.FirstOrDefault();
                x.Status = status;
                Db.SaveChanges();
            }

            internal void SetPaymentStatus(Types.PaymentStatus status)
            {
                var x = Db.OverlappingTrainingDateRequests.FirstOrDefault();
                x.PreviousApprenticeship.PaymentStatus = status;
                Db.SaveChanges();
            }

            internal void SetNotifiedServiceDeskOn()
            {
                var x = Db.OverlappingTrainingDateRequests.FirstOrDefault();
                x.NotifiedServiceDeskOn = DateTime.UtcNow;
                Db.SaveChanges();
            }

            internal void Verify_EmailCommandIsNotSent()
            {
                _messageSession.Verify(y => y.Send(It.IsAny<SendEmailCommand>(), It.IsAny<SendOptions>()), Times.Never);
            }

            internal void SetCreatedOn(int days)
            {
                var x = Db.OverlappingTrainingDateRequests.FirstOrDefault();
                x.CreatedOn = currentProxyDateTime.AddDays(days);
                Db.SaveChanges();
            }

            internal void Verify_EmailCommandSent()
            {
                var x = Db.OverlappingTrainingDateRequests.FirstOrDefault();
                _messageSession.Verify(y => y.Send(It.Is<SendEmailCommand>(z =>
               z.RecipientsAddress == _configuration.ZenDeskEmailAddress &&
               z.TemplateId == OverlappingTrainingDateRequestNotificationToServiceDeskCommandHandler.TemplateId &&
               z.Tokens["NewProviderUkprn"] == x.DraftApprenticeship.Cohort.ProviderId.ToString() &&
               z.Tokens["ULN"] == x.DraftApprenticeship.Uln &&
               z.Tokens["OldProviderUkprn"] == x.PreviousApprenticeship.Cohort.ProviderId.ToString()
               ), It.IsAny<SendOptions>()), Times.Once);
            }

            internal void Verify_NotifiedServiceDeskOn_Updated()
            {
                var overlappingTrainingDateRequest = Db.OverlappingTrainingDateRequests.FirstOrDefault();
                Assert.IsNotNull(overlappingTrainingDateRequest);
                Assert.AreEqual(_currentDateTime.Object.UtcNow.Year, overlappingTrainingDateRequest.NotifiedServiceDeskOn.Value.Year);
                Assert.AreEqual(_currentDateTime.Object.UtcNow.Month, overlappingTrainingDateRequest.NotifiedServiceDeskOn.Value.Month);
                Assert.AreEqual(_currentDateTime.Object.UtcNow.Day, overlappingTrainingDateRequest.NotifiedServiceDeskOn.Value.Day);
            }

            private void SeedData()
            {
                var fixture = new Fixture();
                fixture.Behaviors.Add(new OmitOnRecursionBehavior());

                var oldProvider = new CommitmentsV2.Models.Provider()
                        .Set(x => x.UkPrn, 1)
                        .Set(x => x.Name, "OldProvider");
                var account = new Account()
                    .Set(a => a.Id, 1)
                    .Set(a => a.Name, "OldEmployerName");
                var oldAccountLegalEntity = new AccountLegalEntity()
                    .Set(oal => oal.Id, 1)
                    .Set(oal => oal.Name, "OldAccountLegalEntity")
                    .Set(oal => oal.Account, account);

                var Cohort = new CommitmentsV2.Models.Cohort()
                   .Set(c => c.Id, 1)
                   .Set(c => c.Provider, oldProvider)
                   .Set(c => c.AccountLegalEntity, oldAccountLegalEntity);

                var Apprenticeship = fixture.Build<CommitmentsV2.Models.Apprenticeship>()
                 .With(s => s.Cohort, Cohort)
                 .With(s => s.PaymentStatus, Types.PaymentStatus.Active)
                 .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
                 .With(s => s.EndDate, DateTime.UtcNow.AddDays(100))
                 .With(s => s.PaymentStatus, Types.PaymentStatus.Withdrawn)
                 .Without(s => s.DataLockStatus)
                 .Without(s => s.EpaOrg)
                 .Without(s => s.ApprenticeshipUpdate)
                 .Without(s => s.Continuation)
                 .Without(s => s.PreviousApprenticeship)
                 .Without(s => s.ApprenticeshipConfirmationStatus)
                 .Without(s => s.OverlappingTrainingDateRequests)
                 .Create();

                var newProvider = new CommitmentsV2.Models.Provider()
                     .Set(x => x.UkPrn, 2)
                     .Set(x => x.Name, "NewProvider");
                var newAccount = new Account()
                    .Set(a => a.Id, 2)
                    .Set(a => a.Name, "NewEmployerName");
                var newAccountLegalEntity = new AccountLegalEntity()
                    .Set(oal => oal.Id, 2)
                    .Set(oal => oal.Name, "NewAccountLegalEntity")
                    .Set(oal => oal.Account, account);

                var newCohort = new CommitmentsV2.Models.Cohort()
                   .Set(c => c.Id, 2)
                   .Set(c => c.Provider, newProvider)
                   .Set(c => c.AccountLegalEntity, newAccountLegalEntity);

                var draftApprenticeship = fixture.Build<CommitmentsV2.Models.DraftApprenticeship>()
                 .With(s => s.Cohort, Cohort)
                 .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
                 .With(s => s.EndDate, DateTime.UtcNow.AddDays(100))
                 .Without(s => s.EpaOrg)
                 .Without(s => s.ApprenticeshipUpdate)
                 .Without(s => s.PreviousApprenticeship)
                 .Without(s => s.ApprenticeshipConfirmationStatus)
                 .Without(s => s.OverlappingTrainingDateRequests)
                 .Create();

                var oltd = new OverlappingTrainingDateRequest()
                    .Set(x => x.Id, 1)
                    .Set(x => x.Status, Types.OverlappingTrainingDateRequestStatus.Pending)
                    .Set(x => x.PreviousApprenticeship, Apprenticeship)
                    .Set(x => x.CreatedOn, currentProxyDateTime.AddDays(-29))
                    .Set(x => x.DraftApprenticeship, draftApprenticeship);

                Db.Apprenticeships.Add(Apprenticeship);
                Db.DraftApprenticeships.Add(draftApprenticeship);
                Db.OverlappingTrainingDateRequests.Add(oltd);
                Db.SaveChanges();
            }

            public void Dispose()
            {
                Db?.Dispose();
            }
        }
    }
}
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.OverlappingTrainingDateRequestNotificationToEmployer;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.OverlappingTrainingDate
{
    [TestFixture]
    public class OverlappingTrainingDateRequestNotificationToEmployerTests
    {
        [Test]
        public async Task NotifiedEmployerOn_Updated_Successfully()
        {
            using var fixture = new OverlappingTrainingDateRequestNotificationToEmployerTestsFixture();
            await fixture.Handle();

            fixture.Verify_NotifiedEmployerOn_Updated();
        }

        [Test]
        public async Task Verify_EmailCommandSent()
        {
            using var fixture = new OverlappingTrainingDateRequestNotificationToEmployerTestsFixture();
            await fixture.Handle();

            fixture.Verify_EmailCommandSent();
        }

        [TestCase(Types.OverlappingTrainingDateRequestStatus.Rejected)]
        [TestCase(Types.OverlappingTrainingDateRequestStatus.Resolved)]
        public async Task Verify_EmailIsSentOnlyForPendingRequests(Types.OverlappingTrainingDateRequestStatus status)
        {
            using var fixture = new OverlappingTrainingDateRequestNotificationToEmployerTestsFixture();
            fixture.SetStatus(status);
            await fixture.Handle();

            fixture.Verify_EmailCommandIsNotSent();
        }

        [Test]
        public async Task Verify_SecondChaserEmailIsNotTriggered()
        {
            using var fixture = new OverlappingTrainingDateRequestNotificationToEmployerTestsFixture();
            fixture.SetNotifiedEmployerOn();
            await fixture.Handle();

            fixture.Verify_EmailCommandIsNotSent();
        }

        public async Task Verify_EmailIsSentOnlyForNonExpiredRecords()
        {
            using var fixture = new OverlappingTrainingDateRequestNotificationToEmployerTestsFixture();
            fixture.SetCreatedOn(-5);
            await fixture.Handle();

            fixture.Verify_EmailCommandIsNotSent();
        }

        [Test]
        public async Task Verify_DontSendEmailWhenServiceDeskAlreadyNotified()
        {
            using var fixture = new OverlappingTrainingDateRequestNotificationToEmployerTestsFixture();
            fixture.SetServiceDeskNotifiedOn();
            await fixture.Handle();

            fixture.Verify_EmailCommandIsNotSent();
        }

        [Test]
        public async Task Verify_SendFailureBubblesUpAndEarlierRecordRemainsPersisted()
        {
            using var fixture = new OverlappingTrainingDateRequestNotificationToEmployerTestsFixture();
            fixture.AddSecondValidRecord();
            fixture.Setup_SendFailsForRecord(recordId: 2);

            Assert.ThrowsAsync<Exception>(async () => await fixture.Handle());

            fixture.Verify_NotifiedEmployerOn_Updated(recordId: 1);
            fixture.Verify_NotifiedEmployerOn_NotUpdated(recordId: 2);
        }

        public class OverlappingTrainingDateRequestNotificationToEmployerTestsFixture : IDisposable
        {
            OverlappingTrainingDateRequestNotificationToEmployerCommandHandler _sut;
            OverlappingTrainingDateRequestNotificationToEmployerCommand _command = null;
            ProviderCommitmentsDbContext Db;
            Mock<ICurrentDateTime> _currentDateTime;
            Mock<IMessageSession> _messageSession;
            Mock<IEncodingService> _encodingService;
            DateTime currentProxyDateTime;
            CommitmentsV2Configuration _configuration;

            public OverlappingTrainingDateRequestNotificationToEmployerTestsFixture()
            {


                Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                          .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                          .EnableSensitiveDataLogging()
                          .Options);

                currentProxyDateTime = new DateTime(2022, 2, 1);
                _currentDateTime = new Mock<ICurrentDateTime>();
                _currentDateTime.Setup(x => x.UtcNow).Returns(currentProxyDateTime);
                _messageSession = new Mock<IMessageSession>();
                _encodingService = new Mock<IEncodingService>();

                _configuration = new CommitmentsV2Configuration()
                {
                    ZenDeskEmailAddress = "abc@zendesk.com",
                    EmployerCommitmentsBaseUrl = "https://employerurl"
                };

                _encodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.AccountId)).Returns(() => "EMPLOYERHASHEDID");
                _encodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.ApprenticeshipId)).Returns(() => "APPRENTICESHIPHASHEDID");

                _sut = new OverlappingTrainingDateRequestNotificationToEmployerCommandHandler(
                     new Lazy<ProviderCommitmentsDbContext>(() => Db),
                     _currentDateTime.Object,
                     _messageSession.Object,
                     _configuration,
                     _encodingService.Object,
                     Mock.Of<ILogger<OverlappingTrainingDateRequestNotificationToEmployerCommandHandler>>()
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

            internal void SetNotifiedEmployerOn()
            {
                var x = Db.OverlappingTrainingDateRequests.FirstOrDefault();
                x.NotifiedEmployerOn = DateTime.UtcNow;
                Db.SaveChanges();
            }

            internal void SetServiceDeskNotifiedOn()
            {
                var x = Db.OverlappingTrainingDateRequests.FirstOrDefault();
                x.NotifiedServiceDeskOn = DateTime.UtcNow;
                Db.SaveChanges();
            }

            internal void Setup_SendFailsForRecord(long recordId)
            {
                var record = Db.OverlappingTrainingDateRequests.Single(r => r.Id == recordId);
                var failingUln = record.PreviousApprenticeship.Uln;
                _messageSession
                    .Setup(x => x.Send(It.Is<SendEmailToEmployerCommand>(c => c.Tokens["ULN"] == failingUln), It.IsAny<SendOptions>()))
                    .ThrowsAsync(new Exception("Simulated send failure"));
            }
          
            internal void Verify_EmailCommandIsNotSent()
            {
                _messageSession.Verify(y => y.Send(It.IsAny<SendEmailToEmployerCommand>(), It.IsAny<SendOptions>()), Times.Never);
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
                _messageSession.Verify(y => y.Send(It.Is<SendEmailToEmployerCommand>(z =>
                           z.Template == OverlappingTrainingDateRequestNotificationToEmployerCommandHandler.TemplateId &&
                           z.Tokens["RequestRaisedDate"] == x.CreatedOn.ToString("dd-MM-yyyy") &&
                           z.Tokens["ULN"] == x.PreviousApprenticeship.Uln &&
                           z.Tokens["Apprentice"] == x.PreviousApprenticeship.FirstName + " " + x.PreviousApprenticeship.LastName &&
                           z.Tokens["URL"] == $"{_configuration.EmployerCommitmentsBaseUrl}/EMPLOYERHASHEDID/apprentices/APPRENTICESHIPHASHEDID/details"),
                           It.IsAny<SendOptions>()), Times.Once);
            }

            internal void Verify_NotifiedEmployerOn_Updated(long recordId = 1)
            {
                var overlappingTrainingDateRequest = Db.OverlappingTrainingDateRequests.Single(r => r.Id == recordId);
                Assert.That(overlappingTrainingDateRequest, Is.Not.Null);
                Assert.Multiple(() =>
                {
                    Assert.That(overlappingTrainingDateRequest.NotifiedEmployerOn.Value.Year, Is.EqualTo(_currentDateTime.Object.UtcNow.Year));
                    Assert.That(overlappingTrainingDateRequest.NotifiedEmployerOn.Value.Month, Is.EqualTo(_currentDateTime.Object.UtcNow.Month));
                    Assert.That(overlappingTrainingDateRequest.NotifiedEmployerOn.Value.Day, Is.EqualTo(_currentDateTime.Object.UtcNow.Day));
                });
            }

            internal void Verify_NotifiedEmployerOn_NotUpdated(long recordId)
            {
                var overlappingTrainingDateRequest = Db.OverlappingTrainingDateRequests.Single(r => r.Id == recordId);
                Assert.That(overlappingTrainingDateRequest.NotifiedEmployerOn, Is.Null);
            }

            internal void AddSecondValidRecord()
            {
                var sourceRecord = Db.OverlappingTrainingDateRequests.Single(r => r.Id == 1);
                var secondPreviousApprenticeship = new CommitmentsV2.Models.Apprenticeship()
                    .Set(x => x.Id, sourceRecord.PreviousApprenticeship.Id + 1)
                    .Set(x => x.Uln, $"{sourceRecord.PreviousApprenticeship.Uln}2")
                    .Set(x => x.FirstName, sourceRecord.PreviousApprenticeship.FirstName)
                    .Set(x => x.LastName, sourceRecord.PreviousApprenticeship.LastName)
                    .Set(x => x.Cohort, sourceRecord.PreviousApprenticeship.Cohort)
                    .Set(x => x.PaymentStatus, sourceRecord.PreviousApprenticeship.PaymentStatus)
                    .Set(x => x.StartDate, sourceRecord.PreviousApprenticeship.StartDate)
                    .Set(x => x.EndDate, sourceRecord.PreviousApprenticeship.EndDate);

                var secondRecord = new OverlappingTrainingDateRequest()
                    .Set(x => x.Id, 2)
                    .Set(x => x.Status, Types.OverlappingTrainingDateRequestStatus.Pending)
                    .Set(x => x.PreviousApprenticeship, secondPreviousApprenticeship)
                    .Set(x => x.CreatedOn, currentProxyDateTime.AddDays(-20))
                    .Set(x => x.DraftApprenticeship, sourceRecord.DraftApprenticeship);
                Db.Apprenticeships.Add(secondPreviousApprenticeship);
                Db.OverlappingTrainingDateRequests.Add(secondRecord);
                Db.SaveChanges();
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

                var previousApprenticeship = fixture.Build<CommitmentsV2.Models.Apprenticeship>()
                 .With(s => s.Cohort, Cohort)
                 .With(s => s.PaymentStatus, Types.PaymentStatus.Active)
                 .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
                 .With(s => s.EndDate, DateTime.UtcNow.AddDays(100))
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
                    .Set(x => x.PreviousApprenticeship, previousApprenticeship)
                    .Set(x => x.CreatedOn, currentProxyDateTime.AddDays(-20))
                    .Set(x => x.DraftApprenticeship, draftApprenticeship);

                Db.Apprenticeships.Add(previousApprenticeship);
                Db.DraftApprenticeships.Add(draftApprenticeship);
                Db.OverlappingTrainingDateRequests.Add(oltd);
                Db.SaveChanges();
            }

            public void Dispose()
            {
                Db?.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}

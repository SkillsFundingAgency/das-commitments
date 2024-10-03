using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.OverlappingTrainingDateRequestAutomaticStopAfter2Weeks;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Notifications.Messages.Commands;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.OverlappingTrainingDate
{
    public class OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandlerTests
    {
        [TestCase(PaymentStatus.Completed)]
        [TestCase(PaymentStatus.Withdrawn)]
        public async Task Handle_ShouldSendToZenDesk_WhenPaymentStatusIsWithdrawnOrCompleted_No_StopCommandSent(PaymentStatus status)
        {
            using var fixture = new OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandlerTestFixture();
            fixture.SetStatus(status);

            await fixture.Handle();

            fixture.Verify_ZenDesk_EmailCommandSent();
            fixture.Verify_StopCommandIsNotSent();
        }

        [TestCase(PaymentStatus.Active)]
        [TestCase(PaymentStatus.Paused)]
        public async Task Handle_WhenPaymentStatusIs_ActivePaused_Send_StopCommand_DontSendToZenDesk(PaymentStatus status)
        {
            using var fixture = new OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandlerTestFixture();
            fixture.SetStatus(status);

            await fixture.Handle();

            fixture.Verify_ZenDesk_EmailCommandIsNotSent();
            fixture.Verify_StopCommandSent();
        }

        [TestCase(PaymentStatus.Active)]
        public async Task Handle_When_No_Requests_OlderThan_TwoWeeks(PaymentStatus status)
        {
            using var fixture = new OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandlerTestFixture();
            fixture.SetCreatedOn(-1);
            fixture.SetStatus(status);

            await fixture.Handle();

            fixture.Verify_ZenDesk_EmailCommandIsNotSent();
        }

        [TestCase(PaymentStatus.Active)]
        public async Task Handle_When_ServiceDeskNotifiedOn_IsNotNull(PaymentStatus status)
        {
            using var fixture = new OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandlerTestFixture();
            fixture.SetStatus(status);
            fixture.SetNotifiedEmployerOn();
            await fixture.Handle();

            fixture.Verify_ZenDesk_EmailCommandIsNotSent();
        }

        [TestCase(PaymentStatus.Active)]
        public async Task Handle_When_PreviousApprenticeship_WaitingToStart(PaymentStatus status)
        {
            using var fixture = new OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandlerTestFixture();
            fixture.SetStatus(status);
            fixture.SetPreviousApprenticeshipStartDate_And_EndDate(10, 30);
            fixture.SetDraftApprenticeshipStartDate_And_EndDate(15, 25);
            await fixture.Handle();

            fixture.Verify_StopDate_Is_PreviousApprenticeship_StartDate();
            fixture.Verify_ZenDesk_EmailCommandIsNotSent();
            fixture.Verify_StopCommandSent();
        }

        [TestCase(PaymentStatus.Active)]
        public async Task Handle_When_DraftApprenticeship_HasFutureStartDate(PaymentStatus status)
        {
            using var fixture = new OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandlerTestFixture();
            fixture.SetStatus(status);
            fixture.SetPreviousApprenticeshipStartDate_And_EndDate(-10, 20);
            fixture.SetDraftApprenticeshipStartDate_And_EndDate(10, 30);
            await fixture.Handle();

            fixture.Verify_StopDate_Is_Start_Of_CurrentMonth();
            fixture.Verify_ZenDesk_EmailCommandIsNotSent();
            fixture.Verify_StopCommandSent();
        }

        public class OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandlerTestFixture : IDisposable
        {
            OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandler _sut;
            OverlappingTrainingDateRequestAutomaticStopAfter2WeeksCommand _command = null;
            ProviderCommitmentsDbContext Db;
            Mock<ICurrentDateTime> _currentDateTime;
            Mock<IMessageSession> _messageSession;
            DateTime currentProxyDateTime;
            CommitmentsV2Configuration _configuration;

            public OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandlerTestFixture()
            {
                Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                          .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                          .EnableSensitiveDataLogging()
                          .Options);

                currentProxyDateTime = new DateTime(2022, 2, 1);
                _currentDateTime = new Mock<ICurrentDateTime>();
                _currentDateTime.Setup(x => x.UtcNow).Returns(currentProxyDateTime);
                _messageSession = new Mock<IMessageSession>();

                _configuration = new CommitmentsV2Configuration()
                {
                    ZenDeskEmailAddress = "abc@zendesk.com",
                    EmployerCommitmentsBaseUrl = "https://employerurl"
                };

                _sut = new OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandler(
                     new Lazy<ProviderCommitmentsDbContext>(() => Db),
                     _messageSession.Object,
                     _currentDateTime.Object,
                     _configuration,
                     Mock.Of<ILogger<OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandler>>()
                    );

                SeedData();
            }

            public async Task Handle()
            {
                await _sut.Handle(_command, CancellationToken.None);
            }

            internal void SetStatus(PaymentStatus status)
            {
                var x = Db.OverlappingTrainingDateRequests.FirstOrDefault();
                x.PreviousApprenticeship.PaymentStatus = status;
                Db.SaveChanges();
            }

            internal void SetNotifiedEmployerOn()
            {
                var x = Db.OverlappingTrainingDateRequests.FirstOrDefault();
                x.NotifiedEmployerOn = DateTime.UtcNow;
                Db.SaveChanges();
            }

            internal void Verify_ZenDesk_EmailCommandSent()
            {
                var x = Db.OverlappingTrainingDateRequests.FirstOrDefault();

                var tokens = new Dictionary<string, string>
                {
                    { "RequestCreatedByProviderEmail", "Not available" },
                    { "LastUpdatedByProviderEmail", x.DraftApprenticeship?.Cohort?.LastUpdatedByProviderEmail },
                    { "ULN", x.DraftApprenticeship?.Uln },
                    { "NewProviderUkprn", x.DraftApprenticeship?.Cohort?.ProviderId.ToString() },
                    { "OldProviderUkprn", x.PreviousApprenticeship?.Cohort?.ProviderId.ToString() }
                };
                var emailCommand = new SendEmailCommand(
                    OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandler.TemplateId,
                    _configuration.ZenDeskEmailAddress,
                    tokens);

                _messageSession.Verify(y => y.Send(
                   It.Is<SendEmailCommand>(z =>
                       z.TemplateId == OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandler.TemplateId &&
                       z.Tokens.SequenceEqual(tokens)),
                   It.IsAny<SendOptions>()),
                   Times.Once);
            }

            internal void Verify_ZenDesk_EmailCommandIsNotSent()
            {
                _messageSession.Verify(y => y.Send(It.IsAny<SendEmailCommand>(), It.IsAny<SendOptions>()), Times.Never);
            }

            internal void Verify_StopCommandSent()
            {
                var x = Db.OverlappingTrainingDateRequests.FirstOrDefault();

                _messageSession.Verify(y => y.Send(It.IsAny<AutomaticallyStopOverlappingTrainingDateRequestCommand>(),
                           It.IsAny<SendOptions>()), Times.Once);
            }

            internal void Verify_StopCommandIsNotSent()
            {
                _messageSession.Verify(y => y.Send(It.IsAny<AutomaticallyStopOverlappingTrainingDateRequestCommand>(), It.IsAny<SendOptions>()), Times.Never);
            }

            internal void Verify_StopDate_Is_PreviousApprenticeship_StartDate()
            {
                var previousApprenticeship = Db.Apprenticeships.FirstOrDefault();

                _messageSession.Verify(y => y.Send(
                                It.Is<AutomaticallyStopOverlappingTrainingDateRequestCommand>(z =>
                                    z.StopDate == previousApprenticeship.StartDate.Value
                                  ),
                                It.IsAny<SendOptions>()),
                                Times.Once);
            }

            internal void Verify_StopDate_Is_Start_Of_CurrentMonth()
            {
                var firstOfMonth = new DateTime(currentProxyDateTime.Year, currentProxyDateTime.Month, 1);

                _messageSession.Verify(y => y.Send(
                                It.Is<AutomaticallyStopOverlappingTrainingDateRequestCommand>(z =>
                                    z.StopDate == firstOfMonth
                                  ),
                                It.IsAny<SendOptions>()),
                                Times.Once);
            }


            internal void SetCreatedOn(int days)
            {
                var x = Db.OverlappingTrainingDateRequests.FirstOrDefault();
                x.CreatedOn = currentProxyDateTime.AddDays(days);
                Db.SaveChanges();
            }

            internal void SetPreviousApprenticeshipStartDate_And_EndDate(int startDays, int endDays)
            {
                var x = Db.Apprenticeships.FirstOrDefault();
                x.StartDate = currentProxyDateTime.AddDays(startDays);
                x.EndDate = currentProxyDateTime.AddDays(endDays);
                Db.SaveChanges();
            }

            internal void SetDraftApprenticeshipStartDate_And_EndDate(int startDays, int endDays)
            {
                var x = Db.DraftApprenticeships.FirstOrDefault();
                x.StartDate = currentProxyDateTime.AddDays(startDays);
                x.EndDate = currentProxyDateTime.AddDays(endDays);
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
                   .Set(c => c.Reference, "XXXX")
                   .Set(c => c.Provider, oldProvider)
                   .Set(c => c.AccountLegalEntity, oldAccountLegalEntity);

                var Apprenticeship = fixture.Build<CommitmentsV2.Models.Apprenticeship>()
                 .With(s => s.Cohort, Cohort)
                 .With(s => s.PaymentStatus, PaymentStatus.Active)
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
                    .Set(x => x.Status, OverlappingTrainingDateRequestStatus.Pending)
                    .Set(x => x.PreviousApprenticeship, Apprenticeship)
                    .Set(x => x.CreatedOn, currentProxyDateTime.AddDays(-20))
                    .Set(x => x.DraftApprenticeship, draftApprenticeship)
                    .Set(x => x.NotifiedServiceDeskOn, null);

                Db.Apprenticeships.Add(Apprenticeship);
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

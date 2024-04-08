using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPendingOverlappingTrainingDatesToStop;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.OverlappingTrainingDate
{
    [TestFixture]
    public class GetPendingOverlappingTrainingDatesToStopHandlerTests
    {
        [Test]
        public async Task Verify_RecordsReturned()
        {
            using var fixture = new GetPendingOverlappingTrainingDatesToStopHandlerTestsFixture();
            await fixture.Handle();

            fixture.Verify_RecordsReturned();
        }

        [TestCase(Types.OverlappingTrainingDateRequestStatus.Rejected)]
        [TestCase(Types.OverlappingTrainingDateRequestStatus.Resolved)]
        public async Task Verify_EmailIsSentOnlyForPendingRequests(Types.OverlappingTrainingDateRequestStatus status)
        {
            using var fixture = new GetPendingOverlappingTrainingDatesToStopHandlerTestsFixture();
            fixture.SetStatus(status);
            await fixture.Handle();

            fixture.Verify_NoRecordsReturned();
        }


        [Test]
        public async Task Verify_NoRecordsReturned_ForExpiredRecords()
        {
            using var fixture = new GetPendingOverlappingTrainingDatesToStopHandlerTestsFixture();
            fixture.SetCreatedOn(-28);
            await fixture.Handle();

            fixture.Verify_NoRecordsReturned();
        }

        [Test]
        public async Task Verify_RecordsReturned_After14days_CreatedOn_After_OLTDGoLiveDate()
        {
            using var fixture = new GetPendingOverlappingTrainingDatesToStopHandlerTestsFixture();

            fixture.SetGoLiveDate(-25);

            fixture.SetCreatedOn(-15);

            await fixture.Handle();

            fixture.Verify_RecordsReturned();
        }

        [Test]
        public async Task Verify_NoRecordsReturned_After12days_CreatedOn_After_OLTDGoLiveDate()
        {
            using var fixture = new GetPendingOverlappingTrainingDatesToStopHandlerTestsFixture();

            fixture.SetGoLiveDate(-25);

            fixture.SetCreatedOn(-12);

            await fixture.Handle();

            fixture.Verify_NoRecordsReturned();
        }

        [Test]
        public async Task Verify_EmailIsSent_After28days_CreatedOn_Before_OLTDGoLiveDate()
        {
            using var fixture = new GetPendingOverlappingTrainingDatesToStopHandlerTestsFixture();

            fixture.SetGoLiveDate(-20);

            fixture.SetCreatedOn(-29);

            await fixture.Handle();

            fixture.Verify_RecordsReturned();
        }

        [Test]
        public async Task Verify_NoRecordsReturned_After25days_CreatedOn_Before_OLTDGoLiveDate()
        {
            using var fixture = new GetPendingOverlappingTrainingDatesToStopHandlerTestsFixture();

            fixture.SetGoLiveDate(-20);

            fixture.SetCreatedOn(-25);

            await fixture.Handle();

            fixture.Verify_NoRecordsReturned();
        }

        public class GetPendingOverlappingTrainingDatesToStopHandlerTestsFixture : IDisposable
        {
            GetPendingOverlappingTrainingDatesToStopHandler _sut;
            GetPendingOverlappingTrainingDatesToStopQuery _query;
            ProviderCommitmentsDbContext Db;
            Mock<ICurrentDateTime> _currentDateTime;
            DateTime currentProxyDateTime;
            CommitmentsV2Configuration _configuration;
            GetPendingOverlappingTrainingDatesToStopResult _queryResult;

            public GetPendingOverlappingTrainingDatesToStopHandlerTestsFixture()
            {

                Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                          .UseInMemoryDatabase(Guid.NewGuid().ToString())
                          .EnableSensitiveDataLogging()
                          .Options);

                currentProxyDateTime = new DateTime(2022, 2, 1);
                _currentDateTime = new Mock<ICurrentDateTime>();
                _currentDateTime.Setup(x => x.UtcNow).Returns(currentProxyDateTime);

                _configuration = new CommitmentsV2Configuration()
                {
                    OLTD_GoLiveDate = _currentDateTime.Object.UtcNow.AddDays(-5)
                };

                _sut = new GetPendingOverlappingTrainingDatesToStopHandler(
                     new Lazy<ProviderCommitmentsDbContext>(() => Db),
                     _currentDateTime.Object,
                     _configuration,
                     Mock.Of<ILogger<GetPendingOverlappingTrainingDatesToStopHandler>>()
                    );

                SeedData();
            }

            public async Task Handle()
            {
                _queryResult = await _sut.Handle(_query, CancellationToken.None);
            }

            internal void SetStatus(Types.OverlappingTrainingDateRequestStatus status)
            {
                var x = Db.OverlappingTrainingDateRequests.FirstOrDefault();
                x.Status = status;
                Db.SaveChanges();
            }

            internal void SetNotifiedServiceDeskOn()
            {
                var x = Db.OverlappingTrainingDateRequests.FirstOrDefault();
                x.NotifiedServiceDeskOn = DateTime.UtcNow;
                Db.SaveChanges();
            }

            internal void SetCreatedOn(int days)
            {
                var x = Db.OverlappingTrainingDateRequests.FirstOrDefault();
                x.CreatedOn = currentProxyDateTime.AddDays(days);
                Db.SaveChanges();
            }
            internal void SetGoLiveDate(int daysAgo)
            {
                _configuration.OLTD_GoLiveDate = currentProxyDateTime.AddDays(daysAgo);
            }

            internal void Verify_NoRecordsReturned()
            {
                _queryResult.OverlappingTrainingDateRequests.Should().BeNullOrEmpty();
            }

            internal void Verify_RecordsReturned()
            {
                _queryResult.OverlappingTrainingDateRequests.Should().NotBeNullOrEmpty();
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
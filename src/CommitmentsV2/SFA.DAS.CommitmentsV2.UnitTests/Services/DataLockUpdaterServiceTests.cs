﻿using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class DataLockUpdaterServiceTests
    {
        private ProviderCommitmentsDbContext Db { get; set; }
        private Mock<IApprovalsOuterApiClient> _outerApiClient;
        private CommitmentPaymentsWebJobConfiguration _config;
        private Mock<IFilterOutAcademicYearRollOverDataLocks> _filterOutAcademicYearRollOverDataLocks;
        private Fixture _fixture;
        private DataLockUpdaterService _dataLockUpdater;

        public string LegalEntityIdentifier;
        public OrganisationType organisationType;
        public List<Apprenticeship> SeedApprenticeships;
        public List<DataLockStatus> SeedDataLocks;
        public List<ApprenticeshipUpdate> SeedApprenticeshipUpdates;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            LegalEntityIdentifier = "SC171417";
            organisationType = OrganisationType.CompaniesHouse;
            SeedApprenticeships = new List<Apprenticeship>();
            SeedApprenticeshipUpdates = new List<ApprenticeshipUpdate>();
            SeedDataLocks = new List<DataLockStatus>();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                 .UseInMemoryDatabase(Guid.NewGuid().ToString(), new InMemoryDatabaseRoot())
                 .EnableSensitiveDataLogging()
                 .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning))
                 .Options);

            SeedApprenticeship(1, PaymentStatus.Active);
            SeedApprenticeshipUpdate(SeedApprenticeships[0].Id, PaymentStatus.Active, SeedApprenticeships[0]);
            SeedDataLock(SeedApprenticeships[0], 1, 1, "25-6-01/06/2016", DateTime.Now, DataLockErrorCode.Dlock03, SeedApprenticeshipUpdates[0]);

            var apimResponse = new GetDataLockStatusListResponse
            {
                DataLockStatuses = SeedDataLocks
            };

            _outerApiClient = new Mock<IApprovalsOuterApiClient>();
            _outerApiClient
                .Setup(x => x.GetWithRetry<GetDataLockStatusListResponse>(new GetDataLockEventsRequest(It.IsAny<long>())))
                .ReturnsAsync(apimResponse);

            _dataLockUpdater = new DataLockUpdaterService(
                Mock.Of<ILogger<DataLockUpdaterService>>(),
                new Lazy<ProviderCommitmentsDbContext>(() => Db),
                 _outerApiClient.Object,
                new CommitmentPaymentsWebJobConfiguration(),
                Mock.Of<IFilterOutAcademicYearRollOverDataLocks>());
        }

        [TearDown]
        public void CleanUp()
        {
            Db.Database.EnsureDeleted();
        }

        [Test]
        public async Task ThenTheLastDataLockEventIdAndAPageOfDatalockIsRetrieved()
        {
            // Arrange
            var dataLockStatusId = 2;
            var maxDataLockEventId = 2;
            string priceEpisode = "25-6-01/06/2016";

            SeedDataLock(SeedApprenticeships[0], dataLockStatusId, maxDataLockEventId, priceEpisode, DateTime.Now, DataLockErrorCode.Dlock03, SeedApprenticeshipUpdates[0]);
            SeedData(Db);

            _dataLockUpdater = CreateService();
            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            _outerApiClient
             .Verify(x => x.GetWithRetry<GetDataLockStatusListResponse>(It.Is<GetDataLockEventsRequest>(o => o.SinceEventId == maxDataLockEventId)), Times.Once);
        }

        [Test]
        public async Task ThenInsertDataLockStatusRecordsIfNoDataLockExist()
        {
            SeedData(Db);
            SeedDataLocks.Clear();

            long dataLockEventId2 = 2;
            long dataLockEventId3 = 3;
            long dataLockEventId4 = 4;

            SeedDataLock(SeedApprenticeships.FirstOrDefault(),
                dataLockEventId2, dataLockEventId2, "TEST-15/08/2018", DateTime.Now, DataLockErrorCode.Dlock07,
                SeedApprenticeshipUpdates.FirstOrDefault(), true);

            SeedDataLock(SeedApprenticeships.FirstOrDefault(),
               dataLockEventId3, dataLockEventId3, "TEST-15/08/2019", DateTime.Now, DataLockErrorCode.Dlock07,
               SeedApprenticeshipUpdates.FirstOrDefault(), true);

            SeedDataLock(SeedApprenticeships.FirstOrDefault(),
               dataLockEventId4, dataLockEventId4, "TEST-15/08/2020", DateTime.Now, DataLockErrorCode.Dlock03,
               SeedApprenticeshipUpdates.FirstOrDefault(), true);

            var apimResponse = new GetDataLockStatusListResponse
            {
                DataLockStatuses = SeedDataLocks
            };

            _outerApiClient
               .Setup(x => x.GetWithRetry<GetDataLockStatusListResponse>(It.IsAny<GetDataLockEventsRequest>()))
               .ReturnsAsync(apimResponse);

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            VerifyDataLockIsResolved(new List<long> { dataLockEventId2, dataLockEventId3, dataLockEventId4 });
        }

        [Test]
        public async Task ThenUpdateDataLockStatusRecordToResolved()
        {
            SeedDataLocks.Clear();

            long dataLockEventId2 = 2;
            long dataLockEventId3 = 3;
            long dataLockEventId4 = 4;

            var dataLockResolved = true;

            SeedDataLock(SeedApprenticeships.FirstOrDefault(),
                dataLockEventId2, dataLockEventId2, "TEST-15/08/2018", DateTime.Now, DataLockErrorCode.Dlock07,
                SeedApprenticeshipUpdates.FirstOrDefault(), dataLockResolved);

            SeedDataLock(SeedApprenticeships.FirstOrDefault(),
               dataLockEventId3, dataLockEventId3, "TEST-15/08/2019", DateTime.Now, DataLockErrorCode.Dlock07,
               SeedApprenticeshipUpdates.FirstOrDefault(), dataLockResolved);

            SeedDataLock(SeedApprenticeships.FirstOrDefault(),
               dataLockEventId4, dataLockEventId4, "TEST-15/08/2020", DateTime.Now, DataLockErrorCode.Dlock03,
               SeedApprenticeshipUpdates.FirstOrDefault(), dataLockResolved);

            var apimResponse = new GetDataLockStatusListResponse
            {
                DataLockStatuses = Clone(SeedDataLocks)
            };

            SeedDataLocks.ForEach(x => x.IsResolved = false);
            SeedData(Db);

            _outerApiClient
               .Setup(x => x.GetWithRetry<GetDataLockStatusListResponse>(It.IsAny<GetDataLockEventsRequest>()))
               .ReturnsAsync(apimResponse);

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            VerifyDataLockIsResolved(new List<long> { dataLockEventId2, dataLockEventId3, dataLockEventId4 });
        }

        [Test]
        public async Task AndDataLockPageContainsDataLockSuccessThenDataLockFlagHasBeenUpdatedForTheApprenticeship()
        {
            SeedDataLocks.Clear();
            const long hasHadDataLockSuccessApprenticeshipId = 2;
            const long hasNotHadDataLockSuccessApprenticeshipId = 3;

            //HasHadDataLockSuccess Apprenticeship 1
            SeedApprenticeship(hasHadDataLockSuccessApprenticeshipId, PaymentStatus.Active, false);
            SeedApprenticeshipUpdate(SeedApprenticeships[1].Id, PaymentStatus.Active, SeedApprenticeships[1]);
            SeedDataLock(SeedApprenticeships[1], 1, 1, "TEST-15/08/2018", new DateTime(2018, 8, 1), DataLockErrorCode.None, SeedApprenticeshipUpdates[1]);

            //HasHadDataLockSuccess Apprenticeship HasNotHadDataLockSuccess 2
            SeedApprenticeship(hasNotHadDataLockSuccessApprenticeshipId, PaymentStatus.Active, false);
            SeedApprenticeshipUpdate(SeedApprenticeships[2].Id, PaymentStatus.Active, SeedApprenticeships[2]);
            SeedDataLock(SeedApprenticeships[2], 2, 2, "TEST-15/08/2018", new DateTime(2018, 8, 1), DataLockErrorCode.None, SeedApprenticeshipUpdates[2]);

            var apimResponse = new GetDataLockStatusListResponse
            {
                DataLockStatuses = SeedDataLocks
            };

            SeedData(Db);

            _outerApiClient
             .Setup(x => x.GetWithRetry<GetDataLockStatusListResponse>(It.IsAny<GetDataLockEventsRequest>()))
             .ReturnsAsync(apimResponse);

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            var apprenticeshipHasNotHadDataLockSuccess = Db.Apprenticeships.FirstOrDefault(x => x.Id == hasNotHadDataLockSuccessApprenticeshipId);
            var apprenticeshipHasHadDataLockSuccess = Db.Apprenticeships.FirstOrDefault(x => x.Id == hasHadDataLockSuccessApprenticeshipId);

            Assert.IsTrue(apprenticeshipHasNotHadDataLockSuccess.HasHadDataLockSuccess);
            Assert.IsTrue(apprenticeshipHasHadDataLockSuccess.HasHadDataLockSuccess);
        }

        [TestCase(200, DataLockErrorCode.None, true)]
        [TestCase(10, DataLockErrorCode.Dlock01, false)]
        [TestCase(20, DataLockErrorCode.Dlock02, false)]
        [TestCase(30, DataLockErrorCode.Dlock03, true)]
        [TestCase(40, DataLockErrorCode.Dlock04, true)]
        [TestCase(50, DataLockErrorCode.Dlock05, true)]
        [TestCase(60, DataLockErrorCode.Dlock06, true)]
        [TestCase(70, DataLockErrorCode.Dlock07, true)]
        [TestCase(80, DataLockErrorCode.Dlock08, false)]
        [TestCase(90, DataLockErrorCode.Dlock09, false)]
        [TestCase(100, DataLockErrorCode.Dlock10, false)]
        public async Task ThenDataLocksAreSkippedIfNotOnTheWhitelist(long datalockEventId, DataLockErrorCode errorCode, bool expectUpdate)
        {
            //Arrange
            SeedData(Db);
            SeedDataLocks.Clear();

            SeedDataLock(SeedApprenticeships.FirstOrDefault(),
                datalockEventId, datalockEventId, "TEST-15/08/2018", DateTime.Now, errorCode,
                SeedApprenticeshipUpdates.FirstOrDefault());

            var apimResponse = new GetDataLockStatusListResponse
            {
                DataLockStatuses = SeedDataLocks
            };

            _outerApiClient
               .Setup(x => x.GetWithRetry<GetDataLockStatusListResponse>(It.IsAny<GetDataLockEventsRequest>()))
               .ReturnsAsync(apimResponse);

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            VerifyDataLockIsNotUpdated(datalockEventId, expectUpdate);
        }

        [TestCase(10, DataLockErrorCode.Dlock01 | DataLockErrorCode.Dlock03, DataLockErrorCode.Dlock03)]
        [TestCase(11, DataLockErrorCode.Dlock07 | DataLockErrorCode.Dlock10, DataLockErrorCode.Dlock07)]
        [TestCase(12, DataLockErrorCode.Dlock03 | DataLockErrorCode.Dlock04, DataLockErrorCode.Dlock03 | DataLockErrorCode.Dlock04)]
        public async Task ThenDataLocksWithMultipleErrorCodesAreFilteredUsingWhitelist(long datalockEventId, DataLockErrorCode errorCode, DataLockErrorCode expectSavedErrorCode)
        {
            //Arrange
            SeedData(Db);
            SeedDataLocks.Clear();

            SeedDataLock(SeedApprenticeships.FirstOrDefault(),
                datalockEventId, datalockEventId, "TEST-15/08/2018", DateTime.Now, errorCode,
                SeedApprenticeshipUpdates.FirstOrDefault());

            var apimResponse = new GetDataLockStatusListResponse
            {
                DataLockStatuses = SeedDataLocks
            };

            _outerApiClient
               .Setup(x => x.GetWithRetry<GetDataLockStatusListResponse>(It.IsAny<GetDataLockEventsRequest>()))
               .ReturnsAsync(apimResponse);

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            Assert.IsTrue(Db.DataLocks.Any(x => x.DataLockEventId == datalockEventId && x.ErrorCode == expectSavedErrorCode));
        }

        [TestCase(10, DataLockErrorCode.None, true)]
        [TestCase(30, DataLockErrorCode.Dlock03, false)]
        [TestCase(40, DataLockErrorCode.Dlock04, false)]
        [TestCase(50, DataLockErrorCode.Dlock05, false)]
        [TestCase(60, DataLockErrorCode.Dlock06, false)]
        [TestCase(70, DataLockErrorCode.Dlock07, false)]
        public async Task ThenPendingChangesAreExpiredOnSuccessfulDatalock(long objectId, DataLockErrorCode errorCode, bool expectExpiry)
        {
            //Arrange
            SeedDataLocks.Clear();
            SeedApprenticeships.Clear();
            SeedApprenticeshipUpdates.Clear();

            SeedApprenticeship(objectId, PaymentStatus.Active, pendingUpdateOriginator: Originator.Unknown);
            SeedApprenticeshipUpdate(objectId, PaymentStatus.Active, SeedApprenticeships.First(), ApprenticeshipUpdateStatus.Pending);
            SeedDataLock(SeedApprenticeships.First(), objectId, objectId, "TEST-15/08/2018", new DateTime(2018, 8, 1), errorCode, SeedApprenticeshipUpdates.First());
            SeedData(Db);

            var apimResponse = new GetDataLockStatusListResponse
            {
                DataLockStatuses = SeedDataLocks
            };

            _outerApiClient
               .Setup(x => x.GetWithRetry<GetDataLockStatusListResponse>(It.IsAny<GetDataLockEventsRequest>()))
               .ReturnsAsync(apimResponse);

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            var apprenticeship = Db.Apprenticeships.First(x => x.Id == objectId);
            Assert.AreEqual(apprenticeship.PendingUpdateOriginator == null, expectExpiry);

            var apprenticeshipUpdate = Db.ApprenticeshipUpdates.First(x => x.Id == objectId);
            Assert.AreEqual(apprenticeshipUpdate.Status == ApprenticeshipUpdateStatus.Expired, expectExpiry);
        }

        [TestCase(10, DataLockErrorCode.None)]
        [TestCase(30, DataLockErrorCode.Dlock03)]
        [TestCase(40, DataLockErrorCode.Dlock04)]
        [TestCase(50, DataLockErrorCode.Dlock05)]
        [TestCase(60, DataLockErrorCode.Dlock06)]
        [TestCase(70, DataLockErrorCode.Dlock07)]
        public async Task ThenPendingChangesWithoutCourseOrPriceAreNotExpiredOnSuccessfulDatalock(long objectId, DataLockErrorCode errorCode)
        {
            //Arrange
            SeedDataLocks.Clear();
            SeedApprenticeships.Clear();
            SeedApprenticeshipUpdates.Clear();

            SeedApprenticeship(objectId, PaymentStatus.Active, pendingUpdateOriginator: Originator.Unknown);

            SeedApprenticeshipUpdate(objectId, PaymentStatus.Active, SeedApprenticeships.First());

            SeedApprenticeshipUpdates[0].Cost = null;
            SeedApprenticeshipUpdates[0].TrainingCode = null;
            SeedApprenticeshipUpdates[0].Status = ApprenticeshipUpdateStatus.Pending;
            SeedApprenticeshipUpdates[0].FirstName = "ChangedFirstName";
            SeedApprenticeshipUpdates[0].LastName = "ChangedLastName";
            SeedApprenticeshipUpdates[0].DateOfBirth = new DateTime(1999, 1, 1);

            SeedDataLock(SeedApprenticeships.First(), objectId, objectId, "TEST-15/08/2018", DateTime.Now, errorCode, SeedApprenticeshipUpdates.First());

            SeedData(Db);

            var apimResponse = new GetDataLockStatusListResponse
            {
                DataLockStatuses = SeedDataLocks
            };

            _outerApiClient
               .Setup(x => x.GetWithRetry<GetDataLockStatusListResponse>(It.IsAny<GetDataLockEventsRequest>()))
               .ReturnsAsync(apimResponse);

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            var apprenticeship = Db.Apprenticeships.First(x => x.Id == objectId);
            Assert.IsTrue(apprenticeship.PendingUpdateOriginator == Originator.Unknown);

            var apprenticeshipUpdate = Db.ApprenticeshipUpdates.First(x => x.Id == objectId);
            Assert.IsTrue(apprenticeshipUpdate.Status == ApprenticeshipUpdateStatus.Pending);
        }

        [Test]
        public async Task ThenDatalocksForStoppedAndBackdatedApprenticeshipsAreAutoResolved()
        {
            //Arange
            SeedDataLocks.Clear();

            long dataLockEventId2 = 2;
            var dataLockResolved = false;

            SeedApprenticeships[0].PaymentStatus = PaymentStatus.Withdrawn;
            SeedApprenticeships[0].StartDate = DateTime.Today.AddMonths(-1);
            SeedApprenticeships[0].StopDate = DateTime.Today.AddMonths(-1);

            SeedDataLock(SeedApprenticeships[0],
                dataLockEventId2, dataLockEventId2, "TEST-15/08/2018",
                DateTime.Now,
                DataLockErrorCode.Dlock07,
                SeedApprenticeshipUpdates.FirstOrDefault(), dataLockResolved);

            var apimResponse = new GetDataLockStatusListResponse
            {
                DataLockStatuses = SeedDataLocks
            };

            SeedData(Db);

            _outerApiClient
               .Setup(x => x.GetWithRetry<GetDataLockStatusListResponse>(It.IsAny<GetDataLockEventsRequest>()))
               .ReturnsAsync(apimResponse);

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            VerifyDataLockIsResolved(new List<long> { dataLockEventId2 });
        }

        [Test]
        public async Task ThenPriceDatalocksInCombinationWithDlock09AreIgnored()
        {
            //Arrange
            SeedData(Db);

            SeedDataLocks.Clear();
            SeedApprenticeships.Clear();
            SeedApprenticeshipUpdates.Clear();

            SeedApprenticeship(2, PaymentStatus.Active);
            SeedApprenticeships[0].PaymentStatus = PaymentStatus.Withdrawn;
            SeedApprenticeships[0].StartDate = DateTime.Today.AddMonths(-1);
            SeedApprenticeships[0].StopDate = DateTime.Today.AddMonths(-1);

            SeedApprenticeshipUpdate(SeedApprenticeships.First().Id, PaymentStatus.Active, SeedApprenticeships.First());
            SeedData(Db);

            long dataLockEventId2 = 2;

            SeedDataLock(SeedApprenticeships.First(),
                dataLockEventId2, dataLockEventId2, "TEST-15/08/2018",
                 DateTime.Today.AddMonths(-2),
                DataLockErrorCode.Dlock07,
                SeedApprenticeshipUpdates.First());

            var apimResponse = new GetDataLockStatusListResponse
            {
                DataLockStatuses = SeedDataLocks
            };

            _outerApiClient
                .Setup(x => x.GetWithRetry<GetDataLockStatusListResponse>(It.IsAny<GetDataLockEventsRequest>()))
                .ReturnsAsync(apimResponse);

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            Assert.IsFalse(Db.DataLocks.Any(x => x.Id == dataLockEventId2));
        }

        [TestCase(10, "TEST-01/05/2017", false)]
        [TestCase(20, "TEST-31/07/2017", false)]
        [TestCase(30, "TEST-01/08/2017", true)]
        [TestCase(40, "TEST-01/08/2018", true)]
        public async Task ThenDataLocksAreSkippedIfTheyPertainToThe1617AcademicYear(long datalockEventId, string priceEpisodeIdentifier, bool expectUpdate)
        {
            //Arrange
            SeedData(Db);
            SeedDataLocks.Clear();

            SeedDataLock(SeedApprenticeships.FirstOrDefault(),
                datalockEventId, datalockEventId, priceEpisodeIdentifier, DateTime.Now, DataLockErrorCode.Dlock07,
                SeedApprenticeshipUpdates.FirstOrDefault());

            var apimResponse = new GetDataLockStatusListResponse
            {
                DataLockStatuses = SeedDataLocks
            };

            _outerApiClient
               .Setup(x => x.GetWithRetry<GetDataLockStatusListResponse>(It.IsAny<GetDataLockEventsRequest>()))
               .ReturnsAsync(apimResponse);

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            Assert.AreEqual(Db.DataLocks.Any(x => x.DataLockEventId == datalockEventId), expectUpdate);

            SeedDataLocks.Clear();
        }

        private void SeedData(ProviderCommitmentsDbContext dbContext)
        {
            Db.Database.EnsureCreated();
            dbContext.Apprenticeships.AddRange(SeedApprenticeships);
            dbContext.ApprenticeshipUpdates.AddRange(SeedApprenticeshipUpdates);
            dbContext.DataLocks.AddRange(SeedDataLocks);
            dbContext.SaveChanges(true);
        }

        public void SeedApprenticeship(long apprenticeshipId,
            PaymentStatus paymentStatus,
            bool hasHadDataLockSuccess = false,
            Originator? pendingUpdateOriginator = Originator.Unknown)
        {
            var accountLegalEntity =
                SeedApprenticeships.FirstOrDefault(p => p.Cohort.AccountLegalEntity.Id == 1)?.Cohort?.AccountLegalEntity
                    ?? new AccountLegalEntity()
                        .Set(a => a.LegalEntityId, LegalEntityIdentifier)
                        .Set(a => a.OrganisationType, OrganisationType.CompaniesHouse)
                        .Set(a => a.AccountId, apprenticeshipId)
                        .Set(a => a.Id, apprenticeshipId);

            var cohort =
                SeedApprenticeships.FirstOrDefault(p => p.Cohort.Id == 1)?.Cohort
                    ?? new Cohort()
                        .Set(c => c.Id, apprenticeshipId)
                        .Set(c => c.EmployerAccountId, apprenticeshipId)
                        .Set(c => c.AccountLegalEntity, accountLegalEntity)
                        .Set(c => c.AccountLegalEntityId, apprenticeshipId);

            var apprenticeship = new Apprenticeship()
                .Set(s => s.Id, apprenticeshipId)
                .Set(s => s.CommitmentId, cohort.Id)
                .Set(s => s.Cohort, cohort)
                .Set(s => s.PaymentStatus, paymentStatus)
                .Set(s => s.HasHadDataLockSuccess, hasHadDataLockSuccess)
                .Set(s => s.PendingUpdateOriginator, pendingUpdateOriginator);

            SeedApprenticeships.Add(apprenticeship);
        }

        public void SeedApprenticeshipUpdate(
            long apprenticeshipId,
            PaymentStatus paymentStatus,
            ApprenticeshipBase apprenticeship,
            ApprenticeshipUpdateStatus apprenticeshipUpdateStatus = ApprenticeshipUpdateStatus.Approved,
            decimal? cost = 100m)
        {
            var apprenticeshipUpdate = _fixture.Build<ApprenticeshipUpdate>()
               .Without(c => c.Apprenticeship)
               .Without(c => c.DataLockStatus)
               .Create();

            apprenticeshipUpdate.Id = apprenticeshipId;
            apprenticeshipUpdate.Apprenticeship = apprenticeship;
            apprenticeshipUpdate.ApprenticeshipId = apprenticeship.Id;
            apprenticeshipUpdate.TrainingCode = "UpdatedTrainingCode";
            apprenticeshipUpdate.Status = apprenticeshipUpdateStatus;
            apprenticeshipUpdate.Cost = cost;

            SeedApprenticeshipUpdates.Add(apprenticeshipUpdate);
        }

        public void SeedDataLock(
            Apprenticeship apprenticeship,
            long dataLockStatusId,
             long dataLockEventId,
            string priceEpisodeIdentifier,
            DateTime ilrEffectiveFromDate,
            DataLockErrorCode errorCode,
            ApprenticeshipUpdate apprenticeshipUpdate,
            bool resolve = false)
        {
            var dataLock = _fixture.Build<DataLockStatus>()
               .Without(c => c.Apprenticeship)
               .Without(c => c.ApprenticeshipUpdate)
               .Create();

            dataLock.Id = dataLockStatusId;
            dataLock.Apprenticeship = apprenticeship;
            dataLock.ApprenticeshipId = apprenticeship.Id;
            dataLock.ErrorCode = errorCode;
            dataLock.DataLockEventId = dataLockEventId;
            dataLock.IsResolved = resolve;
            dataLock.PriceEpisodeIdentifier = priceEpisodeIdentifier;
            dataLock.IlrEffectiveFromDate = ilrEffectiveFromDate;
            SeedDataLocks.Add(dataLock);
        }

        private DataLockUpdaterService CreateService()
        {
            return new DataLockUpdaterService(
               Mock.Of<ILogger<DataLockUpdaterService>>(),
               new Lazy<ProviderCommitmentsDbContext>(() => Db),
                _outerApiClient.Object,
               new CommitmentPaymentsWebJobConfiguration(),
               Mock.Of<IFilterOutAcademicYearRollOverDataLocks>());
        }

        private T Clone<T>(T objectToClone)
        {
            var stringValue = JsonConvert.SerializeObject(objectToClone);
            return JsonConvert.DeserializeObject<T>(stringValue);
        }

        private void VerifyDataLockIsResolved(List<long> dataLockEventIds)
        {
            var dataLocks = Db.DataLocks
              .Where(x => dataLockEventIds.Contains(x.DataLockEventId))
              .ToList();

            Assert.AreEqual(dataLocks.Count, dataLockEventIds.Count);

            Assert.True(dataLocks.All(x => x.IsResolved));
        }

        private void VerifyDataLockIsNotUpdated(long dataLockEventId, bool expectDataLock)
        {
            Assert.AreEqual(expectDataLock, Db.DataLocks.Any(x => dataLockEventId == x.DataLockEventId));
        }
    }
}
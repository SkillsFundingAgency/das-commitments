using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class DataLockUpdaterServiceTests
    {
        private ProviderCommitmentsDbContext Db { get; set; }
        private Mock<IApprovalsOuterApiClient> _outerApiClient;
        private CommitmentPaymentsWebJobConfiguration _config;
        private Mock<IFilterOutAcademicYearRollOverDataLocks> _filterOutAcademicYearRollOverDataLocks;

        public void Arrange()
        {
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                 .UseInMemoryDatabase(Guid.NewGuid().ToString(), new InMemoryDatabaseRoot())
                 .EnableSensitiveDataLogging()
                 .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning))
                 .Options);

            _outerApiClient = new Mock<IApprovalsOuterApiClient>();

            _outerApiClient.Setup(x => x.Get(
                It.IsAny<long>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<int>()))
                .ReturnsAsync(new List<DataLockStatus>());

            _outerApiClient.Setup(x => x.Get<StandardResponse>(It.IsAny<GetStandardsRequest>())).ReturnsAsync(apiResponse);

            _apprenticeshipUpdateRepository
                .Setup(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync((ApprenticeshipUpdate)null);

            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(new Apprenticeship
                {
                    PaymentStatus = PaymentStatus.Active
                });

            _dataLockUpdater = new DataLockUpdater(
                Mock.Of<ILogger<DataLockUpdaterService>>(),
                _paymentEvents.Object,
                _dataLockRepository.Object,
                _apprenticeshipUpdateRepository.Object,
                _config,
                Mock.Of<IFilterOutAcademicYearRollOverDataLocks>(),
                _apprenticeshipRepository.Object
                );
        }

        [Test]
        public async Task ThenTheLastDataLockEventIdIsRetrievedFromTheRepository()
        {
            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            _dataLockRepository.Verify(x => x.GetLastDataLockEventId(), Times.Once);
        }

        [Test]
        public async Task ThenAPageOfDataIsRetrievedFromThePaymentEventsService()
        {
            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            _paymentEvents.Verify(x => x.GetDataLockEvents(It.IsAny<long>(), null, null, 0L, 1), Times.Once);
        }

        [Test]
        public async Task ThenDataIsRetrievedFromThePaymentEventsServiceUntilNoMoreDataIsAvailable()
        {
            var page1 = new List<DataLockStatus>
            {
                new DataLockStatus
                {
                    DataLockEventId = 2,
                    PriceEpisodeIdentifier = "TEST-15/08/2018"
                },
                new DataLockStatus
                {
                    DataLockEventId = 3,
                    PriceEpisodeIdentifier = "TEST-15/08/2018"
                },
                new DataLockStatus
                {
                    DataLockEventId = 4,
                    PriceEpisodeIdentifier = "TEST-15/08/2018"
                }
            };

            _paymentEvents.Setup(x => x.GetDataLockEvents(1, null, null, 0L, 1)).ReturnsAsync(page1);
            _paymentEvents.Setup(x => x.GetDataLockEvents(4, null, null, 0L, 1)).ReturnsAsync(new List<DataLockStatus>());

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            _paymentEvents.Verify(x => x.GetDataLockEvents(1, null, null, 0L, 1), Times.Once);
            _paymentEvents.Verify(x => x.GetDataLockEvents(4, null, null, 0L, 1), Times.Once);
            _paymentEvents.Verify(x => x.GetDataLockEvents(It.IsAny<long>(), null, null, 0L, 1), Times.Exactly(2));
        }

        [Test]
        public async Task ThenTheRepositoryIsCalledToUpdateTheDataLockStatus()
        {
            var page1 = new List<DataLockStatus>
            {
                new DataLockStatus
                {
                    DataLockEventId = 2,
                    ErrorCode = DataLockErrorCode.Dlock07,
                    PriceEpisodeIdentifier = "TEST-15/08/2018"
                },
                new DataLockStatus
                {
                    DataLockEventId = 3,
                    ErrorCode = DataLockErrorCode.Dlock07,
                    PriceEpisodeIdentifier = "TEST-15/08/2018"
                },
                new DataLockStatus
                {
                    DataLockEventId = 4,
                    ErrorCode = DataLockErrorCode.Dlock07,
                    PriceEpisodeIdentifier = "TEST-15/08/2018"
                }
            };

            _paymentEvents.Setup(x => x.GetDataLockEvents(1, null, null, 0L, 1)).ReturnsAsync(page1);
            _paymentEvents.Setup(x => x.GetDataLockEvents(4, null, null, 0L, 1)).ReturnsAsync(new List<DataLockStatus>());

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            _dataLockRepository.Verify(x => x.UpdateDataLockStatus(It.Is<DataLockStatus>(d => !d.IsResolved)), Times.Exactly(3));
        }

        [Test]
        public async Task AndPageContainsDataLockSuccessThenDataLockFlagHasBeenUpdatedForTheApprenticeship()
        {
            const long hasHadDataLockSuccessApprenticeshipId = 1;
            const long hasNotHadDataLockSuccessApprenticeshipId = 2;
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    Id = hasHadDataLockSuccessApprenticeshipId,
                    HasHadDataLockSuccess = true
                },
                new Apprenticeship
                {
                    Id = hasNotHadDataLockSuccessApprenticeshipId,
                    HasHadDataLockSuccess = false
                }
            };

            var page1 = new List<DataLockStatus>
            {
                new DataLockStatus
                {
                    ApprenticeshipId = hasHadDataLockSuccessApprenticeshipId,
                    DataLockEventId = 2,
                    ErrorCode = DataLockErrorCode.None,
                    PriceEpisodeIdentifier = "TEST-15/08/2018"
                },
                new DataLockStatus
                {
                    ApprenticeshipId = hasNotHadDataLockSuccessApprenticeshipId,
                    DataLockEventId = 3,
                    ErrorCode = DataLockErrorCode.None,
                    PriceEpisodeIdentifier = "TEST-15/08/2018"
                }
            };

            _paymentEvents.Setup(x => x.GetDataLockEvents(1, null, null, 0L, 1)).ReturnsAsync(page1);
            _paymentEvents.Setup(x => x.GetDataLockEvents(4, null, null, 0L, 1)).ReturnsAsync(new List<DataLockStatus>());

            _apprenticeshipRepository.Setup(x => x.SetHasHadDataLockSuccess(It.IsAny<int>())).Returns(Task.FromResult(0));
            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(hasHadDataLockSuccessApprenticeshipId)).ReturnsAsync(apprenticeships.First(y => y.Id == hasHadDataLockSuccessApprenticeshipId));
            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(hasNotHadDataLockSuccessApprenticeshipId)).ReturnsAsync(apprenticeships.First(y => y.Id == hasNotHadDataLockSuccessApprenticeshipId));

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            _apprenticeshipRepository.Verify(x => x.SetHasHadDataLockSuccess(hasNotHadDataLockSuccessApprenticeshipId), Times.Once);
            _apprenticeshipRepository.Verify(x => x.SetHasHadDataLockSuccess(hasHadDataLockSuccessApprenticeshipId), Times.Once);
        }

        [TestCase(DataLockErrorCode.None, true)]
        [TestCase(DataLockErrorCode.Dlock01, false)]
        [TestCase(DataLockErrorCode.Dlock02, false)]
        [TestCase(DataLockErrorCode.Dlock03, true)]
        [TestCase(DataLockErrorCode.Dlock04, true)]
        [TestCase(DataLockErrorCode.Dlock05, true)]
        [TestCase(DataLockErrorCode.Dlock06, true)]
        [TestCase(DataLockErrorCode.Dlock07, true)]
        [TestCase(DataLockErrorCode.Dlock08, false)]
        [TestCase(DataLockErrorCode.Dlock09, false)]
        [TestCase(DataLockErrorCode.Dlock10, false)]
        public async Task ThenDataLocksAreSkippedIfNotOnTheWhitelist(DataLockErrorCode errorCode, bool expectUpdate)
        {
            //Arrange
            var page1 = new List<DataLockStatus>
            {
                new DataLockStatus
                {
                    DataLockEventId = 2,
                    ErrorCode = errorCode,
                    PriceEpisodeIdentifier = "TEST-15/08/2018"
                }
            };

            _paymentEvents.Setup(x => x.GetDataLockEvents(1, null, null, 0L, 1)).ReturnsAsync(page1);

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            var expectedCalls = expectUpdate ? 1 : 0;
            _dataLockRepository.Verify(x => x.UpdateDataLockStatus(It.IsAny<DataLockStatus>()), Times.Exactly(expectedCalls));
        }

        [TestCase(DataLockErrorCode.Dlock01 | DataLockErrorCode.Dlock03, DataLockErrorCode.Dlock03)]
        [TestCase(DataLockErrorCode.Dlock07 | DataLockErrorCode.Dlock10, DataLockErrorCode.Dlock07)]
        [TestCase(DataLockErrorCode.Dlock03 | DataLockErrorCode.Dlock04, DataLockErrorCode.Dlock03 | DataLockErrorCode.Dlock04)]
        public async Task ThenDataLocksWithMultipleErrorCodesAreFilteredUsingWhitelist(DataLockErrorCode errorCode,
            DataLockErrorCode expectSavedErrorCode)
        {
            //Arrange
            var page1 = new List<DataLockStatus>
            {
                new DataLockStatus
                {
                    DataLockEventId = 2,
                    ErrorCode = errorCode,
                    PriceEpisodeIdentifier = "TEST-15/08/2018"
                }
            };

            _paymentEvents.Setup(x => x.GetDataLockEvents(1, null, null, 0L, 1)).ReturnsAsync(page1);

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            _dataLockRepository.Verify(x => x.UpdateDataLockStatus(It.Is<DataLockStatus>(d => d.ErrorCode == expectSavedErrorCode)), Times.Once);
        }

        [Test]
        public void ThenThrowsExceptionIfDbConstraintErrorOccurs()
        {
            var page1 = new List<DataLockStatus>
            {
                new DataLockStatus
                {
                    ApprenticeshipId = 1,
                    DataLockEventId = 2,
                    ErrorCode = DataLockErrorCode.Dlock07,
                    PriceEpisodeIdentifier = "TEST-15/08/2018"
                }
            };

            _paymentEvents.Setup(x => x.GetDataLockEvents(1, null, null, 0L, 1)).ReturnsAsync(page1);
            _dataLockRepository.Setup(x => x.UpdateDataLockStatus(It.IsAny<DataLockStatus>())).Throws(new RepositoryConstraintException());

            Assert.ThrowsAsync<RepositoryConstraintException>(async () => await _dataLockUpdater.RunUpdate());
        }

        [Test]
        public void ThenIgnoresDbConstraintErrorIfConfigSettingSet()
        {
            var page1 = new List<DataLockStatus>
            {
                new DataLockStatus
                {
                    ApprenticeshipId = 1,
                    DataLockEventId = 2,
                    ErrorCode = DataLockErrorCode.Dlock07,
                    PriceEpisodeIdentifier = "TEST-15/08/2018"
                }
            };

            _paymentEvents.Setup(x => x.GetDataLockEvents(1, null, null, 0L, 1)).ReturnsAsync(page1);
            _dataLockRepository.Setup(x => x.UpdateDataLockStatus(It.IsAny<DataLockStatus>())).Throws(new RepositoryConstraintException());

            _config.IgnoreDataLockStatusConstraintErrors = true;

            Assert.DoesNotThrowAsync(async () => await _dataLockUpdater.RunUpdate());
        }

        [TestCase(DataLockErrorCode.None, true)]
        [TestCase(DataLockErrorCode.Dlock03, false)]
        [TestCase(DataLockErrorCode.Dlock04, false)]
        [TestCase(DataLockErrorCode.Dlock05, false)]
        [TestCase(DataLockErrorCode.Dlock06, false)]
        [TestCase(DataLockErrorCode.Dlock07, false)]
        public async Task ThenPendingChangesAreExpiredOnSuccessfulDatalock(
            DataLockErrorCode errorCode, bool expectExpiry)
        {
            var page1 = new List<DataLockStatus>
            {
                new DataLockStatus
                {
                    ApprenticeshipId = 1,
                    DataLockEventId = 2,
                    ErrorCode = errorCode,
                    PriceEpisodeIdentifier = "TEST-15/08/2018"
                }
            };

            _paymentEvents.Setup(x => x.GetDataLockEvents(1, null, null, 0L, 1)).ReturnsAsync(page1);

            _apprenticeshipUpdateRepository.Setup(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(new ApprenticeshipUpdate
                {
                    Id = 3,
                    Cost = 100,
                    TrainingCode = "UpdatedTrainingCode"
                });

            _apprenticeshipUpdateRepository.Setup(x => x.ExpireApprenticeshipUpdate(It.IsAny<long>()))
                .Returns(() => Task.FromResult(0L));

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            _apprenticeshipUpdateRepository.Verify(
                x => x.GetPendingApprenticeshipUpdate(It.Is<long>(appId => appId == 1)),
                expectExpiry ? Times.Once() : Times.Never());

            _apprenticeshipUpdateRepository.Verify(
                x => x.ExpireApprenticeshipUpdate(It.Is<long>(updateId => updateId == 3)),
                 expectExpiry ? Times.Once() : Times.Never());
        }

        [TestCase(DataLockErrorCode.None)]
        [TestCase(DataLockErrorCode.Dlock03)]
        [TestCase(DataLockErrorCode.Dlock04)]
        [TestCase(DataLockErrorCode.Dlock05)]
        [TestCase(DataLockErrorCode.Dlock06)]
        [TestCase(DataLockErrorCode.Dlock07)]
        public async Task ThenPendingChangesWithoutCourseOrPriceAreNotExpiredOnSuccessfulDatalock(
            DataLockErrorCode errorCode)
        {
            var page1 = new List<DataLockStatus>
            {
                new DataLockStatus
                {
                    ApprenticeshipId = 1,
                    DataLockEventId = 2,
                    ErrorCode = errorCode,
                    PriceEpisodeIdentifier = "TEST-15/08/2018"
                }
            };

            _paymentEvents.Setup(x => x.GetDataLockEvents(1, null, null, 0L, 1)).ReturnsAsync(page1);

            _apprenticeshipUpdateRepository.Setup(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(new ApprenticeshipUpdate
                {
                    Id = 3,
                    FirstName = "ChangedFirstName",
                    LastName = "ChangedLastName",
                    DateOfBirth = new DateTime(1999, 1, 1)
                });

            _apprenticeshipUpdateRepository.Setup(x => x.ExpireApprenticeshipUpdate(It.IsAny<long>()))
                .Returns(() => Task.FromResult(0L));

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            _apprenticeshipUpdateRepository.Verify(
                x => x.ExpireApprenticeshipUpdate(It.IsAny<long>()), Times.Never());
        }

        [Test]
        public async Task ThenDatalocksForStoppedAndBackdatedApprenticeshipsAreAutoResolved()
        {
            var page1 = new List<DataLockStatus>
            {
                new DataLockStatus
                {
                    ApprenticeshipId = 1,
                    DataLockEventId = 2,
                    ErrorCode = DataLockErrorCode.Dlock07,
                    PriceEpisodeIdentifier = "TEST-15/08/2018"
                }
            };

            _paymentEvents.Setup(x => x.GetDataLockEvents(1, null, null, 0L, 1)).ReturnsAsync(page1);

            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(new Apprenticeship
                {
                    PaymentStatus = PaymentStatus.Withdrawn,
                    StartDate = DateTime.Today.AddMonths(-1),
                    StopDate = DateTime.Today.AddMonths(-1)
                });

            await _dataLockUpdater.RunUpdate();

            _dataLockRepository.Verify(x => x.UpdateDataLockStatus(It.Is<DataLockStatus>(d => d.IsResolved)), Times.Once);
        }

        [Test]
        public async Task ThenPriceDatalocksInCombinationWithDlock09AreIgnored()
        {
            var page1 = new List<DataLockStatus>
            {
                new DataLockStatus
                {
                    ApprenticeshipId = 1,
                    DataLockEventId = 2,
                    ErrorCode = DataLockErrorCode.Dlock07,
                    IlrEffectiveFromDate = DateTime.Today.AddMonths(-2),
                    PriceEpisodeIdentifier = "TEST-15/08/2018"
                }
            };

            _paymentEvents.Setup(x => x.GetDataLockEvents(1, null, null, 0L, 1)).ReturnsAsync(page1);

            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(new Apprenticeship
                {
                    PaymentStatus = PaymentStatus.Withdrawn,
                    StartDate = DateTime.Today.AddMonths(-1),
                    StopDate = DateTime.Today.AddMonths(-1)
                });

            await _dataLockUpdater.RunUpdate();

            _dataLockRepository.Verify(x => x.UpdateDataLockStatus(It.IsAny<DataLockStatus>()), Times.Never);
        }

        [TestCase("TEST-01/05/2017", false)]
        [TestCase("TEST-31/07/2017", false)]
        [TestCase("TEST-01/08/2017", true)]
        [TestCase("TEST-01/08/2018", true)]
        public async Task ThenDataLocksAreSkippedIfTheyPertainToThe1617AcademicYear(string priceEpisodeIdentifier, bool expectUpdate)
        {
            //Arrange
            var page1 = new List<DataLockStatus>
            {
                new DataLockStatus
                {
                    DataLockEventId = 2,
                    ErrorCode = DataLockErrorCode.Dlock07,
                    PriceEpisodeIdentifier = priceEpisodeIdentifier
                }
            };

            _paymentEvents.Setup(x => x.GetDataLockEvents(1, null, null, 0L, 1)).ReturnsAsync(page1);

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            var expectedCalls = expectUpdate ? 1 : 0;
            _dataLockRepository.Verify(x => x.UpdateDataLockStatus(It.IsAny<DataLockStatus>()), Times.Exactly(expectedCalls));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentPayments.WebJob.Configuration;
using SFA.DAS.CommitmentPayments.WebJob.Updater;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Commitments.Domain.Exceptions;

namespace SFA.DAS.CommitmentPayments.WebJob.UnitTests.Updater
{
    [TestFixture]
    public class WhenRunningUpdate
    {
        private IDataLockUpdater _dataLockUpdater;
        private Mock<IPaymentEvents> _paymentEvents;
        private Mock<IDataLockRepository> _dataLockRepository;
        private Mock<IApprenticeshipUpdateRepository> _apprenticeshipUpdateRepository;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private CommitmentPaymentsConfiguration _config;

        [SetUp]
        public void Arrange()
        {
            _dataLockRepository = new Mock<IDataLockRepository>();
            _paymentEvents = new Mock<IPaymentEvents>();
            _apprenticeshipUpdateRepository = new Mock<IApprenticeshipUpdateRepository>();
            _config = new CommitmentPaymentsConfiguration();
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _dataLockRepository.Setup(x => x.GetLastDataLockEventId()).ReturnsAsync(1L);
            _dataLockRepository.Setup(x => x.UpdateDataLockStatus(It.IsAny<DataLockStatus>())).ReturnsAsync(0L);
            
            _paymentEvents.Setup(x => x.GetDataLockEvents(
                It.IsAny<long>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<int>()))
                .ReturnsAsync(new List<DataLockStatus>());


            _apprenticeshipUpdateRepository.Setup(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(null);

            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(new Apprenticeship());


            _dataLockUpdater = new DataLockUpdater(
                Mock.Of<ILog>(), 
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
                    DataLockEventId = 2
                },
                new DataLockStatus
                {
                    DataLockEventId = 3
                },
                new DataLockStatus
                {
                    DataLockEventId = 4
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
                    ErrorCode = DataLockErrorCode.Dlock07
                },
                new DataLockStatus
                {
                    DataLockEventId = 3,
                    ErrorCode = DataLockErrorCode.Dlock07
                },
                new DataLockStatus
                {
                    DataLockEventId = 4,
                    ErrorCode = DataLockErrorCode.Dlock07
                }
            };

            _paymentEvents.Setup(x => x.GetDataLockEvents(1, null, null, 0L, 1)).ReturnsAsync(page1);
            _paymentEvents.Setup(x => x.GetDataLockEvents(4, null, null, 0L, 1)).ReturnsAsync(new List<DataLockStatus>());

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            _dataLockRepository.Verify(x => x.UpdateDataLockStatus(It.IsAny<DataLockStatus>()), Times.Exactly(3));
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
                    ErrorCode = DataLockErrorCode.None
                },
                new DataLockStatus
                {
                    ApprenticeshipId = hasNotHadDataLockSuccessApprenticeshipId,
                    DataLockEventId = 3,
                    ErrorCode = DataLockErrorCode.None
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
            _apprenticeshipRepository.Verify(x => x.GetApprenticeship(It.IsAny<long>()), Times.Never);
        }

        [Test]
        public async Task ThenGetApprentishipIsNotCalledUnnecessarily()
        {
            var page1 = new List<DataLockStatus>
            {
                new DataLockStatus
                {
                    DataLockEventId = 2,
                    ErrorCode = DataLockErrorCode.Dlock07
                },
                new DataLockStatus
                {
                    DataLockEventId = 3,
                    ErrorCode = DataLockErrorCode.Dlock07
                },
                new DataLockStatus
                {
                    DataLockEventId = 4,
                    ErrorCode = DataLockErrorCode.Dlock07
                }
            };

            _paymentEvents.Setup(x => x.GetDataLockEvents(1, null, null, 0L, 1)).ReturnsAsync(page1);
            _paymentEvents.Setup(x => x.GetDataLockEvents(4, null, null, 0L, 1)).ReturnsAsync(new List<DataLockStatus>());

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            _apprenticeshipRepository.Verify(x => x.GetApprenticeship(It.IsAny<long>()), Times.Never);
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
                    ErrorCode = errorCode
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
                    ErrorCode = errorCode
                }
            };

            _paymentEvents.Setup(x => x.GetDataLockEvents(1, null, null, 0L, 1)).ReturnsAsync(page1);

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            _dataLockRepository.Verify(x =>x.UpdateDataLockStatus(It.Is<DataLockStatus>(d =>d.ErrorCode == expectSavedErrorCode)), Times.Once);
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
                    ErrorCode = DataLockErrorCode.Dlock07
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
                    ErrorCode = DataLockErrorCode.Dlock07
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
                    ErrorCode = errorCode
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
                    ErrorCode = errorCode
                }
            };
            
            _paymentEvents.Setup(x => x.GetDataLockEvents(1, null, null, 0L, 1)).ReturnsAsync(page1);

            _apprenticeshipUpdateRepository.Setup(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(new ApprenticeshipUpdate
                {
                    Id = 3,
                    FirstName = "ChangedFirstName",
                    LastName = "ChangedLastName",
                    DateOfBirth = new DateTime(1999, 1, 1 )
                });

            _apprenticeshipUpdateRepository.Setup(x => x.ExpireApprenticeshipUpdate(It.IsAny<long>()))
                .Returns(() => Task.FromResult(0L));

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            _apprenticeshipUpdateRepository.Verify(
                x => x.ExpireApprenticeshipUpdate(It.IsAny<long>()), Times.Never());
        }
    }
}

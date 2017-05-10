using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentPayments.WebJob.Updater;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.CommitmentPayments.WebJob.UnitTests.Updater
{
    [TestFixture]
    public class WhenRunningUpdate
    {
        private IDataLockUpdater _dataLockUpdater;
        private Mock<IPaymentEvents> _paymentEvents;
        private Mock<IDataLockRepository> _dataLockRepository;
        private Mock<IApprenticeshipUpdateRepository> _apprenticeshipUpdateRepository;

        [SetUp]
        public void Arrange()
        {
            _dataLockRepository = new Mock<IDataLockRepository>();
            _dataLockRepository.Setup(x => x.GetLastDataLockEventId()).ReturnsAsync(1L);
            _dataLockRepository.Setup(x => x.UpdateDataLockStatus(It.IsAny<DataLockStatus>())).ReturnsAsync(0L);

            _paymentEvents = new Mock<IPaymentEvents>();
            _paymentEvents.Setup(x => x.GetDataLockEvents(
                It.IsAny<long>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<int>()))
                .ReturnsAsync(new List<DataLockStatus>());

            _apprenticeshipUpdateRepository = new Mock<IApprenticeshipUpdateRepository>();
            _apprenticeshipUpdateRepository.Setup(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(null);

            _apprenticeshipUpdateRepository.Setup(x => x.SupercedeApprenticeshipUpdate(It.IsAny<long>()))
                .Returns(()=> Task.FromResult(0L));

            _dataLockUpdater = new DataLockUpdater(Mock.Of<ILog>(), _paymentEvents.Object, _dataLockRepository.Object, _apprenticeshipUpdateRepository.Object);
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

            var page2 = new List<DataLockStatus>();

            _paymentEvents = new Mock<IPaymentEvents>();
            _paymentEvents.Setup(x => x.GetDataLockEvents(1, null, null, 0L, 1)).ReturnsAsync(page1);
            _paymentEvents.Setup(x => x.GetDataLockEvents(4, null, null, 0L, 1)).ReturnsAsync(page2);

            _dataLockUpdater = new DataLockUpdater(Mock.Of<ILog>(), _paymentEvents.Object, _dataLockRepository.Object, _apprenticeshipUpdateRepository.Object);

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

            var page2 = new List<DataLockStatus>();

            _paymentEvents = new Mock<IPaymentEvents>();
            _paymentEvents.Setup(x => x.GetDataLockEvents(1, null, null, 0L, 1)).ReturnsAsync(page1);
            _paymentEvents.Setup(x => x.GetDataLockEvents(4, null, null, 0L, 1)).ReturnsAsync(page2);

            _dataLockUpdater = new DataLockUpdater(Mock.Of<ILog>(), _paymentEvents.Object, _dataLockRepository.Object, _apprenticeshipUpdateRepository.Object);

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            _dataLockRepository.Verify(x => x.UpdateDataLockStatus(It.IsAny<DataLockStatus>()), Times.Exactly(3));
        }

        [TestCase(UpdateOrigin.DataLock, true)]
        [TestCase(UpdateOrigin.ChangeOfCircumstances, false)]
        public async Task ThenAnyPendingApprenticeshipUpdateIsMarkedAsObsoleteWhenUpdatingDataLockStatus(UpdateOrigin updateOrigin, bool expectSupercede)
        {
            //Arrange
            var page1 = new List<DataLockStatus>
            {
                new DataLockStatus
                {
                    DataLockEventId = 2
                }
            };

            _paymentEvents = new Mock<IPaymentEvents>();
            _paymentEvents.Setup(x => x.GetDataLockEvents(1, null, null, 0L, 1)).ReturnsAsync(page1);

            _apprenticeshipUpdateRepository.Setup(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(new ApprenticeshipUpdate
                {
                    UpdateOrigin = updateOrigin
                });

            _dataLockUpdater = new DataLockUpdater(Mock.Of<ILog>(), _paymentEvents.Object, _dataLockRepository.Object, _apprenticeshipUpdateRepository.Object);

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            var expectedCalls = expectSupercede ? Times.Once() : Times.Never();
            _apprenticeshipUpdateRepository.Verify(x => x.SupercedeApprenticeshipUpdate(It.IsAny<long>()), expectedCalls);
        }
    }
}

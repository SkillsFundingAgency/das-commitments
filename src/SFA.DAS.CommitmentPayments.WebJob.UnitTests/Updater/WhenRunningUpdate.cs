using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentPayments.WebJob.Updater;
using SFA.DAS.Commitments.Domain.Data;
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

            _dataLockUpdater = new DataLockUpdater(Mock.Of<ILog>(), _paymentEvents.Object, _dataLockRepository.Object);
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

            _dataLockUpdater = new DataLockUpdater(Mock.Of<ILog>(), _paymentEvents.Object, _dataLockRepository.Object);

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

            _dataLockUpdater = new DataLockUpdater(Mock.Of<ILog>(), _paymentEvents.Object, _dataLockRepository.Object);

            //Act
            await _dataLockUpdater.RunUpdate();

            //Assert
            _dataLockRepository.Verify(x => x.UpdateDataLockStatus(It.IsAny<DataLockStatus>()), Times.Exactly(3));
        }
    }
}

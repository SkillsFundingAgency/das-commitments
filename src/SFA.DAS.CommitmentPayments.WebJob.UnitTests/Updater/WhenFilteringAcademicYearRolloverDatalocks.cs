using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentPayments.WebJob.Updater;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.NLog.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentPayments.WebJob.UnitTests.Updater
{
    [TestFixture]
    public sealed class WhenFilteringAcademicYearRolloverDatalocks
    {
        private Mock<IDataLockRepository> _mockDataLockRepository;
        private Mock<ILog> _mockLogger;
        private FilterOutAcademicYearRollOverDataLocks _filter;

        [SetUp]
        public void Setup()
        {
            _mockDataLockRepository = new Mock<IDataLockRepository>();
            _mockLogger = new Mock<ILog>();
            _filter = new FilterOutAcademicYearRollOverDataLocks(_mockDataLockRepository.Object, _mockLogger.Object);
        }

        [Test(Description = "No datalocks for apprenticeship so nothing to do")]
        public async Task WhenNoDataLocks()
        {
            List<DataLockStatus> apprenticeshipDataLocks = new List<DataLockStatus>();

            await _filter.Filter(123);

            _mockDataLockRepository.Verify(x => x.Delete(It.Is<long>(a => a == 4)), Times.Never);
        }

        [Test(Description = "When has data locks but there are none with the same effective date then do nothing")]
        public async Task WhenNoDuplicateDatalocksForEffectiveDate()
        {
            List<DataLockStatus> apprenticeshipDataLocks = new List<DataLockStatus>
            {
                new DataLockStatus { DataLockEventId = 1, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/05/2017", IlrEffectiveFromDate = new DateTime(2017, 5, 1), Status = Status.Pass },
                new DataLockStatus { DataLockEventId = 2, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/06/2017", IlrEffectiveFromDate = new DateTime(2017, 6, 1), Status = Status.Pass },
                new DataLockStatus { DataLockEventId = 3, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/07/2017", IlrEffectiveFromDate = new DateTime(2017, 7, 1), Status = Status.Pass },
                new DataLockStatus { DataLockEventId = 4, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/08/2017", IlrEffectiveFromDate = new DateTime(2017, 8, 1), Status = Status.Pass }
            };

            _mockDataLockRepository.Setup(x => x.GetDataLocks(It.Is<long>(a => a == 123))).ReturnsAsync(apprenticeshipDataLocks);

            await _filter.Filter(123);

            _mockDataLockRepository.Verify(x => x.Delete(It.Is<long>(a => a == 4)), Times.Never);
        }

        [Test(Description = "When there are duplicate datalocks with the same effective date then delete the latest if it's for August price period.")]
        public async Task WhenHasDuplicateDatalocksForEffectiveDate()
        {
            DateTime duplicatIlreEffectiveFromDate = new DateTime(2017, 07, 01);

            List<DataLockStatus> apprenticeshipDataLocks = new List<DataLockStatus>
            {
                new DataLockStatus { DataLockEventId = 1, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/05/2017", IlrEffectiveFromDate = new DateTime(2017, 5, 1), Status = Status.Pass },
                new DataLockStatus { DataLockEventId = 2, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/06/2017", IlrEffectiveFromDate = new DateTime(2017, 6, 1), Status = Status.Pass },
                new DataLockStatus { DataLockEventId = 3, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/07/2017", IlrEffectiveFromDate = duplicatIlreEffectiveFromDate, Status = Status.Pass },
                new DataLockStatus { DataLockEventId = 4, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/08/2017", IlrEffectiveFromDate = duplicatIlreEffectiveFromDate, Status = Status.Pass }
            };

            _mockDataLockRepository.Setup(x => x.GetDataLocks(It.Is<long>(a => a == 123))).ReturnsAsync(apprenticeshipDataLocks);

            await _filter.Filter(123);

            _mockDataLockRepository.Verify(x => x.Delete(It.Is<long>(a => a == 4)), Times.Once);
        }

        [Test(Description = "When there are duplicate datalocks with the same effective date but the price episode isn't august do nothing other than log an error")]
        public async Task WhenHasDuplicateDataLocksButLatestIsntAugust()
        {
            DateTime duplicatIlreEffectiveFromDate = new DateTime(2017, 07, 01);

            List<DataLockStatus> apprenticeshipDataLocks = new List<DataLockStatus>
            {
                new DataLockStatus { DataLockEventId = 1, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/05/2017", IlrEffectiveFromDate = new DateTime(2017, 5, 1), Status = Status.Pass },
                new DataLockStatus { DataLockEventId = 2, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/06/2017", IlrEffectiveFromDate = new DateTime(2017, 6, 1), Status = Status.Pass },
                new DataLockStatus { DataLockEventId = 3, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/07/2017", IlrEffectiveFromDate = duplicatIlreEffectiveFromDate, Status = Status.Pass },
                new DataLockStatus { DataLockEventId = 4, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/09/2017", IlrEffectiveFromDate = duplicatIlreEffectiveFromDate, Status = Status.Pass }
            };

            _mockDataLockRepository.Setup(x => x.GetDataLocks(It.Is<long>(a => a == 123))).ReturnsAsync(apprenticeshipDataLocks);

            await _filter.Filter(123);

            _mockDataLockRepository.Verify(x => x.Delete(It.Is<long>(a => a == 4)), Times.Never);
            _mockLogger.Verify(x => x.Error(It.IsAny<AcademicYearFilterException>(), It.IsAny<string>()), Times.Once);
        }
    }
}

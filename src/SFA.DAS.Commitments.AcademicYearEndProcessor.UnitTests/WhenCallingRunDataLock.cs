using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob.Updater;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Messaging.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.AcademicYearEndProcessor.UnitTests
{
    [TestFixture]
    public class WhenCallingRunDataLock
    {
        private Mock<ILog> _logger;
        private Mock<IAcademicYearDateProvider> _academicYearProvider;
        private Mock<IDataLockRepository> _dataLockRepository;
        private IAcademicYearEndExpiryProcessor _academicYearEndProcessor;
        private Mock<IApprenticeshipUpdateRepository> _apprenticeshipUpdateRepository;

        [SetUp]
        public void Arrange()
        {
            // ARRANGE
            _logger = new Mock<ILog>();
            _academicYearProvider = new Mock<IAcademicYearDateProvider>();
            _dataLockRepository = new Mock<IDataLockRepository>();
            _apprenticeshipUpdateRepository = new Mock<IApprenticeshipUpdateRepository>();
        }
        


        [TestCase(2016, "2016-8-01", "2017-7-31", "2016-10-19 18:00", "2016-10-19 18:00:00")]
        [TestCase(2016, "2016-8-01", "2017-7-31", "2016-10-19 18:00", "2017-7-31 23:59:59")]
        [TestCase(2017, "2017-8-01", "2018-7-31", "2017-10-19 18:00", "2017-10-19 18:00:00")]
        [TestCase(2017, "2017-8-01", "2018-7-31", "2017-10-19 18:00", "2018-7-31 23:59:59")]
        [TestCase(2018, "2018-8-01", "2019-7-31", "2018-10-19 18:00", "2018-10-19 18:00:00")]
        [TestCase(2018, "2018-8-01", "2019-7-31", "2018-10-19 18:00", "2019-7-31 23:59:59")]
        public async Task ForAnyAcademicYearFromR14CutoffTimeThenExpirableItemsAreRetrievedAndExpired(
            int acYear,
            DateTime thisAcademicYearStartDate,
            DateTime thisAcademicYearEndDate,
            DateTime lastAcademicYearFundingPeriod,
            DateTime atTheTime

        )
        {

            // ARRANGE
            var testDatalockStatusItems = DatalockStatusTestData.GetData(acYear);

            var currentDatetime = new StubCurrentDateTime(atTheTime);

            _academicYearProvider.Setup(y => y.CurrentAcademicYearStartDate).Returns(thisAcademicYearStartDate);
            _academicYearProvider.Setup(y => y.CurrentAcademicYearEndDate).Returns(thisAcademicYearEndDate);
            _academicYearProvider.Setup(y => y.LastAcademicYearFundingPeriod).Returns(lastAcademicYearFundingPeriod);

            _academicYearEndProcessor = new AcademicYearEndExpiryProcessor(
                _logger.Object, 
                _academicYearProvider.Object, 
                _dataLockRepository.Object,
                _apprenticeshipUpdateRepository.Object,
                currentDatetime,
                Mock.Of<IMessagePublisher>());

            _dataLockRepository.Setup(r => r.GetExpirableDataLocks(_academicYearProvider.Object.CurrentAcademicYearStartDate)).ReturnsAsync(testDatalockStatusItems);

            var id = "jobId";
            // ACT
            await _academicYearEndProcessor.RunDataLock(id);

            //ASSERT
            _logger.Verify(
                x => x.Info(
                    $"{nameof(AcademicYearEndExpiryProcessor)} run at {currentDatetime.Now} for Academic Year CurrentAcademicYearStartDate: {thisAcademicYearStartDate}, CurrentAcademicYearEndDate: {thisAcademicYearEndDate}, LastAcademicYearFundingPeriod: {lastAcademicYearFundingPeriod}, JobId: {id}"),
                Times.Once);

            _dataLockRepository.Verify(x => x.GetExpirableDataLocks(_academicYearProvider.Object.CurrentAcademicYearStartDate), Times.Once);

            _dataLockRepository.Verify(r => r.UpdateExpirableDataLocks(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Exactly(testDatalockStatusItems.Count));

            _logger.Verify(x => x.Info($"{nameof(AcademicYearEndExpiryProcessor)} expired {testDatalockStatusItems.Count} items, JobId: {id}"), Times.Once);

        }

    }
}
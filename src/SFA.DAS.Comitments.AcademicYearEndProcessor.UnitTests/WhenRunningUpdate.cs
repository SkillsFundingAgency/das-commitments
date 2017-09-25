using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Comitments.AcademicYearEndProcessor.WebJob.Updater;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Comitments.AcademicYearEndProcessor.UnitTests
{
    [TestFixture]
    public class WhenRunningUpdate
    {
        [SetUp]
        public void Arrange()
        {
            // ARRANGE
            _logger = new Mock<ILog>();
            _academicYearProvider = new Mock<IAcademicYearDateProvider>();
            _dataLockRepository = new Mock<IDataLockRepository>();
            _testDatalockStatusItems = DatalockStatusTestData.GetData();
        }

        private Mock<ILog> _logger;
        private Mock<IAcademicYearDateProvider> _academicYearProvider;
        private Mock<IDataLockRepository> _dataLockRepository;
        private IAcademicYearEndExpiryProcessor _academicYearEndProcessor;
        private List<DataLockStatus> _testDatalockStatusItems = new List<DataLockStatus>();

        [TestCase("2018-8-01", "2019-7-31", "2018-10-19 18:00", "2018-10-19 17:59:59", 0)]
        [TestCase("2018-8-01", "2019-7-31", "2018-10-19 18:00", "2018-10-19 18:00:00", 5)]
        public async Task OnNextAcademicYearThenExpirableItemsAreRetrievedAndExpired(
            DateTime thisAcademicYearStartDate,
            DateTime thisAcademicYearEndDate,
            DateTime lastAcademicYearFundingPeriod,
            DateTime atTheTime,
            int expectedUpdates)
        {
            var explainFailure =
                $"For the Academic Year start {thisAcademicYearStartDate} end {thisAcademicYearEndDate} with prior year cutoff time of {lastAcademicYearFundingPeriod} expectations are;  at {atTheTime} there will be {expectedUpdates} updates";

            // ARRANGE
            var currentDatetime = new StubCurrentDateTime(atTheTime);

            _academicYearProvider.Setup(y => y.CurrentAcademicYearStartDate).Returns(thisAcademicYearStartDate);
            _academicYearProvider.Setup(y => y.CurrentAcademicYearEndDate).Returns(thisAcademicYearEndDate);
            _academicYearProvider.Setup(y => y.LastAcademicYearFundingPeriod).Returns(lastAcademicYearFundingPeriod);

            SetRepositoryResults(currentDatetime);

            _academicYearEndProcessor = new AcademicYearEndExpiryProcessor(_logger.Object, _academicYearProvider.Object,
                                                                            _dataLockRepository.Object,currentDatetime);
            // ACT
            await _academicYearEndProcessor.RunUpdate();

            //ASSERT
            _logger.Verify(x => x.Info($"{nameof(AcademicYearEndExpiryProcessor)} run at {currentDatetime.Now} for Academic Year CurrentAcademicYearStartDate: {thisAcademicYearStartDate}, CurrentAcademicYearEndDate: {thisAcademicYearEndDate}, LastAcademicYearFundingPeriod: {lastAcademicYearFundingPeriod}"), Times.Once);

            _dataLockRepository.Verify(x =>
                x.GetExpirableDataLocks(_academicYearProvider.Object.CurrentAcademicYearStartDate), Times.Once, explainFailure);

            _dataLockRepository.Verify(
                r => r.UpdateExpirableDataLocks(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.Exactly(expectedUpdates), explainFailure);


            _logger.Verify(x => x.Info($"{nameof(AcademicYearEndExpiryProcessor)} expired {expectedUpdates} items"), Times.Once);

        }

        private void SetRepositoryResults(StubCurrentDateTime currentDatetime)
        {
            var fakeResults = new List<DataLockStatus>();

            if (currentDatetime.Now >= _academicYearProvider.Object.LastAcademicYearFundingPeriod)
            {
                fakeResults = _testDatalockStatusItems.Where(
                    x => x.IlrEffectiveFromDate < _academicYearProvider.Object.CurrentAcademicYearStartDate &&
                         !x.IsExpired
                ).ToList();
            }

            _dataLockRepository.Setup(r => r.GetExpirableDataLocks(_academicYearProvider.Object.CurrentAcademicYearStartDate))
                 .ReturnsAsync(fakeResults);

        }

        [TestCase("2017-8-01", "2018-7-31", "2017-10-19 18:00", "2017-8-1", 0)]
        [TestCase("2017-8-01", "2018-7-31", "2017-10-19 18:00", "2017-9-1", 0)]
        [TestCase("2017-8-01", "2018-7-31", "2017-10-19 18:00", "2017-10-19 17:59:59", 0)]
        [TestCase("2017-8-01", "2018-7-31", "2017-10-19 18:00", "2017-10-19 18:00:00", 5)]
        public async Task OnCurrentAcademicYearThenExpirableItemsAreRetrievedAndExpired(
            DateTime thisAcademicYearStartDate,
            DateTime thisAcademicYearEndDate,
            DateTime lastAcademicYearFundingPeriod,
            DateTime atTheTime,
            int expectedUpdates
            )
        {
            var explainFailure =
                $"For the Academic Year start {thisAcademicYearStartDate} end {thisAcademicYearEndDate} with prior year cutoff time of {lastAcademicYearFundingPeriod} expectations are;  at {atTheTime} there will be {expectedUpdates} updates";

            // ARRANGE
            var currentDatetime = new StubCurrentDateTime(atTheTime);

            _academicYearProvider.Setup(y => y.CurrentAcademicYearStartDate).Returns(thisAcademicYearStartDate);
            _academicYearProvider.Setup(y => y.CurrentAcademicYearEndDate).Returns(thisAcademicYearEndDate);
            _academicYearProvider.Setup(y => y.LastAcademicYearFundingPeriod).Returns(lastAcademicYearFundingPeriod);

            SetRepositoryResults(currentDatetime);

            _academicYearEndProcessor = new AcademicYearEndExpiryProcessor(_logger.Object, _academicYearProvider.Object, _dataLockRepository.Object, currentDatetime);

            // ACT
            await _academicYearEndProcessor.RunUpdate();


            //ASSERT
            _logger.Verify(x => x.Info($"{nameof(AcademicYearEndExpiryProcessor)} run at {currentDatetime.Now} for Academic Year CurrentAcademicYearStartDate: {thisAcademicYearStartDate}, CurrentAcademicYearEndDate: {thisAcademicYearEndDate}, LastAcademicYearFundingPeriod: {lastAcademicYearFundingPeriod}"), Times.Once);

            _dataLockRepository.Verify(x =>
                x.GetExpirableDataLocks(_academicYearProvider.Object.CurrentAcademicYearStartDate), Times.Once, explainFailure);


            _dataLockRepository.Verify(
                r => r.UpdateExpirableDataLocks(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.Exactly(expectedUpdates), explainFailure);



            _logger.Verify(x => x.Info($"{nameof(AcademicYearEndExpiryProcessor)} expired {expectedUpdates} items"), Times.Once);

        }


        [TestCase("2016-8-01", "2017-7-31", "2016-10-19 18:00", "2017-7-31", 0)]
        public async Task OnPreviousAcademicYearThenThereAreNoExpirableItemsToRetrieveAndExpire(
            DateTime thisAcademicYearStartDate,
            DateTime thisAcademicYearEndDate,
            DateTime lastAcademicYearFundingPeriod,
            DateTime atTheTime,
            int expectedUpdates)
        {
            var explainFailure =
                $"For the Academic Year start {thisAcademicYearStartDate} end {thisAcademicYearEndDate} with prior year cutoff time of {lastAcademicYearFundingPeriod} expectations are;  at {atTheTime} there will be {expectedUpdates} updates";

            // ARRANGE
            var currentDatetime = new StubCurrentDateTime(atTheTime);

            _academicYearProvider.Setup(y => y.CurrentAcademicYearStartDate).Returns(thisAcademicYearStartDate);
            _academicYearProvider.Setup(y => y.CurrentAcademicYearEndDate).Returns(thisAcademicYearEndDate);
            _academicYearProvider.Setup(y => y.LastAcademicYearFundingPeriod).Returns(lastAcademicYearFundingPeriod);

            SetRepositoryResults(currentDatetime);

            _academicYearEndProcessor = new AcademicYearEndExpiryProcessor(_logger.Object,
                _academicYearProvider.Object,
                _dataLockRepository.Object,
                currentDatetime);

            // ACT
            await _academicYearEndProcessor.RunUpdate();

            //ASSERT
            _logger.Verify(x => x.Info($"{nameof(AcademicYearEndExpiryProcessor)} run at {currentDatetime.Now} for Academic Year CurrentAcademicYearStartDate: {thisAcademicYearStartDate}, CurrentAcademicYearEndDate: {thisAcademicYearEndDate}, LastAcademicYearFundingPeriod: {lastAcademicYearFundingPeriod}"), Times.Once);

            _dataLockRepository.Verify(x =>
                x.GetExpirableDataLocks(_academicYearProvider.Object.CurrentAcademicYearStartDate), Times.Once, explainFailure);



            _dataLockRepository.Verify(
                r => r.UpdateExpirableDataLocks(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.Exactly(expectedUpdates), explainFailure);


            _logger.Verify(x => x.Info($"{nameof(AcademicYearEndExpiryProcessor)} expired {expectedUpdates} items"), Times.Once);
        }
    }
}
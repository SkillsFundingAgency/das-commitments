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

namespace SFA.DAS.Comitments.AcademicYearEndProcessor.UnitTests
{
    [TestFixture]
    public class WhenRunningUpdate
    {
        [SetUp]
        public void Arrange()
        {
            // ARRANGE
            _academicYearProvider = new Mock<IAcademicYearDateProvider>();
            _dataLockRepository = new Mock<IDataLockRepository>();
            _testDatalockStatusItems = new List<DataLockStatus>
            {
                new DataLockStatus
                {
                    DataLockEventId = 1,
                    ApprenticeshipId = 121, PriceEpisodeIdentifier = "25-6-01/05/2017",
                    IlrEffectiveFromDate = new DateTime(2016, 6, 1), // in Acc.Yr 2015/16
                    ErrorCode = DataLockErrorCode.Dlock01 // not of interest
                },
                new DataLockStatus
                {
                    DataLockEventId = 2,
                    ApprenticeshipId = 122, PriceEpisodeIdentifier = "25-6-01/05/2017",
                    IlrEffectiveFromDate = new DateTime(2017, 6, 1), // in Acc.Yr 2016/17
                    ErrorCode = DataLockErrorCode.Dlock03 // of interest
                },
                new DataLockStatus
                {
                    DataLockEventId = 3,
                    ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/05/2017",
                    IlrEffectiveFromDate = new DateTime(2017, 6, 1), // in Acc.Yr 2016/17
                    ErrorCode = DataLockErrorCode.Dlock04, // of interest
                    IsExpired = true,
                    Expired = DateTime.MaxValue // but already expired
                },
                new DataLockStatus
                {
                    DataLockEventId = 4,
                    ApprenticeshipId = 124, PriceEpisodeIdentifier = "25-6-01/05/2017",
                    IlrEffectiveFromDate = new DateTime(2017, 6, 2), // in Acc.Yr 2016/17
                    ErrorCode = DataLockErrorCode.Dlock05 // of interest
                },
                new DataLockStatus
                {
                    DataLockEventId = 5,
                    ApprenticeshipId = 125, PriceEpisodeIdentifier = "25-6-01/05/2017",
                    IlrEffectiveFromDate = new DateTime(2017, 6, 3), // in Acc.Yr 2016/17
                    ErrorCode = DataLockErrorCode.Dlock06 // of interest
                },
                new DataLockStatus
                {
                    DataLockEventId = 6,
                    ApprenticeshipId = 126, PriceEpisodeIdentifier = "25-6-01/05/2017",
                    IlrEffectiveFromDate = new DateTime(2017, 6, 4), // in Acc.Yr 2016/17
                    ErrorCode = DataLockErrorCode.Dlock07 // of interest
                },
                new DataLockStatus
                {
                    DataLockEventId = 7,
                    ApprenticeshipId = 127, PriceEpisodeIdentifier = "25-6-01/05/2017",
                    IlrEffectiveFromDate = new DateTime(2017, 6, 5), // in Acc.Yr 2016/17
                    ErrorCode = DataLockErrorCode.Dlock03 |
                                DataLockErrorCode.Dlock04 |
                                DataLockErrorCode.Dlock05 |
                                DataLockErrorCode.Dlock06 |
                                DataLockErrorCode.Dlock07 // of interest
                },
                new DataLockStatus
                {
                    DataLockEventId = 8,
                    ApprenticeshipId = 128, PriceEpisodeIdentifier = "25-6-01/05/2017",
                    IlrEffectiveFromDate = new DateTime(2017, 8, 1), // in Acc.Yr 2017/18
                    ErrorCode = DataLockErrorCode.Dlock03 |
                                DataLockErrorCode.Dlock04 |
                                DataLockErrorCode.Dlock05 |
                                DataLockErrorCode.Dlock06 |
                                DataLockErrorCode.Dlock07 // of interest
                } 
            };
        }

        private Mock<IAcademicYearDateProvider> _academicYearProvider;
        private Mock<IDataLockRepository> _dataLockRepository;
        private IAcademicYearEndExpiryProcessor _academicYearEndProcessor;

        private List<DataLockStatus> _testDatalockStatusItems = new List<DataLockStatus>();

        private readonly DataLockErrorCode _expirableDataLockErrorCode =
                DataLockErrorCode.Dlock03 |
                DataLockErrorCode.Dlock04 |
                DataLockErrorCode.Dlock05 |
                DataLockErrorCode.Dlock06 |
                DataLockErrorCode.Dlock07
            ;

        [TestCase("2016-8-01", "2017-7-31", "2016-10-19 18:00", "2017-7-31", true, 0, null)]
        [TestCase("2016-8-01", "2017-7-31", "2016-10-19 18:00", "2017-9-1", false, 0, typeof(InvalidAcademicYearException))]
        [TestCase("2017-8-01", "2018-7-31", "2017-10-19 18:00", "2017-8-1", false, 0, null)]
        [TestCase("2017-8-01", "2018-7-31", "2017-10-19 18:00", "2017-9-1", false, 0, null)]
        [TestCase("2017-8-01", "2018-7-31", "2017-10-19 18:00", "2017-10-19 17:59:59", false, 0, null)]
        [TestCase("2017-8-01", "2018-7-31", "2017-10-19 18:00", "2017-10-19 18:00:00", true, 5, null)]
        [TestCase("2018-8-01", "2019-7-31", "2018-10-19 18:00", "2018-10-19 17:59:59", false, 0, null)]
        [TestCase("2018-8-01", "2019-7-31", "2018-10-19 18:00", "2018-10-19 18:00:00", true, 6, null)]
        public async Task ThenExpirableItemsAreRetrievedAndExpired(
            DateTime thisAcademicYearStartDate,
            DateTime thisAcademicYearEndDate,
            DateTime lastAcademicYearFundingPeriod,
            DateTime onTheDate,
            bool expectRetrieval,
            int expectedUpdates,
            Type expectedExceptionType)
        {
            var scenarioDescription =
                $"For the Academic Year start {thisAcademicYearStartDate} end {thisAcademicYearEndDate} with prior year cutoff time of {lastAcademicYearFundingPeriod} expectations are;  on {onTheDate} there will be {(expectRetrieval ? "" : "no ")}data retrieval and {expectedUpdates} updates";

            // ARRANGE
            var currentDatetime = new StubCurrentDateTime(onTheDate);


            _academicYearProvider.Setup(y => y.CurrentAcademicYearStartDate).Returns(thisAcademicYearStartDate);
            _academicYearProvider.Setup(y => y.CurrentAcademicYearEndDate).Returns(thisAcademicYearEndDate);
            _academicYearProvider.Setup(y => y.LastAcademicYearFundingPeriod).Returns(lastAcademicYearFundingPeriod);


            var fakeResults = new List<DataLockStatus>();

            if (currentDatetime.Now >= _academicYearProvider.Object.LastAcademicYearFundingPeriod)
            {
                fakeResults = _testDatalockStatusItems.Where(
                    x => x.IlrEffectiveFromDate < _academicYearProvider.Object.CurrentAcademicYearStartDate &&
                         _expirableDataLockErrorCode.HasFlag(x.ErrorCode) &&
                         !x.IsExpired
                ).ToList();
            }

            _dataLockRepository.Setup(r => r.GetExpirableDataLocks(
                _academicYearProvider.Object.CurrentAcademicYearStartDate,
                _expirableDataLockErrorCode)).ReturnsAsync(fakeResults);

            _academicYearEndProcessor = new AcademicYearEndExpiryProcessor(_academicYearProvider.Object,
                _dataLockRepository.Object, _expirableDataLockErrorCode, currentDatetime);

            Type actualExceptionType = null;
            Exception actualException = null;
            try
            {
                // ACT
                await _academicYearEndProcessor.RunUpdate();
            }
            catch (Exception e)
            {
                actualException = e;
                actualExceptionType = e.GetType();
            }

            //ASSERT
            if (actualException != null)
            {
                Assert.AreEqual(actualExceptionType, expectedExceptionType);
            }
            if (expectRetrieval)
            {
                _dataLockRepository.Verify(x =>
                    x.GetExpirableDataLocks(_academicYearProvider.Object.CurrentAcademicYearStartDate,
                        _expirableDataLockErrorCode), Times.Once, scenarioDescription);
                if (expectedUpdates == 0)
                    _dataLockRepository.Verify(r => r.UpdateExpirableDataLocks(It.IsAny<long>(), It.IsAny<string>()), Times.Never,
                        scenarioDescription);
                else
                    _dataLockRepository.Verify(r => r.UpdateExpirableDataLocks(It.IsAny<long>(), It.IsAny<string>()),
                        Times.Exactly(expectedUpdates), scenarioDescription);
            }
            else
            {
                _dataLockRepository.Verify(x =>
                    x.GetExpirableDataLocks(_academicYearProvider.Object.CurrentAcademicYearStartDate,
                        _expirableDataLockErrorCode), Times.Never, scenarioDescription);
            }
        }
    }
}
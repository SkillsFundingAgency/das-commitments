using System;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Services.AcademicYear
{
    [TestFixture]
    public class WhenDeterminingTheAcademicYear
    {
        private Mock<ICurrentDateTime> _currentDateTime;
        private IAcademicYearDateProvider _academicYear;

        [SetUp]
        public void Arrange()
        {
            _currentDateTime = new Mock<ICurrentDateTime>();
        }

        [TestCase("2017-08-01", "2017-08-01", "2018-07-31", "2017-10-19 18:00")]
        [TestCase("2018-03-01", "2017-08-01", "2018-07-31", "2017-10-19 18:00")]
        [TestCase("2018-07-31", "2017-08-01", "2018-07-31", "2017-10-19 18:00")]
        [TestCase("2018-10-01", "2018-08-01", "2019-07-31", "2018-10-19 18:00")]
        [TestCase("2018-01-01", "2017-08-01", "2018-07-31", "2017-10-19 18:00")]
        public void ThenAcademicYearRunsAugustToJuly(DateTime currentDate, DateTime expectedYearStart, DateTime expectedYearEnd, DateTime expectedLastAcademicYearFundingPeriod)
        {
            //Arrange
            _currentDateTime.Setup(x => x.Now).Returns(currentDate);
            _academicYear = new Infrastructure.Services.AcademicYearDateProvider(_currentDateTime.Object);

            //Act
            var actualStart = _academicYear.CurrentAcademicYearStartDate;
            var actualEnd = _academicYear.CurrentAcademicYearEndDate;
            var actualLastAcademicYearFundingPeriod = _academicYear.LastAcademicYearFundingPeriod;

            //Assert
            Assert.AreEqual(expectedYearStart, actualStart);
            Assert.AreEqual(expectedYearEnd, actualEnd);
            Assert.AreEqual(expectedLastAcademicYearFundingPeriod, actualLastAcademicYearFundingPeriod);
        }
    }
}

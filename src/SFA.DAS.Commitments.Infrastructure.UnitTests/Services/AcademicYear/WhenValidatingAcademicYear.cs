using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Entities.AcademicYear;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Services.AcademicYear
{
    [TestFixture]
    public class WhenValidatingAcademicYear
    {
        private Mock<ICurrentDateTime> _mockCurrentDateTime;
        private IAcademicYearDateProvider _academicYear;
        private IAcademicYearValidator _academicYearValidator;

        [SetUp]
        public void SetUp()
        {
            _mockCurrentDateTime = new Mock<ICurrentDateTime>();
         
        }

        [TestCase("2017-10-20", "2017-07-01", AcademicYearValidationResult.NotWithinFundingPeriod)]
        [TestCase("2017-10-20", "2016-07-01", AcademicYearValidationResult.NotWithinFundingPeriod)]
        [TestCase("2017-11-01", "2016-07-20", AcademicYearValidationResult.NotWithinFundingPeriod)]
        [TestCase("2017-10-19 18:05", "2017-07-19", AcademicYearValidationResult.NotWithinFundingPeriod)]
        [TestCase("2017-10-18", "2017-01-01", AcademicYearValidationResult.Success)]
        [TestCase("2018-10-01", "2018-06-01", AcademicYearValidationResult.Success)]
        [TestCase("2017-10-18", "2017-07-01", AcademicYearValidationResult.Success)]
        public void ThenAcademicYearValidationShouldReturnExpectedResult(DateTime currentDate, DateTime startDate, AcademicYearValidationResult expectedResult)
        {
            //Arrange
            var yearStartDate = new DateTime(2016, 8, 1);
            var fundingPeriodDate = new DateTime(2017, 10, 19);

            _mockCurrentDateTime.Setup(x => x.Now).Returns(currentDate);
            _academicYear = new Infrastructure.Services.AcademicYearDateProvider(_mockCurrentDateTime.Object);

            _academicYearValidator = new AcademicYearValidator(_mockCurrentDateTime.Object, _academicYear);
            //Act
            var result = _academicYearValidator.Validate(startDate);

            //Assert
            Assert.AreEqual(expectedResult, result);
       
        }


    }
}

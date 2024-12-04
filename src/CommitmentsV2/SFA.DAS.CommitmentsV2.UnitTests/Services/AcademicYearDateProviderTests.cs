using SFA.DAS.CommitmentsV2.Shared.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class AcademicYearDateProviderTests
    {
        [TestCase("2019-08-01", "2019-08-01", Description = "First day of academic year")]
        [TestCase("2020-01-01", "2019-08-01", Description = "First day of calendar year")]
        [TestCase("2020-07-31", "2019-08-01", Description = "Last day of academic year")]
        [TestCase("2020-08-01", "2020-08-01", Description = "First day of academic year (subsequent)")]
        public void AcademicYearStartDateIsCorrectlyCalculated(DateTime currentDate, DateTime expectedResult)
        {
            var academicYearDateProvider = new AcademicYearDateProvider(new CurrentDateTime(currentDate));
            var result = academicYearDateProvider.CurrentAcademicYearStartDate;
            Assert.That(result, Is.EqualTo(expectedResult));
        }      
    }
}

using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Domain.TrainingProgramme
{
    [TestFixture]
    public class WhenDeterminingIfATrainingProgrammeIsActive
    {
        [TestCase("2016-01-01", "2016-12-01", "2016-06-01", true, Description = "Within date range")]
        [TestCase("2016-01-15", "2016-12-15", "2016-01-01", true, Description = "Within date range - ignoring start day")]
        [TestCase("2016-01-15", "2016-12-15", "2016-12-30", false, Description = "After date range (but in same month as courseEnd")]
        [TestCase(null, "2016-12-01", "2016-06-01", true, Description = "Within date range with no defined course start date")]
        [TestCase("2016-01-01", null, "2016-06-01", true, Description = "Withing date range, with no defined course end date")]
        [TestCase(null, null, "2016-06-01", true, Description = "Within date range, with no defined course effective dates")]
        [TestCase("2016-01-01", "2016-12-01", "2015-06-01", false, Description = "Outside (before) date range")]
        [TestCase("2016-01-01", "2016-12-01", "2017-06-01", false, Description = "Outside (after) date range")]
        public void ThenIfWithinEffectiveRangeThenIsActive(DateTime? courseStart, DateTime? courseEnd,
            DateTime effectiveDate, bool expectIsActive)
        {
            //Arrange
            var course = new CommitmentsV2.Domain.Entities.TrainingProgramme("TEST", "TEST", ProgrammeType.Framework, courseStart, courseEnd);

            //Act
            var result = course.IsActiveOn(effectiveDate);

            //Assert
            Assert.That(result, Is.EqualTo(expectIsActive));
        }
    }
}

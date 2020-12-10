using System;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Extensions.TrainingProgramme
{
    [TestFixture]
    public class WhenDeterminingWhetherACourseIsActive
    {
        [TestCase("2016-01-01", "2016-12-01", "2016-06-01", true, Description = "Within date range")]
        [TestCase("2016-01-15", "2016-12-15", "2016-01-01", true, Description = "Within date range - ignoring start day")]
        //[TestCase("2016-01-15", "2016-12-15", "2016-12-30", true, Description = "Within date range - ignoring end day")]
        [TestCase("2016-01-15", "2016-12-15", "2016-12-30", false, Description = "After date range (but in same month as courseEnd")]
        [TestCase(null, "2016-12-01", "2016-06-01", true, Description = "Within date range with no defined course start date")]
        [TestCase("2016-01-01", null, "2016-06-01", true, Description = "Withing date range, with no defined course end date")]
        [TestCase(null, null, "2016-06-01", true, Description = "Within date range, with no defined course effective dates")]
        [TestCase("2016-01-01", "2016-12-01", "2015-06-01", false, Description = "Outside (before) date range")]
        [TestCase("2016-01-01", "2016-12-01", "2017-06-01", false, Description = "Outside (after) date range")]
        public void ThenIfWithinCourseEffectiveRangeThenIsActive(DateTime? courseStart, DateTime? courseEnd, DateTime effectiveDate, bool expectIsActive)
        {
            //Arrange
            var course = new CommitmentsV2.Domain.Entities.TrainingProgramme("1","test",ProgrammeType.Standard,courseStart,courseEnd);
            
            //Act
            var result = course.IsActiveOn(effectiveDate);

            //Assert
            Assert.AreEqual(expectIsActive, result);
        }
    }
}
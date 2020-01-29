using System;
using NUnit.Framework;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.ApprenticeshipSearchFilters
{
    public class WhenDeterminingIfFiltersExist
    {
        [Test]
        public void ThenWillReturnTrueIfEmployerNameSet()
        {
            //Arrange
            var filters = new CommitmentsV2.Models.ApprenticeshipSearchFilters
            {
                EmployerName = "Test"
            };

            //Act
            var result = filters.HasFilters;

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ThenWillReturnTrueIfCourseNameSet()
        {
            //Arrange
            var filters = new CommitmentsV2.Models.ApprenticeshipSearchFilters
            {
                CourseName = "Test"
            };

            //Act
            var result = filters.HasFilters;

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ThenWillReturnTrueIfStatusSet()
        {
            //Arrange
            var filters = new CommitmentsV2.Models.ApprenticeshipSearchFilters
            {
                Status = "Test"
            };

            //Act
            var result = filters.HasFilters;

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ThenWillReturnTrueIfStartDateSet()
        {
            //Arrange
            var filters = new CommitmentsV2.Models.ApprenticeshipSearchFilters
            {
                StartDate = DateTime.Now
            };

            //Act
            var result = filters.HasFilters;

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ThenWillReturnTrueIfEndDateSet()
        {
            //Arrange
            var filters = new CommitmentsV2.Models.ApprenticeshipSearchFilters
            {
               EndDate = DateTime.Now
            };

            //Act
            var result = filters.HasFilters;

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ThenWillReturnFalseIfNoFiltersSet()
        {
            //Arrange
            var filters = new CommitmentsV2.Models.ApprenticeshipSearchFilters();

            //Act
            var result = filters.HasFilters;

            //Assert
            Assert.IsFalse(result);
        }
    }
}

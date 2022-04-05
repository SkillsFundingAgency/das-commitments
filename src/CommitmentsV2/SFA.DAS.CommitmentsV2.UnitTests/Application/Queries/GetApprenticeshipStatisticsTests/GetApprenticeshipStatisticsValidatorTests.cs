using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatistics;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeshipStatisticsTests
{
    [TestFixture]
    public class GetApprenticeshipStatisticsValidatorTests
    {
        [TestCase(-10, false)]
        [TestCase(0, false)]
        [TestCase(10, true)]
        public void WhenCallingValidate_ThenCorrectlyValidatesLastNumberOfDays(int lastNumberOfDays,
            bool expectedValid)
        {
            //Arrange
            var validator = new GetApprenticeshipStatisticsQueryValidator();

            //Act
            var result = validator.Validate(new GetApprenticeshipStatisticsQuery { LastNumberOfDays = lastNumberOfDays });

            //Assert
            result.IsValid.Should().Be(expectedValid);
        }
    }
}
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohorts;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetCohorts
{
    [TestFixture]
    public class GetCohortsQueryValidationTests
    {
        [TestCase(1, true)]
        [TestCase(null, false)]
        public void Validate_WithAccountId_ShouldSetIsValidCorrectly(int id, bool expectedIsValid)
        {
            // arrange
            var validator = new GetCohortsQueryValidator();
            var validationResults = validator.Validate(new GetCohortsQuery(id));

            // act
            var actualIsValid = validationResults.IsValid;

            // Assert
            Assert.AreEqual(expectedIsValid, actualIsValid);
        }
    }
}

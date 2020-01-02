using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetPriceEpisodes
{
    [TestFixture]
    public class GetPriceEpisodesValidationTests
    {
            [TestCase(-1, false)]
            [TestCase(0, false)]
            [TestCase(1, true)]
            public void Validate_WithSpecifiedCohortId_ShouldSetIsValidCorrectly(int apprenticeshipId, bool expectedIsValid)
            {
                // arrange
                var validator = new GetPriceEpisodesQueryValidator();
                var validationResults = validator.Validate(new GetPriceEpisodesQuery(apprenticeshipId));

                // act
                var actualIsValid = validationResults.IsValid;

                // Assert
                Assert.AreEqual(expectedIsValid, actualIsValid);
            }
    }
}

using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship;
using FluentValidation.TestHelper;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetDraftApprentice
{
    [TestFixture]
    [Parallelizable]
    public class GetDraftApprenticeValidatorTests
    {
        [TestCase(-1, false)]
        [TestCase( 0, false)]
        [TestCase( 1, true)]

        public void Validate_WhenValidatingCohortId_ThenShouldRejectNonPositiveNumbers(long cohortId, bool expectToBeValid)
        {
            // arrange
            var validator = new GetDraftApprenticeshipQueryValidator();
            var request = new GetDraftApprenticeshipQuery(cohortId, 1);
            
            // act
            var result = validator.TestValidate(request);

            // assert
            if (expectToBeValid)
            {
                result.ShouldNotHaveValidationErrorFor(r => r.CohortId);
            }
            else
            {
                result.ShouldHaveValidationErrorFor(r => r.CohortId);
            }
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]

        public void Validate_WhenValidatingApprenticeshipId_ThenShouldRejectNonPositiveNumbers(long draftApprenticeshipId, bool expectToBeValid)
        {
            // arrange
            var validator = new GetDraftApprenticeshipQueryValidator();
            var request = new GetDraftApprenticeshipQuery(1, draftApprenticeshipId);

            // act
            var result = validator.TestValidate(request);
            
            // assert
            if (expectToBeValid)
            {
                result.ShouldNotHaveValidationErrorFor(r => r.DraftApprenticeshipId);
            }
            else
            {
                result.ShouldHaveValidationErrorFor(r => r.DraftApprenticeshipId);
            }
        }
    }
}
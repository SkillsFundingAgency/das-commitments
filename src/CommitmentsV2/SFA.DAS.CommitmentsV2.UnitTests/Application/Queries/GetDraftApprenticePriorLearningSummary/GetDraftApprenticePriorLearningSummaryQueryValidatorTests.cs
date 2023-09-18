using FluentValidation.TestHelper;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipPriorLearningSummary;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetDraftApprenticePriorLearningSummary
{
    [TestFixture]
    [Parallelizable]
    public class GetDraftApprenticePriorLearningSummaryQueryValidatorTests
    {
        [TestCase(-1, false)]
        [TestCase( 0, false)]
        [TestCase( 1, true)]

        public void Validate_WhenValidatingCohortId_ThenShouldRejectNonPositiveNumbers(long cohortId, bool expectToBeValid)
        {
            // arrange
            var validator = new GetDraftApprenticeshipPriorLearningSummaryQueryValidator();
            var request = new GetDraftApprenticeshipPriorLearningSummaryQuery(cohortId, 1);
            
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
            var validator = new GetDraftApprenticeshipPriorLearningSummaryQueryValidator();
            var request = new GetDraftApprenticeshipPriorLearningSummaryQuery(1, draftApprenticeshipId);
            
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
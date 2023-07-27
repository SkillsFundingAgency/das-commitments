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

            // assert
            if (expectToBeValid)
            {
                validator.ShouldNotHaveValidationErrorFor(r => r.CohortId, request, null);
            }
            else
            {
                validator.ShouldHaveValidationErrorFor(r => r.CohortId, request, null);
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
            if (expectToBeValid)
            {
                validator.ShouldNotHaveValidationErrorFor(r => r.DraftApprenticeshipId, request, null);
            }
            else
            {
                validator.ShouldHaveValidationErrorFor(r => r.DraftApprenticeshipId, request, null);
            }
        }
    }
}
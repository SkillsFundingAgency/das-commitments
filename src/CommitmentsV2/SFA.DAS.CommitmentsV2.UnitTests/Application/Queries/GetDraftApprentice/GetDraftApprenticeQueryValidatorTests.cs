using FluentValidation.Results;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProvider;
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
            var validator = new GetDraftApprenticeshipQueryValidator();
            var request = new GetDraftApprenticeshipQuery(1, draftApprenticeshipId);

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
using FluentValidation.Results;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipCreatedEventsForCohort;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetDraftApprenticeshipCreatedEventsForCohort
{
    [TestFixture]
    [Parallelizable]
    public class GetDraftApprenticeshipCreatedEventsForCohortQueryValidationTests
    {
        private GetDraftApprenticeshipCreatedEventsForCohortValidationTestsFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GetDraftApprenticeshipCreatedEventsForCohortValidationTestsFixture();
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_WhenValidatingProviderId_ThenShouldValidate(int providerId, bool isValid)
        {
            var validationResult = _fixture.Validate(providerId, 1);

            Assert.That(validationResult.IsValid, Is.EqualTo(isValid));
        }
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_WhenValidatingCohortId_ThenShouldValidate(int cohortId, bool isValid)
        {
            var validationResult = _fixture.Validate(1, cohortId);
            Assert.That(validationResult.IsValid, Is.EqualTo(isValid));
        }
    }

    public class GetDraftApprenticeshipCreatedEventsForCohortValidationTestsFixture
    {
        public GetDraftApprenticeshipCreatedEventsForCohortQueryValidator QueryValidator { get; set; }

        public GetDraftApprenticeshipCreatedEventsForCohortValidationTestsFixture()
        {
            QueryValidator = new GetDraftApprenticeshipCreatedEventsForCohortQueryValidator();
        }

        public ValidationResult Validate(long providerId, long cohortId)
        {
            return QueryValidator.Validate(new GetDraftApprenticeshipCreatedEventsForCohortQuery(providerId, cohortId, 10, DateTime.Now));
        }
    }
}
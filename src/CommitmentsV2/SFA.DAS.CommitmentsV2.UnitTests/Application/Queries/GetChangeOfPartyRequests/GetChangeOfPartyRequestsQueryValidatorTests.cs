using FluentValidation.Results;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequests;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetChangeOfPartyRequests
{
    [TestFixture]
    [Parallelizable]
    public class GetChangeOfPartyRequestsQueryValidatorTests
    {
        private GetChangeOfPartyRequestsQueryValidatorTestsFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GetChangeOfPartyRequestsQueryValidatorTestsFixture();
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_WhenValidating_ThenShouldValidate(int accountId, bool isValid)
        {
            var validationResult = _fixture.Validate(accountId);

            Assert.That(validationResult.IsValid, Is.EqualTo(isValid));
        }
    }

    public class GetChangeOfPartyRequestsQueryValidatorTestsFixture
    {
        public GetChangeOfPartyRequestsQueryValidator Validator { get; set; }

        public GetChangeOfPartyRequestsQueryValidatorTestsFixture()
        {
            Validator = new GetChangeOfPartyRequestsQueryValidator();
        }

        public ValidationResult Validate(long apprenticeshipId)
        {
            return Validator.Validate(new GetChangeOfPartyRequestsQuery(apprenticeshipId));
        }
    }
}

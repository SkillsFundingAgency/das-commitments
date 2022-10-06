using FluentValidation.Results;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetOverlappingTrainingDateRequest
{
    [TestFixture]
    [Parallelizable]
    public class GetOverlappingTrainingDateRequestQueryValidatorTests
    {
        private GetOverlappingTrainingDateRequestQueryValidatorTestsFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GetOverlappingTrainingDateRequestQueryValidatorTestsFixture();
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_WhenValidating_ThenShouldValidate(int accountId, bool isValid)
        {
            var validationResult = _fixture.Validate(accountId);

            Assert.AreEqual(isValid, validationResult.IsValid);
        }
    }

    public class GetOverlappingTrainingDateRequestQueryValidatorTestsFixture
    {
        public GetOverlappingTrainingDateRequestQueryValidator Validator { get; set; }

        public GetOverlappingTrainingDateRequestQueryValidatorTestsFixture()
        {
            Validator = new GetOverlappingTrainingDateRequestQueryValidator();
        }

        public ValidationResult Validate(long apprenticeshipId)
        {
            return Validator.Validate(new GetOverlappingTrainingDateRequestQuery(apprenticeshipId));
        }
    }
}
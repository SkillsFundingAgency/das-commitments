using FluentValidation.Results;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProvider;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetProvider
{
    [TestFixture]
    [Parallelizable]
    public class GetProviderQueryValidatorTests
    {
        private GetProviderQueryValidatorTestsFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GetProviderQueryValidatorTestsFixture();
        }
        
        [TestCase(-1, false)]
        [TestCase( 0, false)]
        [TestCase( 1, true)]
        public void Validate_WhenValidating_ThenShouldValidate(int providerId, bool isValid)
        {
            var validationResult = _fixture.Validate(providerId);
            
            Assert.That(validationResult.IsValid, Is.EqualTo(isValid));
        }
    }

    public class GetProviderQueryValidatorTestsFixture
    {
        public GetProviderQueryValidator Validator { get; set; }

        public GetProviderQueryValidatorTestsFixture()
        {
            Validator = new GetProviderQueryValidator();
        }

        public ValidationResult Validate(long providerId)
        {
            return Validator.Validate(new GetProviderQuery(providerId));
        }
    }
}
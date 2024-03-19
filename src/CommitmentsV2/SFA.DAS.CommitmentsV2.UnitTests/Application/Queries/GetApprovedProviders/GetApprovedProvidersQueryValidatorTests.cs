using FluentValidation.Results;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedProviders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetProvider
{
    [TestFixture]
    [Parallelizable]
    public class GetApprovedProvidersQueryValidatorTests
    {
        private GetApproveProvidersdQueryValidatorTestsFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GetApproveProvidersdQueryValidatorTestsFixture();
        }
        
        [TestCase(-1, false)]
        [TestCase( 0, false)]
        [TestCase( 1, true)]
        public void Validate_WhenValidating_ThenShouldValidate(int accountId, bool isValid)
        {
            var validationResult = _fixture.Validate(accountId);
            
            Assert.That(validationResult.IsValid, Is.EqualTo(isValid));
        }
    }

    public class GetApproveProvidersdQueryValidatorTestsFixture
    {
        public GetApprovedProvidersQueryValidator Validator { get; set; }

        public GetApproveProvidersdQueryValidatorTestsFixture()
        {
            Validator = new GetApprovedProvidersQueryValidator();
        }

        public ValidationResult Validate(long providerId)
        {
            return Validator.Validate(new GetApprovedProvidersQuery(providerId));
        }
    }
}
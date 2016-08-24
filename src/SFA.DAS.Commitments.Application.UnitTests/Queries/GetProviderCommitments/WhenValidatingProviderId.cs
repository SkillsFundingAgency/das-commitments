using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetProviderCommitments;

namespace SFA.DAS.Commitments.Application.UnitTests.GetProviderCommitments
{
    [TestFixture]
    public class WhenValidatingProviderId
    {
        private GetProviderCommitmentsValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new GetProviderCommitmentsValidator();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIfTheProviderIdIsZeroOrLessIsNotValid(long testProviderId)
        {
            var result = _validator.Validate(new GetProviderCommitmentsRequest { ProviderId = testProviderId });

            result.IsValid.Should().BeFalse();
        }

        [TestCase(1)]
        [TestCase(99999)]
        public void ThenIfTheProviderIdGreaterThanZeroIsValid(long testProviderId)
        {
            var result = _validator.Validate(new GetProviderCommitmentsRequest { ProviderId = testProviderId });

            result.IsValid.Should().BeTrue();
        }
    }
}

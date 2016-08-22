using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetProviderCommitments;

namespace SFA.DAS.Commitments.Application.UnitTests
{
    [TestFixture]
    public class TestClass
    {
        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIfTheProviderIdIsZeroOrLessIsNotValid(long testProviderId)
        {
            var validator = new GetProviderCommitmentsValidator();

            var result = validator.Validate(new GetProviderCommitmentsRequest { ProviderId = testProviderId });

            result.IsValid.Should().BeFalse();
        }

        [TestCase(1)]
        [TestCase(99999)]
        public void ThenIfTheProviderIdGreaterThanZeroIsValid(long testProviderId)
        {
            var validator = new GetProviderCommitmentsValidator();

            var result = validator.Validate(new GetProviderCommitmentsRequest { ProviderId = testProviderId });

            result.IsValid.Should().BeTrue();
        }
    }
}

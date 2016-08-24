using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetEmployerCommitments;

namespace SFA.DAS.Commitments.Application.UnitTests.GetEmployerCommitments
{
    [TestFixture]
    public class WhenValidatingAccountId
    {
        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIfTheAccountIdIsZeroOrLessIsNotValid(long testAccountId)
        {
            var validator = new GetEmployerCommitmentsValidator();

            var result = validator.Validate(new GetEmployerCommitmentsRequest { AccountId = testAccountId });

            result.IsValid.Should().BeFalse();
        }

        [TestCase(1)]
        [TestCase(99999)]
        public void ThenIfTheAccountIdGreaterThanZeroIsValid(long testProviderId)
        {
            var validator = new GetEmployerCommitmentsValidator();

            var result = validator.Validate(new GetEmployerCommitmentsRequest { AccountId = testProviderId });

            result.IsValid.Should().BeTrue();
        }
    }
}

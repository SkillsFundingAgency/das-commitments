using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetCommitment
{
    [TestFixture]
    public class WhenValidatingRequest
    {
        private GetCommitmentValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new GetCommitmentValidator();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIfTheCommitmentIdIsZeroOrLessIsNotValid(long testCommitmentId)
        {
            var result = _validator.Validate(new GetCommitmentRequest { CommitmentId = testCommitmentId, ProviderId = 1 });

            result.IsValid.Should().BeFalse();
        }

        [TestCase(1)]
        [TestCase(99999)]
        public void ThenIfTheAccountIdGreaterThanZeroIsValid(long testProviderId)
        {
            var result = _validator.Validate(new GetCommitmentRequest { CommitmentId = testProviderId, ProviderId = 1 });

            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void ThenIfBothProviderAndAccountIdsHaveAValueIsNotValid()
        {
            var result = _validator.Validate(new GetCommitmentRequest { CommitmentId = 1, ProviderId = 2, AccountId = 3 });

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ThenIfNeitherProviderAndAccountIdsHaveAValueIsNotValid()
        {
            var result = _validator.Validate(new GetCommitmentRequest { CommitmentId = 1, ProviderId = null, AccountId = null });

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIfTheAccountIdIsZeroOrLessIsNotValid(long testAccountId)
        {
            var result = _validator.Validate(new GetCommitmentRequest { CommitmentId = 1, ProviderId = null, AccountId = testAccountId });

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIfTheProviderIdIsZeroOrLessIsNotValid(long testProviderId)
        {
            var result = _validator.Validate(new GetCommitmentRequest { CommitmentId = 1, ProviderId = testProviderId, AccountId = null });

            result.IsValid.Should().BeFalse();
        }
    }
}

using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetCommitment
{
    [TestFixture]
    public sealed class WhenValidatingRequest
    {
        private const long ValidAccountId = 1L;
        private const long ValidCommitmentId = 1L;
        private const long ValidApprenticeshipId = 1L;
        private GetApprenticeshipValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new GetApprenticeshipValidator();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIfTheAccountIdIsZeroOrLessIsNotValid(long testAccountId)
        {
            var result = _validator.Validate(new GetApprenticeshipRequest { AccountId = testAccountId, CommitmentId = ValidCommitmentId, ApprenticeshipId = ValidApprenticeshipId });

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIfTheCommitmentIdIsZeroOrLessIsNotValid(long testCommitmentId)
        {
            var result = _validator.Validate(new GetApprenticeshipRequest { AccountId = ValidAccountId, CommitmentId = testCommitmentId, ApprenticeshipId = ValidApprenticeshipId });

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIfTheApprenticeshipIdIsZeroOrLessIsNotValid(long testApprenticeshipId)
        {
            var result = _validator.Validate(new GetApprenticeshipRequest { AccountId = ValidAccountId, CommitmentId = ValidCommitmentId, ApprenticeshipId = testApprenticeshipId });

            result.IsValid.Should().BeFalse();
        }

        //[Test]
        //public void ThenIfBothProviderAndAccountIdsHaveAValueIsNotValid()
        //{
        //    var result = _validator.Validate(new GetApprenticeshipRequest { CommitmentId = 1, ProviderId = 2, AccountId = 3 });
        //    var result = _validator.Validate(new GetApprenticeshipRequest { AccountId = ValidAccountId, CommitmentId = ValidCommitmentId, ApprenticeshipId = ValidApprenticeshipId });

        //    result.IsValid.Should().BeFalse();
        //}

        public void ThenIfTheIdsAreAllGreaterThanZeroItIsValid()
        {
            var result = _validator.Validate(new GetApprenticeshipRequest { AccountId = ValidAccountId, CommitmentId = ValidCommitmentId, ApprenticeshipId = ValidApprenticeshipId });

            result.IsValid.Should().BeTrue();
        }
    }
}

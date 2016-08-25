using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetCommitment
{
    [TestFixture]
    public class WhenValidatingCommitmentId
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
            var result = _validator.Validate(new GetCommitmentRequest { CommitmentId = testCommitmentId });

            result.IsValid.Should().BeFalse();
        }

        [TestCase(1)]
        [TestCase(99999)]
        public void ThenIfTheAccountIdGreaterThanZeroIsValid(long testProviderId)
        {
            var result = _validator.Validate(new GetCommitmentRequest { CommitmentId = testProviderId });

            result.IsValid.Should().BeTrue();
        }
    }
}

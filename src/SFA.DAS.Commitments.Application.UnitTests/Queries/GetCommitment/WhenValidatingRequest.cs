using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Domain;

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
            var result = _validator.Validate(new GetCommitmentRequest
            {
                CommitmentId = testCommitmentId,
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 1
                }
            });

            result.IsValid.Should().BeFalse();
        }

        [TestCase(1)]
        [TestCase(99999)]
        public void ThenIfTheAccountIdGreaterThanZeroIsValid(long testProviderId)
        {
            var result = _validator.Validate(new GetCommitmentRequest
            {
                CommitmentId = testProviderId,
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 1
                }
            });

            result.IsValid.Should().BeTrue();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIfTheAccountIdIsZeroOrLessIsNotValid(long testAccountId)
        {
            var result = _validator.Validate(new GetCommitmentRequest
            {
                CommitmentId = 1,
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = testAccountId
                }
            });

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIfTheProviderIdIsZeroOrLessIsNotValid(long testProviderId)
        {
            var result = _validator.Validate(new GetCommitmentRequest
            {
                CommitmentId = 1,
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = testProviderId
                }
            });

            result.IsValid.Should().BeFalse();
        }
    }
}

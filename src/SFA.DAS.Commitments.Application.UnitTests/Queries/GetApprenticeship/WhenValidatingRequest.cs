using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetApprenticeship
{
    [TestFixture]
    public sealed class WhenValidatingRequest
    {
        private const long ValidAccountId = 1L;
        private const long ValidProviderId = 1L;
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
            var result = _validator.Validate(new GetApprenticeshipRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = testAccountId
                },
                CommitmentId = ValidCommitmentId,
                ApprenticeshipId = ValidApprenticeshipId
            });

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIfTheCommitmentIdIsZeroOrLessIsNotValid(long testCommitmentId)
        {
            var result = _validator.Validate(new GetApprenticeshipRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = ValidAccountId
                },
                CommitmentId = testCommitmentId,
                ApprenticeshipId = ValidApprenticeshipId
            });

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIfTheApprenticeshipIdIsZeroOrLessIsNotValid(long testApprenticeshipId)
        {
            var result = _validator.Validate(new GetApprenticeshipRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = ValidAccountId
                },
                CommitmentId = ValidCommitmentId,
                ApprenticeshipId = testApprenticeshipId
            });

            result.IsValid.Should().BeFalse();
        }

        public void ThenIfAccountIdICommitmentIdAndApprenticeshipIdAreAllGreaterThanZeroItIsValid()
        {
            var result = _validator.Validate(new GetApprenticeshipRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = ValidAccountId
                },
                CommitmentId = ValidCommitmentId,
                ApprenticeshipId = ValidApprenticeshipId
            });

            result.IsValid.Should().BeTrue();
        }

        public void ThenIfProviderIdICommitmentIdAndApprenticeshipIdAreAllGreaterThanZeroItIsValid()
        {
            var result = _validator.Validate(new GetApprenticeshipRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = ValidProviderId
                },
                CommitmentId = ValidCommitmentId,
                ApprenticeshipId = ValidApprenticeshipId
            });

            result.IsValid.Should().BeTrue();
        }
    }
}

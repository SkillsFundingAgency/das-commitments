using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetCommitments;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.UnitTests.GetEmployerCommitments
{
    [TestFixture]
    public class WhenValidatingAccountId
    {
        private GetEmployerCommitmentsValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new GetEmployerCommitmentsValidator();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIfTheAccountIdIsZeroOrLessIsNotValid(long testAccountId)
        {
            var result = _validator.Validate(new GetCommitmentsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = testAccountId
                }
            });

            result.IsValid.Should().BeFalse();
        }

        [TestCase(1)]
        [TestCase(99999)]
        public void ThenIfTheAccountIdGreaterThanZeroIsValid(long testAccountId)
        {
            var result = _validator.Validate(new GetCommitmentsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = testAccountId
                }
            });

            result.IsValid.Should().BeTrue();
        }
    }
}

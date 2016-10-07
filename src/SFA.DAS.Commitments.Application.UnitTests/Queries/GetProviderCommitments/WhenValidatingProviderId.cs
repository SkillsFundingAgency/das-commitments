using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetCommitments;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.UnitTests.GetProviderCommitments
{
    [TestFixture]
    public class WhenValidatingProviderId
    {
        private GetCommitmentsValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new GetCommitmentsValidator();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIfTheProviderIdIsZeroOrLessIsNotValid(long testProviderId)
        {
            var result = _validator.Validate(new GetCommitmentsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = testProviderId
                }
            });

            result.IsValid.Should().BeFalse();
        }

        [TestCase(1)]
        [TestCase(99999)]
        public void ThenIfTheProviderIdGreaterThanZeroIsValid(long testProviderId)
        {
            var result = _validator.Validate(new GetCommitmentsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = testProviderId
                }
            });

            result.IsValid.Should().BeTrue();
        }
    }
}

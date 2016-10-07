using NUnit.Framework;
using Ploeh.AutoFixture;
using FluentAssertions;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Domain;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateApprenticeship
{
    [TestFixture]
    public class WhenValidatingCommand
    {
        private CreateApprenticeshipValidator _validator;
        private CreateApprenticeshipCommand _exampleCommand;

        [SetUp]
        public void Setup()
        {
            var fixture = new Fixture();

            _validator = new CreateApprenticeshipValidator();
            var populatedCommitment = fixture.Build<Apprenticeship>().Create();
            _exampleCommand = new CreateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 1
                },
                CommitmentId = 123L,
                Apprenticeship = populatedCommitment
            };
        }
        
        [Test]
        public void ThenIsInvalidIfApprenticeshipIsNull()
        {
            _exampleCommand.Apprenticeship = null;
            _exampleCommand.Caller = new Caller
            {
                CallerType = CallerType.Employer,
                Id = 1
            };

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void ThenCommitmentIdIsLessThanOneIsInvalid(long apprenticeshipId)
        {
            _exampleCommand.CommitmentId = apprenticeshipId;
            _exampleCommand.Caller = new Caller
            {
                CallerType = CallerType.Employer,
                Id = 1
            };

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ThenAccountIdNotSetAndProviderIdIsGreaterThanZeroIsValid()
        {
            _exampleCommand.Caller = new Caller
            {
                CallerType = CallerType.Provider,
                Id = 12
            };

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeTrue();
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void ThenIfAccouuntIdNotSetAndProviderIdIsLessThanOneIsInvalid(long providerId)
        {
            _exampleCommand.Caller = new Caller
            {
                CallerType = CallerType.Provider,
                Id = providerId
            };

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ThenProviderIdNotSetAndAccountIdIsGreaterThanZeroIsValid()
        {
            _exampleCommand.Caller = new Caller
            {
                CallerType = CallerType.Employer,
                Id = 12
            };

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeTrue();
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void ThenProviderIdNotSetAndAccountIdIsLessThanOneIsInvalid(long accountId)
        {
            _exampleCommand.Caller = new Caller
            {
                CallerType = CallerType.Employer,
                Id = accountId
            };

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }
    }
}

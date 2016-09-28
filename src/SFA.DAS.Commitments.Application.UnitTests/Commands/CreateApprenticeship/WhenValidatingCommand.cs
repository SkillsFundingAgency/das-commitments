using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types;
using Ploeh.AutoFixture;
using FluentAssertions;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;

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
            Fixture fixture = new Fixture();

            _validator = new CreateApprenticeshipValidator();
            var populatedCommitment = fixture.Build<Apprenticeship>().Create();
            _exampleCommand = new CreateApprenticeshipCommand { ProviderId = 1, AccountId = null, CommitmentId = 123L, Apprenticeship = populatedCommitment };
        }
        
        [Test]
        public void ThenIsInvalidIfApprenticeshipIsNull()
        {
            _exampleCommand.Apprenticeship = null;
            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void ThenCommitmentIdIsLessThanOneIsInvalid(long apprenticeshipId)
        {
            _exampleCommand.CommitmentId = apprenticeshipId;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ThenAccountIdNotSetAndProviderIdIsGreaterThanZeroIsValid()
        {
            _exampleCommand.ProviderId = 12;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeTrue();
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void ThenIfAccouuntIdNotSetAndProviderIdIsLessThanOneIsInvalid(long providerId)
        {
            _exampleCommand.ProviderId = providerId;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ThenProviderIdNotSetAndAccountIdIsGreaterThanZeroIsValid()
        {
            _exampleCommand.ProviderId = null;
            _exampleCommand.AccountId = 12;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeTrue();
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void ThenProviderIdNotSetAndAccountIdIsLessThanOneIsInvalid(long accountId)
        {
            _exampleCommand.AccountId = accountId;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ThenIfBothProviderAndAccountIdsHaveAValueIsNotValid()
        {
            _exampleCommand.AccountId = 123L;
            _exampleCommand.ProviderId = 233L;
            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        public void ThenIfProviderIdICommitmentIdAndApprenticeshipIdAreAllGreaterThanZeroItIsValid()
        {
            _exampleCommand.ProviderId = 321L;
            _exampleCommand.AccountId = null;
            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeTrue();
        }

        public void ThenIfAccountIdICommitmentIdAndApprenticeshipIdAreAllGreaterThanZeroItIsValid()
        {
            _exampleCommand.AccountId = 321L;
            _exampleCommand.ProviderId = null;
            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeTrue();
        }
    }
}

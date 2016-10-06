using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Api.Types;
using Ploeh.AutoFixture;
using FluentAssertions;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateCommitment
{
    [TestFixture]
    public class WhenValidatingCommitment
    {
        private CreateCommitmentValidator _validator;
        private CreateCommitmentCommand _exampleCommand;

        [SetUp]
        public void Setup()
        {
            Fixture fixture = new Fixture();

            _validator = new CreateCommitmentValidator();
            var populatedCommitment = fixture.Build<Commitment>().Create();
            _exampleCommand = new CreateCommitmentCommand { Commitment = populatedCommitment };
        }
        
        [Test]
        public void ThenIsInvalidIfCommitmentIsNull()
        {
            var result = _validator.Validate(new CreateCommitmentCommand { Commitment = null });

            result.IsValid.Should().BeFalse();
        }

        [TestCase(null)]
        [TestCase("")]
        public void ThenNameBeingNullOrEmptyIsInvalid(string commitmentName)
        {
            _exampleCommand.Commitment.Name = commitmentName;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void ThenAccountIdLessThanOneIsInvalid(long accountId)
        {
            _exampleCommand.Commitment.EmployerAccountId = accountId;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase("")]
        [TestCase("  ")]
        public void ThenLegalEntityCodeLessThanOneIsInvalid(string legalEntityCode)
        {
            _exampleCommand.Commitment.LegalEntityCode = legalEntityCode;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-3)]
        public void ThenProviderIdLessThanOneIsInvalid(long providerId)
        {
            _exampleCommand.Commitment.ProviderId = providerId;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ThenProviderIdOfNullIsValid()
        {
            _exampleCommand.Commitment.ProviderId = null;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeTrue();
        }
    }
}

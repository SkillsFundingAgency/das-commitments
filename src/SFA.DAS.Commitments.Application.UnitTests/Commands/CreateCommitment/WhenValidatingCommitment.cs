using System;

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
            fixture.Customize<Api.Types.Apprenticeship>(ob => ob
                .With(x => x.ULN, ApprenticeshipTestDataHelper.CreateValidULN())
            );
            _validator = new CreateCommitmentValidator();
            fixture.Customize<Api.Types.Apprenticeship>(ob => ob
                .With(x => x.ULN, ApprenticeshipTestDataHelper.CreateValidULN())
                .With(x => x.NINumber, ApprenticeshipTestDataHelper.CreateValidNino())
                .With(x => x.FirstName, "First name")
                .With(x => x.FirstName, "Last name")
                .With(x => x.ProviderRef, "Provider ref")
                .With(x => x.EmployerRef, null)
                .With(x => x.StartDate, DateTime.Now.AddYears(5))
                .With(x => x.EndDate, DateTime.Now.AddYears(7))
                .With(x => x.DateOfBirth, DateTime.Now.AddYears(-16))
            );
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
            _exampleCommand.Commitment.Reference = commitmentName;

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
        public void ThenLegalEntityCodeLessThanOneIsInvalid(string legalEntityId)
        {
            _exampleCommand.Commitment.LegalEntityId = legalEntityId;

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

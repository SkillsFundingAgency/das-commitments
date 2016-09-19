using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    [TestFixture]
    public sealed class WhenValidatingCommand
    {
        private UpdateApprenticeshipStatusValidator _validator;
        private UpdateApprenticeshipStatusCommand _exampleCommand;

        [SetUp]
        public void Setup()
        {

            _validator = new UpdateApprenticeshipStatusValidator();
            _exampleCommand = new UpdateApprenticeshipStatusCommand { AccountId = 1L, CommitmentId = 123L, ApprenticeshipId = 444L, Status = Api.Types.ApprenticeshipStatus.Approved };
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void ThenAccountIsLessThanOneIsInvalid(long accountId)
        {
            _exampleCommand.AccountId = accountId;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void ThenCommitmentIdIsLessThanOneIsInvalid(long commitmentId)
        {
            _exampleCommand.CommitmentId = commitmentId;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void ThenApprenticeshipIdIsLessThanOneIsInvalid(long apprenticeship)
        {
            _exampleCommand.CommitmentId = apprenticeship;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ThenStatusCodeIsNullIsInvalid()
        {
            _exampleCommand.Status = null;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase(-1)]
        [TestCase(3)]
        public void ThenIfStatusCodeIsNotValidValueIsNotValid(short statusCode)
        {
            _exampleCommand.Status = (ApprenticeshipStatus)statusCode;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }
    }
}

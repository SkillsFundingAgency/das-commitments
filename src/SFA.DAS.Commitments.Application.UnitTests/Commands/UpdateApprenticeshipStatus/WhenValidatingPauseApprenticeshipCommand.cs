using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    [TestFixture]
    public sealed class WhenValidatingPauseApprenticeshipCommand
    {
        [SetUp]
        public void Setup()
        {
            _validator = new ApprenticeshipStatusChangeCommandValidator();
            _exampleCommand = new PauseApprenticeshipCommand {AccountId = 1L, ApprenticeshipId = 444L};
        }

        private ApprenticeshipStatusChangeCommandValidator _validator;
        private PauseApprenticeshipCommand _exampleCommand;

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
        public void ThenApprenticeshipIdIsLessThanOneIsInvalid(long apprenticeship)
        {
            _exampleCommand.ApprenticeshipId = apprenticeship;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }
    }
}
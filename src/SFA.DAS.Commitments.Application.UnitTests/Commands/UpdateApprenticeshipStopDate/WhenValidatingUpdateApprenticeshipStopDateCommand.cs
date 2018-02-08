using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStopDate;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStopDate
{
    [TestFixture]
    public sealed class WhenValidatingUpdateApprenticeshipStopDateCommand
    {
        [SetUp]
        public void Setup()
        {
            _validator = new UpdateApprenticeshipStopDateCommandValidator();
            _exampleCommand = new UpdateApprenticeshipStopDateCommand {AccountId = 1L, ApprenticeshipId = 444L, StopDate = DateTime.Today};
        }

        private UpdateApprenticeshipStopDateCommandValidator _validator;
        private UpdateApprenticeshipStopDateCommand _exampleCommand;

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

        public void ThenStopDateLessThanTodayIsInvalid()
        {
            _exampleCommand.StopDate = DateTime.Today.AddDays(-1);

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }
    }
}
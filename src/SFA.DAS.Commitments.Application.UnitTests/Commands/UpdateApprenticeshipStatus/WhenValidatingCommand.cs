using FluentAssertions;
using NUnit.Framework;

using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    [TestFixture]
    public sealed class WhenValidatingStopApprenticeshipCommand
    {
        private ApprenticeshipStatusChangeCommandValidator _validator;
        private StopApprenticeshipCommand _exampleCommand;

        [SetUp]
        public void Setup()
        {

            _validator = new ApprenticeshipStatusChangeCommandValidator();
            _exampleCommand = new StopApprenticeshipCommand {AccountId = 1L, ApprenticeshipId = 444L};
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
        public void ThenApprenticeshipIdIsLessThanOneIsInvalid(long apprenticeship)
        {
            _exampleCommand.ApprenticeshipId = apprenticeship;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        //[Test]
        //public void ThenStatusCodeIsNullIsInvalid()
        //{
        //    _exampleCommand.PaymentStatus = null;

        //    var result = _validator.Validate(_exampleCommand);

        //    result.IsValid.Should().BeFalse();
        //}

        //[TestCase(-1)]
        //[TestCase(6)]
        //public void ThenIfStatusCodeIsNotValidValueIsNotValid(short statusCode)
        //{
        //    _exampleCommand.PaymentStatus = (Domain.Entities.PaymentStatus)statusCode;

        //    var result = _validator.Validate(_exampleCommand);

        //    result.IsValid.Should().BeFalse();
        //}
    }

    [TestFixture]
    public sealed class WhenValidatingPauseApprenticeshipCommand
    {
        private ApprenticeshipStatusChangeCommandValidator _validator;
        private PauseApprenticeshipCommand _exampleCommand;

        [SetUp]
        public void Setup()
        {

            _validator = new ApprenticeshipStatusChangeCommandValidator();
            _exampleCommand = new PauseApprenticeshipCommand { AccountId = 1L, ApprenticeshipId = 444L };
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
        public void ThenApprenticeshipIdIsLessThanOneIsInvalid(long apprenticeship)
        {
            _exampleCommand.ApprenticeshipId = apprenticeship;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        //[Test]
        //public void ThenStatusCodeIsNullIsInvalid()
        //{
        //    _exampleCommand.PaymentStatus = null;

        //    var result = _validator.Validate(_exampleCommand);

        //    result.IsValid.Should().BeFalse();
        //}

        //[TestCase(-1)]
        //[TestCase(6)]
        //public void ThenIfStatusCodeIsNotValidValueIsNotValid(short statusCode)
        //{
        //    _exampleCommand.PaymentStatus = (Domain.Entities.PaymentStatus)statusCode;

        //    var result = _validator.Validate(_exampleCommand);

        //    result.IsValid.Should().BeFalse();
        //}
    }

    [TestFixture]
    public sealed class WhenValidatingResumeApprenticeshipCommand
    {
        private ApprenticeshipStatusChangeCommandValidator _validator;
        private ResumeApprenticeshipCommand _exampleCommand;

        [SetUp]
        public void Setup()
        {

            _validator = new ApprenticeshipStatusChangeCommandValidator();
            _exampleCommand = new ResumeApprenticeshipCommand { AccountId = 1L, ApprenticeshipId = 444L };
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
        public void ThenApprenticeshipIdIsLessThanOneIsInvalid(long apprenticeship)
        {
            _exampleCommand.ApprenticeshipId = apprenticeship;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        //[Test]
        //public void ThenStatusCodeIsNullIsInvalid()
        //{
        //    _exampleCommand.PaymentStatus = null;

        //    var result = _validator.Validate(_exampleCommand);

        //    result.IsValid.Should().BeFalse();
        //}

        //[TestCase(-1)]
        //[TestCase(6)]
        //public void ThenIfStatusCodeIsNotValidValueIsNotValid(short statusCode)
        //{
        //    _exampleCommand.PaymentStatus = (Domain.Entities.PaymentStatus)statusCode;

        //    var result = _validator.Validate(_exampleCommand);

        //    result.IsValid.Should().BeFalse();
        //}
    }

}

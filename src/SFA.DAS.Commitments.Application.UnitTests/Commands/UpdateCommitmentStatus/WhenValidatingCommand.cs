using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentStatus;
using SFA.DAS.Commitments.Domain;
using CommitmentStatus = SFA.DAS.Commitments.Api.Types.CommitmentStatus;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateCommitmentStatus
{
    [TestFixture]
    public class WhenValidatingCommand
    {
        private UpdateCommitmentStatusValidator _validator;
        private UpdateCommitmentStatusCommand _exampleCommand;

        [SetUp]
        public void Setup()
        {

            _validator = new UpdateCommitmentStatusValidator();
            _exampleCommand = new UpdateCommitmentStatusCommand { Caller = new Caller {CallerType = CallerType.Employer, Id = 1L}, CommitmentId = 123L, Status = Api.Types.CommitmentStatus.Active };
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void ThenAccountIsLessThanOneIsInvalid(long accountId)
        {
            _exampleCommand.Caller.Id = accountId;

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

        [Test]
        public void ThenStatusCodeIsNullIsInvalid()
        {
            _exampleCommand.Status = null;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase(-1)]
        [TestCase(2)]
        public void ThenIfStatusCodeIsNotValidValueIsNotValid(short statusCode)
        {
            _exampleCommand.Status = (CommitmentStatus)statusCode;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }
    }
}

using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.EmployerApproveCohort;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CohortApproval.EmployerApproveCohort
{
    [TestFixture]
    public class WhenValidatingCommand
    {
        private EmployerApproveCohortCommandValidator _target;
        private EmployerApproveCohortCommand _command;

        [SetUp]
        public void Setup()
        {
            _target = new EmployerApproveCohortCommandValidator();

            _command = new EmployerApproveCohortCommand
            {
                Caller = new Caller(23, CallerType.Employer),
                CommitmentId = 123,
                LastUpdatedByEmail = "test@email.com",
                LastUpdatedByName = "Someone"
            };
        }

        [Test]
        public void ThenIsValidIfAllFieldsAreSetCorrectly()
        {
            var validationResult = _target.Validate(_command);
            Assert.IsTrue(validationResult.IsValid);
        }

        [Test]
        public void ThenIsInvalidIfCommitmentIdIsZero()
        {
            _command.CommitmentId = 0;
            var validationResult = _target.Validate(_command);
            Assert.IsFalse(validationResult.IsValid);
        }

        [Test]
        public void ThenIsInvalidIfCallerIsNull()
        {
            _command.Caller = null;
            var validationResult = _target.Validate(_command);
            Assert.IsFalse(validationResult.IsValid);
        }

        [Test]
        public void ThenIsInvalidIfCallerIdIsZero()
        {
            _command.Caller.Id = 0;
            var validationResult = _target.Validate(_command);
            Assert.IsFalse(validationResult.IsValid);
        }

        [Test]
        public void ThenIsInvalidIfCallerTypeIsNotEmployer()
        {
            _command.Caller.CallerType = CallerType.Provider;
            var validationResult = _target.Validate(_command);
            Assert.IsFalse(validationResult.IsValid);
        }

        [Test]
        public void ThenIsInvalidIfLastUpdatedByNameIsEmpty()
        {
            _command.LastUpdatedByName = string.Empty;
            var validationResult = _target.Validate(_command);
            Assert.IsFalse(validationResult.IsValid);
        }

        [Test]
        public void ThenIsInvalidIfLastUpdatedByEmailDoesNotMatchRegex()
        {
            _command.LastUpdatedByEmail = "fglkihdfkbgj";
            var validationResult = _target.Validate(_command);
            Assert.IsFalse(validationResult.IsValid);
        }
    }
}

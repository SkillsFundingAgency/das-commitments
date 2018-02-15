using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.CohortApproval.ProiderApproveCohort;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CohortApproval.ProviderApproveCohort
{
    [TestFixture]
    public class WhenValidatingCommand
    {
        private ProviderApproveCohortCommandValidator _target;
        private ProviderApproveCohortCommand _command;

        [SetUp]
        public void Setup()
        {
            _target = new ProviderApproveCohortCommandValidator();

            _command = new ProviderApproveCohortCommand
            {
                Caller = new Caller(23, CallerType.Provider),
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
        public void ThenIsInvalidIfCallerTypeIsNotProvider()
        {
            _command.Caller.CallerType = CallerType.Employer;
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

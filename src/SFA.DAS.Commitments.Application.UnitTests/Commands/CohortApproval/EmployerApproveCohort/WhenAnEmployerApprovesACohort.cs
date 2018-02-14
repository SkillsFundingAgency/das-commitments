using System;
using FluentValidation;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.EmployerApproveCohort;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CohortApproval.EmployerApproveCohort
{
    [TestFixture]
    public class WhenAnEmployerApprovesACohort
    {
        private AbstractValidator<EmployerApproveCohortCommand> _validator;
        private Mock<ICommitmentRepository> _commitmentRepository;
        private EmployerApproveCohortCommandHandler _target;
        private EmployerApproveCohortCommand _command;
        private Commitment _commitment;

        [SetUp]
        public void SetUp()
        {
            _command = new EmployerApproveCohortCommand { Caller = new Caller(213, CallerType.Employer), CommitmentId = 123, LastUpdatedByName = "Test", LastUpdatedByEmail = "test@email.com" };
            _commitment = new Commitment { CommitmentStatus = CommitmentStatus.New, EditStatus = EditStatus.ProviderOnly, Id = _command.CommitmentId };

            _validator = new EmployerApproveCohortCommandValidator();;
            _commitmentRepository = new Mock<ICommitmentRepository>();
            _commitmentRepository.Setup(x => x.GetCommitmentById(_command.CommitmentId)).ReturnsAsync(_commitment);
            
            _target = new EmployerApproveCohortCommandHandler(_validator, _commitmentRepository.Object);
        }

        [Test]
        public void ThenIfValidationFailsThenTheCommitmentCannotBeApproved()
        {
            _command.Caller = null;

            Assert.ThrowsAsync<ValidationException>(() => _target.Handle(_command));
        }

        [Test]
        public void ThenIfTheCommitmentIsDeletedThenItCannotBeApproved()
        {
            _commitment.CommitmentStatus = CommitmentStatus.Deleted;

            Assert.ThrowsAsync<InvalidOperationException>(() => _target.Handle(_command));
        }

        [Test]
        public void ThenIfTheCommitmentCanOnlyBeEditedByTheProviderThenItCannotBeApproved()
        {
            _commitment.EditStatus = EditStatus.ProviderOnly;

            Assert.ThrowsAsync<UnauthorizedException>(() => _target.Handle(_command));
        }

        [Test]
        public void ThenIfTheCommitmentCannotEditedThenItCannotBeApproved()
        {
            _commitment.EditStatus = EditStatus.Neither;

            Assert.ThrowsAsync<UnauthorizedException>(() => _target.Handle(_command));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.EmployerApproveCohort;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.Validation;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CohortApproval.EmployerApproveCohort
{
    [TestFixture]
    public class WhenAnEmployerApprovesACohort
    {
        private AbstractValidator<EmployerApproveCohortCommand> _validator;
        private Mock<ICommitmentRepository> _commitmentRepository;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private Mock<IApprenticeshipOverlapRules> _overlapRules;
        private EmployerApproveCohortCommandHandler _target;
        private EmployerApproveCohortCommand _command;
        private Commitment _commitment;

        [SetUp]
        public void SetUp()
        {
            _command = new EmployerApproveCohortCommand { Caller = new Caller(213, CallerType.Employer), CommitmentId = 123, LastUpdatedByName = "Test", LastUpdatedByEmail = "test@email.com" };
            _commitment = CreateCommitment();

            _validator = new EmployerApproveCohortCommandValidator();;
            _commitmentRepository = new Mock<ICommitmentRepository>();
            _commitmentRepository.Setup(x => x.GetCommitmentById(_command.CommitmentId)).ReturnsAsync(_commitment);
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            SetupSuccessfulOverlapCheck();

            _target = new EmployerApproveCohortCommandHandler(_validator, _commitmentRepository.Object, _apprenticeshipRepository.Object, _overlapRules.Object);
        }

        [Test]
        public void ThenIfValidationFailsTheCommitmentCannotBeApproved()
        {
            _command.Caller = null;

            Assert.ThrowsAsync<ValidationException>(() => _target.Handle(_command));
        }

        [Test]
        public void ThenIfTheCommitmentIsDeletedItCannotBeApproved()
        {
            _commitment.CommitmentStatus = CommitmentStatus.Deleted;

            Assert.ThrowsAsync<InvalidOperationException>(() => _target.Handle(_command));
        }

        [Test]
        public void ThenIfTheCommitmentCanOnlyBeEditedByTheProviderItCannotBeApproved()
        {
            _commitment.EditStatus = EditStatus.ProviderOnly;

            Assert.ThrowsAsync<UnauthorizedException>(() => _target.Handle(_command));
        }

        [Test]
        public void ThenIfTheCommitmentCannotEditedItCannotBeApproved()
        {
            _commitment.EditStatus = EditStatus.Neither;

            Assert.ThrowsAsync<UnauthorizedException>(() => _target.Handle(_command));
        }

        [Test]
        public void ThenIfTheCallerIsNotTheEmployerForTheCommitmentItCannotBeApproved()
        {
            _commitment.EmployerAccountId = _command.Caller.Id + 1;

            Assert.ThrowsAsync<UnauthorizedException>(() => _target.Handle(_command));
        }

        [Test]
        public void ThenIfTheCommitmentIsNotReadyToBeApprovedByTheEmployerItCannotBeApproved()
        {
            _commitment.EmployerCanApproveCommitment = false;

            Assert.ThrowsAsync<InvalidOperationException>(() => _target.Handle(_command));
        }

        [Test]
        public void ThenIfTheCommitmentHasOverlappingApprenticeshipsItCannotBeApproved()
        {
            var apprenticeship = _commitment.Apprenticeships.Last();
            var apprenticeshipResult = new ApprenticeshipResult { Uln = apprenticeship.ULN };
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.Is<IEnumerable<string>>(y => y.First() == _commitment.Apprenticeships.First().ULN && y.Last() == _commitment.Apprenticeships.Last().ULN))).ReturnsAsync(new List<ApprenticeshipResult> { apprenticeshipResult });
            _overlapRules.Setup(x => x.DetermineOverlap(It.Is<ApprenticeshipOverlapValidationRequest>(r => r.Uln == apprenticeship.ULN && r.ApprenticeshipId == apprenticeship.Id && r.StartDate == apprenticeship.StartDate.Value && r.EndDate == apprenticeship.EndDate.Value), apprenticeshipResult)).Returns(ValidationFailReason.OverlappingEndDate);

            Assert.ThrowsAsync<ValidationException>(() => _target.Handle(_command));
        }

        private Commitment CreateCommitment()
        {
            var apprenticeships = new List<Apprenticeship> { new Apprenticeship {  ULN = "1233435", Id = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddYears(1) }, new Apprenticeship { ULN = "894567645", Id = 2, StartDate = DateTime.Now.AddYears(-1), EndDate = DateTime.Now.AddYears(2) } };
            return new Commitment { CommitmentStatus = CommitmentStatus.New, EditStatus = EditStatus.EmployerOnly, Id = _command.CommitmentId, EmployerAccountId = _command.Caller.Id, EmployerCanApproveCommitment = true, Apprenticeships = apprenticeships };
        }

        private void SetupSuccessfulOverlapCheck()
        {
            var apprenticeship = _commitment.Apprenticeships.First();
            var apprenticeshipResult = new ApprenticeshipResult { Uln = apprenticeship.ULN };
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<ApprenticeshipResult> {apprenticeshipResult});
            _overlapRules = new Mock<IApprenticeshipOverlapRules>();
            _overlapRules.Setup(x => x.DetermineOverlap(It.Is<ApprenticeshipOverlapValidationRequest>(r => r.Uln == apprenticeship.ULN && r.ApprenticeshipId == apprenticeship.Id && r.StartDate == apprenticeship.StartDate.Value && r.EndDate == apprenticeship.EndDate.Value), apprenticeshipResult)).Returns(ValidationFailReason.None);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.EmployerApproveCohort;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Entities.Validation;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CohortApproval.EmployerApproveCohort
{
    [TestFixture]
    public class WhenAnEmployerApprovesACohort
    {
        private AbstractValidator<EmployerApproveCohortCommand> _validator;
        private Mock<ICommitmentRepository> _commitmentRepository;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private Mock<IApprenticeshipOverlapRules> _overlapRules;
        private Mock<ICurrentDateTime> _currentDateTime;
        private Mock<IHistoryRepository> _historyRepository;
        private EmployerApproveCohortCommandHandler _target;
        private EmployerApproveCohortCommand _command;
        private Commitment _commitment;

        [SetUp]
        public void SetUp()
        {
            _command = new EmployerApproveCohortCommand { Caller = new Caller(213, CallerType.Employer), CommitmentId = 123, LastUpdatedByName = "Test", LastUpdatedByEmail = "test@email.com", Message = "Some text" };
            _commitment = CreateCommitment();

            _validator = new EmployerApproveCohortCommandValidator();;
            _commitmentRepository = new Mock<ICommitmentRepository>();
            _commitmentRepository.Setup(x => x.GetCommitmentById(_command.CommitmentId)).ReturnsAsync(_commitment);
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _overlapRules = new Mock<IApprenticeshipOverlapRules>();
            SetupSuccessfulOverlapCheck();
            _currentDateTime = new Mock<ICurrentDateTime>();
            _historyRepository = new Mock<IHistoryRepository>();

            _target = new EmployerApproveCohortCommandHandler(_validator, _commitmentRepository.Object, _apprenticeshipRepository.Object, _overlapRules.Object, _currentDateTime.Object, _historyRepository.Object);
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

        [Test]
        public async Task ThenIfTheProviderHasNotYetApprovedTheApprenticeshipsAgreementsStatusesEmployerAgreedAndAreNotActive()
        {
            await _target.Handle(_command);

            Assert.IsTrue(_commitment.Apprenticeships.All(x => x.AgreementStatus == AgreementStatus.EmployerAgreed));
            Assert.IsTrue(_commitment.Apprenticeships.All(x => x.PaymentStatus == PaymentStatus.PendingApproval));
            Assert.IsTrue(_commitment.Apprenticeships.All(x => x.AgreedOn == null));
            _apprenticeshipRepository.Verify(x => x.UpdateApprenticeshipStatuses(_commitment.Apprenticeships), Times.Once());
        }

        [Test]
        public async Task ThenIfTheProviderHasNotYetApprovedTheCommitmentIsEditableByTheProviderAndHistoryIsCreated()
        {
            await _target.Handle(_command);

            Assert.AreEqual(EditStatus.ProviderOnly, _commitment.EditStatus);
            Assert.AreEqual(LastAction.Approve, _commitment.LastAction);
            Assert.AreEqual(_command.LastUpdatedByEmail, _commitment.LastUpdatedByEmployerEmail);
            Assert.AreEqual(_command.LastUpdatedByName, _commitment.LastUpdatedByEmployerName);
            _commitmentRepository.Verify(x => x.UpdateCommitment(_commitment), Times.Once);
            _historyRepository.Verify(x => x.InsertHistory(It.Is<IEnumerable<HistoryItem>>(y => VerifyHistoryItem(y.Single(), CommitmentChangeType.SentForApproval))), Times.Once);
        }

        [Test]
        public async Task ThenIfTheProviderHasAlreadyApprovedTheApprenticeshipsAgreementsAreAgreedAndTheAreActive()
        {
            var expectedAgreedOnDate = DateTime.Now;
            _currentDateTime.SetupGet(x => x.Now).Returns(expectedAgreedOnDate);

            _commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.ProviderAgreed);

            await _target.Handle(_command);

            Assert.IsTrue(_commitment.Apprenticeships.All(x => x.AgreementStatus == AgreementStatus.BothAgreed));
            Assert.IsTrue(_commitment.Apprenticeships.All(x => x.PaymentStatus == PaymentStatus.Active));
            Assert.IsTrue(_commitment.Apprenticeships.All(x => x.AgreedOn == expectedAgreedOnDate));
            _apprenticeshipRepository.Verify(x => x.UpdateApprenticeshipStatuses(_commitment.Apprenticeships), Times.Once());
        }

        [Test]
        public async Task ThenIfTheProviderHasAlreadyApprovedTheCommitmentIsEditableByBothPartiesAndHistoryIsCreated()
        {
            _commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.ProviderAgreed);

            await _target.Handle(_command);

            Assert.AreEqual(EditStatus.Both, _commitment.EditStatus);
            Assert.AreEqual(LastAction.Approve, _commitment.LastAction);
            Assert.AreEqual(_command.LastUpdatedByEmail, _commitment.LastUpdatedByEmployerEmail);
            Assert.AreEqual(_command.LastUpdatedByName, _commitment.LastUpdatedByEmployerName);
            _commitmentRepository.Verify(x => x.UpdateCommitment(_commitment), Times.Once);
            _historyRepository.Verify(x => x.InsertHistory(It.Is<IEnumerable<HistoryItem>>(y => VerifyHistoryItem(y.Single(), CommitmentChangeType.FinalApproval))), Times.Once);
        }

        [Test]
        public async Task ThenIfTheProviderHasAlreadyApprovedTheCommitmentPriceHistoryIsCreatedForTheApprenticeships()
        {
            _commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.ProviderAgreed);

            await _target.Handle(_command);

            _apprenticeshipRepository.Verify(x => x.CreatePriceHistoryForApprenticeshipsInCommitment(_commitment.Id));
            Assert.AreEqual(_commitment.Apprenticeships[0].Id, _commitment.Apprenticeships[0].PriceHistory[0].ApprenticeshipId);
            Assert.AreEqual(_commitment.Apprenticeships[0].StartDate, _commitment.Apprenticeships[0].PriceHistory[0].FromDate);
            Assert.AreEqual(_commitment.Apprenticeships[0].Cost, _commitment.Apprenticeships[0].PriceHistory[0].Cost);
            Assert.AreEqual(_commitment.Apprenticeships[1].Id, _commitment.Apprenticeships[1].PriceHistory[0].ApprenticeshipId);
            Assert.AreEqual(_commitment.Apprenticeships[1].StartDate, _commitment.Apprenticeships[1].PriceHistory[0].FromDate);
            Assert.AreEqual(_commitment.Apprenticeships[1].Cost, _commitment.Apprenticeships[1].PriceHistory[0].Cost);
        }

        [Test]
        public async Task ThenAMessageIsCreatedForTheProvider()
        {
            await _target.Handle(_command);

            Assert.AreEqual(_command.LastUpdatedByName, _commitment.Messages.Last().Author);
            Assert.AreEqual(_command.Message, _commitment.Messages.Last().Text);
            Assert.AreEqual(CallerType.Employer, _commitment.Messages.Last().CreatedBy);
            _commitmentRepository.Verify(x => x.SaveMessage(_commitment.Id, _commitment.Messages.Last()));
        }

        private Commitment CreateCommitment()
        {
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship {ULN = "1233435", Id = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddYears(1), AgreementStatus = AgreementStatus.NotAgreed, Cost = 2347 },
                new Apprenticeship {ULN = "894567645", Id = 2, StartDate = DateTime.Now.AddYears(-1), EndDate = DateTime.Now.AddYears(2), AgreementStatus = AgreementStatus.NotAgreed, Cost = 23812}
            };
            return new Commitment { CommitmentStatus = CommitmentStatus.New, EditStatus = EditStatus.EmployerOnly, Id = _command.CommitmentId, EmployerAccountId = _command.Caller.Id, EmployerCanApproveCommitment = true, Apprenticeships = apprenticeships, ProviderId = 12453 };
        }

        private void SetupSuccessfulOverlapCheck()
        {
            var apprenticeship = _commitment.Apprenticeships.First();
            var apprenticeshipResult = new ApprenticeshipResult { Uln = apprenticeship.ULN };
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<ApprenticeshipResult> {apprenticeshipResult});
            _overlapRules.Setup(x => x.DetermineOverlap(It.Is<ApprenticeshipOverlapValidationRequest>(r => r.Uln == apprenticeship.ULN && r.ApprenticeshipId == apprenticeship.Id && r.StartDate == apprenticeship.StartDate.Value && r.EndDate == apprenticeship.EndDate.Value), apprenticeshipResult)).Returns(ValidationFailReason.None);
        }

        private bool VerifyHistoryItem(HistoryItem historyItem, CommitmentChangeType changeType)
        {
            return historyItem.ChangeType == changeType.ToString() &&
                   historyItem.TrackedObject == _commitment &&
                   historyItem.CommitmentId == _commitment.Id &&
                   historyItem.UpdatedByRole == CallerType.Employer.ToString() &&
                   historyItem.UserId == _command.UserId &&
                   historyItem.ProviderId == _commitment.ProviderId &&
                   historyItem.EmployerAccountId == _commitment.EmployerAccountId &&
                   historyItem.UpdatedByName == _command.LastUpdatedByName;
        }
    }
}

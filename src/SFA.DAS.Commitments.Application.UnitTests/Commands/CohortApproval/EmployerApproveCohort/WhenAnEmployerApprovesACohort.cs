using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.CohortApproval.EmployerApproveCohort;
using SFA.DAS.Commitments.Application.Commands.SetPaymentOrder;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CohortApproval.EmployerApproveCohort
{
    [TestFixture]
    public class WhenAnEmployerApprovesACohort : ApproveCohortTestBase<EmployerApproveCohortCommand>
    {
        [SetUp]
        public void SetUp()
        {
            Validator = new EmployerApproveCohortCommandValidator();
            Command = new EmployerApproveCohortCommand { Caller = new Caller(213, CallerType.Employer), CommitmentId = 123, LastUpdatedByName = "Test", LastUpdatedByEmail = "test@email.com", Message = "Some text" };
            SetUpCommonMocks();
            Commitment = CreateCommitment(Command.CommitmentId, Command.Caller.Id, 234587);
            CommitmentRepository.Setup(x => x.GetCommitmentById(Command.CommitmentId)).ReturnsAsync(Commitment);
            SetupSuccessfulOverlapCheck();

            Target = new EmployerApproveCohortCommandHandler(Validator, CommitmentRepository.Object, ApprenticeshipRepository.Object, OverlapRules.Object, CurrentDateTime.Object, HistoryRepository.Object, ApprenticeshipEventsList.Object, ApprenticeshipEventsPublisher.Object, Mediator.Object, MessagePublisher.Object, Mock.Of<ICommitmentsLogger>());
        }

        [Test]
        public void ThenIfValidationFailsTheCommitmentCannotBeApproved()
        {
            Command.Caller = null;

            Assert.ThrowsAsync<ValidationException>(() => Target.Handle(Command));
        }

        [Test]
        public void ThenIfTheCommitmentCanOnlyBeEditedByTheProviderItCannotBeApproved()
        {
            Commitment.EditStatus = EditStatus.ProviderOnly;

            Assert.ThrowsAsync<InvalidOperationException>(() => Target.Handle(Command));
        }

        [Test]
        public void ThenIfTheCallerIsNotTheEmployerForTheCommitmentItCannotBeApproved()
        {
            Commitment.EmployerAccountId = Command.Caller.Id + 1;

            Assert.ThrowsAsync<UnauthorizedException>(() => Target.Handle(Command));
        }

        [Test]
        public void ThenIfTheCommitmentIsNotReadyToBeApprovedByTheEmployerItCannotBeApproved()
        {
            Commitment.EmployerCanApproveCommitment = false;

            Assert.ThrowsAsync<InvalidOperationException>(() => Target.Handle(Command));
        }

        [Test]
        public async Task ThenIfTheProviderHasNotYetApprovedTheApprenticeshipsAgreementsStatusesEmployerAgreedAndAreNotActive()
        {
            await Target.Handle(Command);

            Assert.IsTrue(Commitment.Apprenticeships.All(x => x.AgreementStatus == AgreementStatus.EmployerAgreed));
            Assert.IsTrue(Commitment.Apprenticeships.All(x => x.PaymentStatus == PaymentStatus.PendingApproval));
            Assert.IsTrue(Commitment.Apprenticeships.All(x => x.AgreedOn == null));
            ApprenticeshipRepository.Verify(x => x.UpdateApprenticeshipStatuses(Commitment.Apprenticeships), Times.Once());
        }

        [Test]
        public async Task ThenTheTransferApprovalStatusIsReset()
        {
            await Target.Handle(Command);
            Assert.IsNull(Commitment.TransferApprovalStatus);
        }

        [Test]
        public async Task ThenIfTheProviderHasNotYetApprovedTheCommitmentIsEditableByTheProviderAndHistoryIsCreated()
        {
            await Target.Handle(Command);

            Assert.AreEqual(EditStatus.ProviderOnly, Commitment.EditStatus);
            Assert.AreEqual(LastAction.Approve, Commitment.LastAction);
            Assert.AreEqual(CommitmentStatus.Active, Commitment.CommitmentStatus);
            Assert.AreEqual(Command.LastUpdatedByEmail, Commitment.LastUpdatedByEmployerEmail);
            Assert.AreEqual(Command.LastUpdatedByName, Commitment.LastUpdatedByEmployerName);
            CommitmentRepository.Verify(x => x.UpdateCommitment(Commitment), Times.Once);
            HistoryRepository.Verify(x => x.InsertHistory(It.Is<IEnumerable<HistoryItem>>(y => VerifyHistoryItem(y.Single(), CommitmentChangeType.SentForApproval, Command.UserId, Command.LastUpdatedByName, CallerType.Employer))), Times.Once);
        }

        [Test]
        public async Task ThenIfTheProviderHasNotYetApprovedEventsArePublishedForApprenticeshipsWithNoEffectiveFromDate()
        {
            await Target.Handle(Command);

            ApprenticeshipEventsList.Verify(x => x.Add(Commitment, Commitment.Apprenticeships[0], "APPRENTICESHIP-AGREEMENT-UPDATED", null, null), Times.Once);
            ApprenticeshipEventsList.Verify(x => x.Add(Commitment, Commitment.Apprenticeships[1], "APPRENTICESHIP-AGREEMENT-UPDATED", null, null), Times.Once);
            ApprenticeshipEventsPublisher.Verify(x => x.Publish(ApprenticeshipEventsList.Object), Times.Once);
        }

        [Test]
        public async Task ThenIfTheProviderHasAlreadyApprovedTheApprenticeshipsAgreementsAreAgreedAndTheAreActive()
        {
            var expectedAgreedOnDate = DateTime.Now;
            CurrentDateTime.SetupGet(x => x.Now).Returns(expectedAgreedOnDate);

            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.ProviderAgreed);

            await Target.Handle(Command);

            Assert.IsTrue(Commitment.Apprenticeships.All(x => x.AgreementStatus == AgreementStatus.BothAgreed));
            Assert.IsTrue(Commitment.Apprenticeships.All(x => x.PaymentStatus == PaymentStatus.Active));
            Assert.IsTrue(Commitment.Apprenticeships.All(x => x.AgreedOn == expectedAgreedOnDate));
            ApprenticeshipRepository.Verify(x => x.UpdateApprenticeshipStatuses(Commitment.Apprenticeships), Times.Once());
        }

        [Test]
        public async Task ThenIfTheProviderHasAlreadyApprovedTheCommitmentIsEditableByBothPartiesAndHistoryIsCreated()
        {
            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.ProviderAgreed);

            await Target.Handle(Command);

            Assert.AreEqual(EditStatus.Both, Commitment.EditStatus);
            Assert.AreEqual(LastAction.Approve, Commitment.LastAction);
            Assert.AreEqual(CommitmentStatus.Active, Commitment.CommitmentStatus);
            Assert.AreEqual(Command.LastUpdatedByEmail, Commitment.LastUpdatedByEmployerEmail);
            Assert.AreEqual(Command.LastUpdatedByName, Commitment.LastUpdatedByEmployerName);
            CommitmentRepository.Verify(x => x.UpdateCommitment(Commitment), Times.Once);
            HistoryRepository.Verify(x => x.InsertHistory(It.Is<IEnumerable<HistoryItem>>(y => VerifyHistoryItem(y.Single(), CommitmentChangeType.FinalApproval, Command.UserId, Command.LastUpdatedByName, CallerType.Employer))), Times.Once);
        }

        [Test]
        public async Task ThenIfTheProviderHasAlreadyApprovedTheCommitmentPriceHistoryIsCreatedForTheApprenticeships()
        {
            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.ProviderAgreed);

            await Target.Handle(Command);

            ApprenticeshipRepository.Verify(x => x.CreatePriceHistoryForApprenticeshipsInCommitment(Commitment.Id));
            Assert.AreEqual(Commitment.Apprenticeships[0].Id, Commitment.Apprenticeships[0].PriceHistory[0].ApprenticeshipId);
            Assert.AreEqual(Commitment.Apprenticeships[0].StartDate, Commitment.Apprenticeships[0].PriceHistory[0].FromDate);
            Assert.AreEqual(Commitment.Apprenticeships[0].Cost, Commitment.Apprenticeships[0].PriceHistory[0].Cost);
            Assert.AreEqual(Commitment.Apprenticeships[1].Id, Commitment.Apprenticeships[1].PriceHistory[0].ApprenticeshipId);
            Assert.AreEqual(Commitment.Apprenticeships[1].StartDate, Commitment.Apprenticeships[1].PriceHistory[0].FromDate);
            Assert.AreEqual(Commitment.Apprenticeships[1].Cost, Commitment.Apprenticeships[1].PriceHistory[0].Cost);
        }

        [Test]
        public async Task ThenIfTheProviderHasAlreadyApprovedTheCommitmentAndAnApprenticeHasAPreviousApprenticeshipEndingInTheSameMonthThenAnEventIsPublishedWithTheStartDateADayAfterThePreviousApprentieceshipStopped()
        {
            var apprenticeship = Commitment.Apprenticeships.First();
            var apprenticeshipResult = new ApprenticeshipResult { Uln = apprenticeship.ULN, StopDate = apprenticeship.StartDate.Value.AddDays(-10) };
            ApprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.Is<IEnumerable<string>>(y => y.First() == Commitment.Apprenticeships.First().ULN && y.Last() == Commitment.Apprenticeships.Last().ULN))).ReturnsAsync(new List<ApprenticeshipResult> { apprenticeshipResult });

            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.ProviderAgreed);

            await Target.Handle(Command);

            ApprenticeshipEventsList.Verify(x => x.Add(Commitment, Commitment.Apprenticeships[0], "APPRENTICESHIP-AGREEMENT-UPDATED", apprenticeshipResult.StopDate.Value.AddDays(1), null), Times.Once);
            ApprenticeshipEventsPublisher.Verify(x => x.Publish(ApprenticeshipEventsList.Object), Times.Once);
        }

        [Test]
        public async Task ThenIfTheProviderHasAlreadyApprovedTheCommitmentAndAnApprenticeHasAPreviousApprenticeshipEndingPriorToTheStartMonthThenAnEventIsPublishedWithTheStartDateAsTheFirstDayOfTheMonthTheApprentieceshipStarted()
        {
            var apprenticeship = Commitment.Apprenticeships.First();
            var apprenticeshipResult = new ApprenticeshipResult { Uln = apprenticeship.ULN, StopDate = apprenticeship.StartDate.Value.AddMonths(-1) };
            ApprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.Is<IEnumerable<string>>(y => y.First() == Commitment.Apprenticeships.First().ULN && y.Last() == Commitment.Apprenticeships.Last().ULN))).ReturnsAsync(new List<ApprenticeshipResult> { apprenticeshipResult });

            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.ProviderAgreed);

            await Target.Handle(Command);

            ApprenticeshipEventsList.Verify(x => x.Add(Commitment, Commitment.Apprenticeships[0], "APPRENTICESHIP-AGREEMENT-UPDATED", new DateTime(apprenticeship.StartDate.Value.Year, apprenticeship.StartDate.Value.Month, 1), null), Times.Once);
            ApprenticeshipEventsPublisher.Verify(x => x.Publish(ApprenticeshipEventsList.Object), Times.Once);
        }

        [Test]
        public async Task ThenIfTheProviderHasAlreadyApprovedTheCommitmentAndAnApprenticeDoesNotHaveAPreviousApprenticeshipThenAnEventIsPublishedWithTheStartDateAsTheFirstDayOfTheMonthTheApprentieceshipStarted()
        {
            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.ProviderAgreed);

            await Target.Handle(Command);

            ApprenticeshipEventsList.Verify(x => x.Add(Commitment, Commitment.Apprenticeships[0], "APPRENTICESHIP-AGREEMENT-UPDATED", new DateTime(Commitment.Apprenticeships[0].StartDate.Value.Year, Commitment.Apprenticeships[0].StartDate.Value.Month, 1), null), Times.Once);
            ApprenticeshipEventsPublisher.Verify(x => x.Publish(ApprenticeshipEventsList.Object), Times.Once);
        }

        [Test]
        public async Task ThenIfTheProviderHasAlreadyApprovedThePaymentOrderIsUpdated()
        {
            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.ProviderAgreed);

            await Target.Handle(Command);

            Mediator.Verify(x => x.SendAsync(It.Is<SetPaymentOrderCommand>(c => c.AccountId == Commitment.EmployerAccountId)));
        }

        [Test]
        public async Task ThenAMessageIsCreatedForTheProvider()
        {
            await Target.Handle(Command);

            Assert.AreEqual(Command.LastUpdatedByName, Commitment.Messages.Last().Author);
            Assert.AreEqual(Command.Message, Commitment.Messages.Last().Text);
            Assert.AreEqual(CallerType.Employer, Commitment.Messages.Last().CreatedBy);
            CommitmentRepository.Verify(x => x.SaveMessage(Commitment.Id, Commitment.Messages.Last()));
        }

        [Test]
        public async Task ThenIfTheProviderHasAlreadyApprovedAnApprovalMessageIsPublished()
        {
            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.ProviderAgreed);

            await Target.Handle(Command);

            MessagePublisher.Verify(x => x.PublishAsync(It.Is<CohortApprovedByEmployer>(y => y.ProviderId == Commitment.ProviderId && y.AccountId == Commitment.EmployerAccountId && y.CommitmentId == Commitment.Id)));
        }
    }
}

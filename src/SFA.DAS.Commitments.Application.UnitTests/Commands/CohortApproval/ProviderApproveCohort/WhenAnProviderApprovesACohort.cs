using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.CohortApproval.ProiderApproveCohort;
using SFA.DAS.Commitments.Application.Commands.SetPaymentOrder;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Types;
using AgreementStatus = SFA.DAS.Commitments.Domain.Entities.AgreementStatus;
using CommitmentStatus = SFA.DAS.Commitments.Domain.Entities.CommitmentStatus;
using EditStatus = SFA.DAS.Commitments.Domain.Entities.EditStatus;
using LastAction = SFA.DAS.Commitments.Domain.Entities.LastAction;
using PaymentStatus = SFA.DAS.Commitments.Domain.Entities.PaymentStatus;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CohortApproval.ProviderApproveCohort
{
    [TestFixture]
    public class WhenAProviderApprovesACohort : ApproveCohortTestBase<ProviderApproveCohortCommand>
    {
        [SetUp]
        public void SetUp()
        {
            Validator = new ProviderApproveCohortCommandValidator();
            Command = new ProviderApproveCohortCommand { Caller = new Caller(213, CallerType.Provider), CommitmentId = 123, LastUpdatedByName = "Test", LastUpdatedByEmail = "test@email.com", Message = "Some text" };
            SetUpCommonMocks();
            Commitment = CreateCommitment(Command.CommitmentId, 11234, Command.Caller.Id);
            Commitment.EditStatus = EditStatus.ProviderOnly;
            Account = CreateAccount(Commitment.EmployerAccountId, ApprenticeshipEmployerType.Levy);
            CommitmentRepository.Setup(x => x.GetCommitmentById(Command.CommitmentId)).ReturnsAsync(Commitment);
            EmployerAccountsService.Setup(x => x.GetAccount(Commitment.EmployerAccountId)).ReturnsAsync(Account);
            SetupSuccessfulOverlapCheck();
            
            Target = new ProviderApproveCohortCommandHandler(Validator,
                CommitmentRepository.Object,
                ApprenticeshipRepository.Object,
                OverlapRules.Object,
                CurrentDateTime.Object,
                HistoryRepository.Object,
                ApprenticeshipEventsList.Object,
                ApprenticeshipEventsPublisher.Object,
                Mediator.Object,
                MessagePublisher.Object,
                Mock.Of<ICommitmentsLogger>(),
                Mock.Of<IApprenticeshipInfoService>(),
                FeatureToggleService.Object,
                EmployerAccountsService.Object,
                NotificationsPublisher.Object);
        }

        [Test]
        public void ThenIfValidationFailsTheCommitmentCannotBeApproved()
        {
            Command.Caller = null;

            Assert.ThrowsAsync<ValidationException>(() => Target.Handle(Command));
        }

        [Test]
        public void ThenIfTheCommitmentCanOnlyBeEditedByEmployerItCannotBeApproved()
        {
            Commitment.EditStatus = EditStatus.EmployerOnly;

            Assert.ThrowsAsync<InvalidOperationException>(() => Target.Handle(Command));
        }

        [Test]
        public void ThenIfTheCallerIsNotTheProviderForTheCommitmentItCannotBeApproved()
        {
            Commitment.ProviderId = Command.Caller.Id + 1;

            Assert.ThrowsAsync<UnauthorizedException>(() => Target.Handle(Command));
        }

        [Test]
        public void ThenIfTheCommitmentIsNotReadyToBeApprovedByTheProviderItCannotBeApproved()
        {
            Commitment.ProviderCanApproveCommitment = false;

            Assert.ThrowsAsync<InvalidOperationException>(() => Target.Handle(Command));
        }

        [Test]
        public async Task ThenIfTheEmployerHasNotYetApprovedTheApprenticeshipsAgreementsStatusesProviderAgreedAndAreNotActive()
        {
            await Target.Handle(Command);

            Assert.IsTrue(Commitment.Apprenticeships.All(x => x.AgreementStatus == AgreementStatus.ProviderAgreed));
            Assert.IsTrue(Commitment.Apprenticeships.All(x => x.PaymentStatus == PaymentStatus.PendingApproval));
            Assert.IsTrue(Commitment.Apprenticeships.All(x => x.AgreedOn == null));
            ApprenticeshipRepository.Verify(x => x.UpdateApprenticeshipStatuses(Commitment.Id, PaymentStatus.PendingApproval, AgreementStatus.ProviderAgreed, null), Times.Once());
        }

        [Test]
        public async Task ThenIfTheEmployerHasNotYetApprovedTheCommitmentIsEditableByTheProviderAndHistoryIsCreated()
        {
            await Target.Handle(Command);

            Assert.AreEqual(EditStatus.EmployerOnly, Commitment.EditStatus);
            Assert.AreEqual(LastAction.Approve, Commitment.LastAction);
            Assert.AreEqual(CommitmentStatus.Active, Commitment.CommitmentStatus);
            Assert.AreEqual(Command.LastUpdatedByEmail, Commitment.LastUpdatedByProviderEmail);
            Assert.AreEqual(Command.LastUpdatedByName, Commitment.LastUpdatedByProviderName);
            CommitmentRepository.Verify(x => x.UpdateCommitment(Commitment), Times.Once);
            HistoryRepository.Verify(x => x.InsertHistory(It.Is<IEnumerable<HistoryItem>>(y => VerifyHistoryItem(y.Single(), CommitmentChangeType.SentForApproval, Command.UserId, Command.LastUpdatedByName, CallerType.Provider))), Times.Once);
        }

        [Test]
        public async Task ThenIfTheEmployerHasNotYetApprovedTheCommitmentACohortApprovalRequestedByProviderIsPublished()
        {
            await Target.Handle(Command);

            MessagePublisher.Verify(x => x.PublishAsync(It.Is<CohortApprovalRequestedByProvider>(y => y.ProviderId == Commitment.ProviderId && y.AccountId == Commitment.EmployerAccountId && y.CommitmentId == Commitment.Id)));
        }

        [Test]
        public async Task ThenIfTheEmployerHasNotYetApprovedEventsArePublishedForApprenticeshipsWithNoEffectiveFromDate()
        {
            await Target.Handle(Command);

            ApprenticeshipEventsList.Verify(x => x.Add(Commitment, Commitment.Apprenticeships[0], "APPRENTICESHIP-AGREEMENT-UPDATED", null, null), Times.Once);
            ApprenticeshipEventsList.Verify(x => x.Add(Commitment, Commitment.Apprenticeships[1], "APPRENTICESHIP-AGREEMENT-UPDATED", null, null), Times.Once);
            ApprenticeshipEventsPublisher.Verify(x => x.Publish(ApprenticeshipEventsList.Object), Times.Once);
        }

        [Test]
        public async Task ThenIfTheEmployerHasAlreadyApprovedTheApprenticeshipsAgreementsAreAgreedAndTheAreActive()
        {
            var expectedAgreedOnDate = DateTime.Now;
            CurrentDateTime.SetupGet(x => x.Now).Returns(expectedAgreedOnDate);

            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.EmployerAgreed);

            await Target.Handle(Command);

            Assert.IsTrue(Commitment.Apprenticeships.All(x => x.AgreementStatus == AgreementStatus.BothAgreed));
            Assert.IsTrue(Commitment.Apprenticeships.All(x => x.PaymentStatus == PaymentStatus.Active));
            Assert.IsTrue(Commitment.Apprenticeships.All(x => x.AgreedOn == expectedAgreedOnDate));
            ApprenticeshipRepository.Verify(x => x.UpdateApprenticeshipStatuses(Commitment.Id, PaymentStatus.Active, AgreementStatus.BothAgreed, expectedAgreedOnDate), Times.Once());
        }

        [Test]
        public async Task ThenIfTheEmployerHasAlreadyApprovedTheCommitmentIsEditableByBothPartiesAndHistoryIsCreated()
        {
            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.EmployerAgreed);

            await Target.Handle(Command);

            Assert.AreEqual(EditStatus.Both, Commitment.EditStatus);
            Assert.AreEqual(LastAction.Approve, Commitment.LastAction);
            Assert.AreEqual(CommitmentStatus.Active, Commitment.CommitmentStatus);
            Assert.AreEqual(Command.LastUpdatedByEmail, Commitment.LastUpdatedByProviderEmail);
            Assert.AreEqual(Command.LastUpdatedByName, Commitment.LastUpdatedByProviderName);
            CommitmentRepository.Verify(x => x.UpdateCommitment(Commitment), Times.Once);
            HistoryRepository.Verify(x => x.InsertHistory(It.Is<IEnumerable<HistoryItem>>(y => VerifyHistoryItem(y.Single(), CommitmentChangeType.FinalApproval, Command.UserId, Command.LastUpdatedByName, CallerType.Provider))), Times.Once);
        }

        [TestCase(false, null)]
        [TestCase(true, ApprenticeshipEmployerType.NonLevy)]
        [TestCase(true, ApprenticeshipEmployerType.Levy)]
        public async Task ThenIfTheProviderHasAlreadyApprovedTheCommitmentApprenticeshipEmployerTypeIsSet(bool isManageReservationsEnabled, ApprenticeshipEmployerType? apprenticeshipEmployerType)
        {
            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.EmployerAgreed);
            FeatureToggleService.Setup(s => s.IsEnabled("ManageReservations")).Returns(isManageReservationsEnabled);
            Account.ApprenticeshipEmployerType = apprenticeshipEmployerType.GetValueOrDefault();
            
            await Target.Handle(Command);
            
            Assert.AreEqual(apprenticeshipEmployerType, Commitment.ApprenticeshipEmployerTypeOnApproval);
        }

        [Test]
        public async Task ThenIfTheEmployerHasAlreadyApprovedTheCommitmentPriceHistoryIsCreatedForTheApprenticeships()
        {
            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.EmployerAgreed);

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
        public async Task ThenIfTheEmployerHasAlreadyApprovedTheCommitmentAndAnApprenticeHasAPreviousApprenticeshipEndingInTheSameMonthThenAnEventIsPublishedWithTheStartDateADayAfterThePreviousApprentieceshipStopped()
        {
            var apprenticeship = Commitment.Apprenticeships.First();
            var apprenticeshipResult = new ApprenticeshipResult { Uln = apprenticeship.ULN, StopDate = apprenticeship.StartDate.Value.AddDays(-10) };
            ApprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.Is<IEnumerable<string>>(y => y.First() == Commitment.Apprenticeships.First().ULN && y.Last() == Commitment.Apprenticeships.Last().ULN))).ReturnsAsync(new List<ApprenticeshipResult> { apprenticeshipResult });

            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.EmployerAgreed);

            await Target.Handle(Command);

            ApprenticeshipEventsList.Verify(x => x.Add(Commitment, Commitment.Apprenticeships[0], "APPRENTICESHIP-AGREEMENT-UPDATED", apprenticeshipResult.StopDate.Value.AddDays(1), null), Times.Once);
            ApprenticeshipEventsPublisher.Verify(x => x.Publish(ApprenticeshipEventsList.Object), Times.Once);
        }

        [Test]
        public async Task ThenIfTheEmployerHasAlreadyApprovedTheCommitmentAndAnApprenticeHasAPreviousApprenticeshipEndingPriorToTheStartMonthThenAnEventIsPublishedWithTheStartDateAsTheFirstDayOfTheMonthTheApprentieceshipStarted()
        {
            var apprenticeship = Commitment.Apprenticeships.First();
            var apprenticeshipResult = new ApprenticeshipResult { Uln = apprenticeship.ULN, StopDate = apprenticeship.StartDate.Value.AddMonths(-1) };
            ApprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.Is<IEnumerable<string>>(y => y.First() == Commitment.Apprenticeships.First().ULN && y.Last() == Commitment.Apprenticeships.Last().ULN))).ReturnsAsync(new List<ApprenticeshipResult> { apprenticeshipResult });

            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.EmployerAgreed);

            await Target.Handle(Command);

            ApprenticeshipEventsList.Verify(x => x.Add(Commitment, Commitment.Apprenticeships[0], "APPRENTICESHIP-AGREEMENT-UPDATED", new DateTime(apprenticeship.StartDate.Value.Year, apprenticeship.StartDate.Value.Month, 1), null), Times.Once);
            ApprenticeshipEventsPublisher.Verify(x => x.Publish(ApprenticeshipEventsList.Object), Times.Once);
        }

        [Test]
        public async Task ThenIfTheEmployerHasAlreadyApprovedTheCommitmentAndAnApprenticeDoesNotHaveAPreviousApprenticeshipThenAnEventIsPublishedWithTheStartDateAsTheFirstDayOfTheMonthTheApprentieceshipStarted()
        {
            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.EmployerAgreed);

            await Target.Handle(Command);

            ApprenticeshipEventsList.Verify(x => x.Add(Commitment, Commitment.Apprenticeships[0], "APPRENTICESHIP-AGREEMENT-UPDATED", new DateTime(Commitment.Apprenticeships[0].StartDate.Value.Year, Commitment.Apprenticeships[0].StartDate.Value.Month, 1), null), Times.Once);
            ApprenticeshipEventsPublisher.Verify(x => x.Publish(ApprenticeshipEventsList.Object), Times.Once);
        }

        [Test]
        public async Task ThenIfTheEmployerHasAlreadyApprovedThePaymentOrderIsUpdated()
        {
            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.EmployerAgreed);

            await Target.Handle(Command);

            Mediator.Verify(x => x.SendAsync(It.Is<SetPaymentOrderCommand>(c => c.AccountId == Commitment.EmployerAccountId)));
        }

        [Test]
        public async Task ThenAMessageIsCreatedForTheEmployer()
        {
            await Target.Handle(Command);

            Assert.AreEqual(Command.LastUpdatedByName, Commitment.Messages.Last().Author);
            Assert.AreEqual(Command.Message, Commitment.Messages.Last().Text);
            Assert.AreEqual(CallerType.Provider, Commitment.Messages.Last().CreatedBy);
            CommitmentRepository.Verify(x => x.SaveMessage(Commitment.Id, Commitment.Messages.Last()));
        }

        [Test]
        public async Task ThenTheProviderApprovedCohortNotificationIsSent()
        {
            await Target.Handle(Command);
            NotificationsPublisher.Verify(x => x.ProviderApprovedCohort(Commitment));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.CohortApproval.ProiderApproveCohort;
using SFA.DAS.Commitments.Application.Commands.SetPaymentOrder;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CohortApproval.ProviderApproveCohort
{
    [TestFixture]
    public class WhenAProviderApprovesACohortWhichHasATransferSender : ApproveCohortTestBase<ProviderApproveCohortCommand>
    {
        private long _transferRequestId = 999;

        [SetUp]
        public void SetUp()
        {
            Validator = new ProviderApproveCohortCommandValidator();
            Command = new ProviderApproveCohortCommand { Caller = new Caller(213, CallerType.Provider), CommitmentId = 123, LastUpdatedByName = "Test", LastUpdatedByEmail = "test@email.com", Message = "Some text" };
            SetUpCommonMocks();

            Commitment = CreateCommitment(Command.CommitmentId, 11234, Command.Caller.Id, 1000, "Nice Company");
            Commitment.EditStatus = EditStatus.ProviderOnly;
            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.EmployerAgreed);
            CommitmentRepository.Setup(x => x.StartTransferRequestApproval(It.IsAny<long>(), It.IsAny<decimal>(),
                It.IsAny<List<TrainingCourseSummary>>())).ReturnsAsync(_transferRequestId);

            CommitmentRepository.Setup(x => x.GetCommitmentById(Command.CommitmentId)).ReturnsAsync(Commitment);
            SetupSuccessfulOverlapCheck();

            Target = new ProviderApproveCohortCommandHandler(Validator, CommitmentRepository.Object, ApprenticeshipRepository.Object, OverlapRules.Object, CurrentDateTime.Object, HistoryRepository.Object, ApprenticeshipEventsList.Object, ApprenticeshipEventsPublisher.Object, Mediator.Object, MessagePublisher.Object, Mock.Of<ICommitmentsLogger>());
        }

        [Test]
        public async Task ThenIfTheEmployerHasAlreadyApprovedAMessageIsPublishedToTransferSender()
        {

            await Target.Handle(Command);

            MessagePublisher.Verify(x => x.PublishAsync(It.Is<CohortApprovalByTransferSenderRequested>(y =>
                y.TransferRequestId == _transferRequestId &&
                y.ReceivingEmployerAccountId == Commitment.EmployerAccountId &&
                y.CommitmentId == Commitment.Id && y.SendingEmployerAccountId == Commitment.TransferSenderId &&
                y.TransferCost == Commitment.Apprenticeships.Sum(a => a.Cost ?? 0))), Times.Once);
        }

        [Test]
        public async Task ThenIfTheEmployerHasAlreadyApprovedNoSetPaymentOrderCommandIsSent()
        {

            await Target.Handle(Command);

            Mediator.Verify(x => x.SendAsync(It.IsAny<SetPaymentOrderCommand>()), Times.Never);
        }

        [Test]
        public async Task ThenIfTheEmployerHasAlreadyApprovedNoPriceHistoryIsCreated()
        {
            await Target.Handle(Command);

            ApprenticeshipRepository.Verify(x => x.CreatePriceHistoryForApprenticeshipsInCommitment(It.IsAny<long>()), Times.Never);
        }

        [Test]
        public async Task ThenIfTheEmployerHasAlreadyApprovedDoNotSetAStartDateForTheApprenticeshipEventsList()
        {

            await Target.Handle(Command);

            ApprenticeshipEventsList.Verify(x => x.Add(Commitment, Commitment.Apprenticeships[0], "APPRENTICESHIP-AGREEMENT-UPDATED", null, null), Times.Once);
            ApprenticeshipEventsPublisher.Verify(x => x.Publish(ApprenticeshipEventsList.Object), Times.Once);
        }


        [Test]
        public async Task ThenEnsureTheStartATransferRequestInRepositoryIsCalled()
        {
            var expectedTotal = (decimal)Commitment.Apprenticeships.Sum(i => i.Cost);

            await Target.Handle(Command);

            CommitmentRepository.Verify(x => x.StartTransferRequestApproval(Commitment.Id,
                expectedTotal, It.Is<List<TrainingCourseSummary>>(p =>
                    p.Count == 1 && p[0].ApprenticeshipCount == 2 &&
                    p[0].CourseTitle == Commitment.Apprenticeships[0].TrainingName)));

        }

        [Test]
        public async Task ThenIfTheEmployerHasAlreadyApprovedThenEventsEmittedShouldIndicatePendingTransferApproval()
        {
            await Target.Handle(Command);

            ApprenticeshipEventsList.Verify(x => x.Add(
                It.Is<Commitment>(c => c.TransferApprovalStatus == TransferApprovalStatus.Pending),
                It.IsAny<Apprenticeship>(),
                It.IsAny<string>(),
                null,
                null
                ), Times.Exactly(Commitment.Apprenticeships.Count));
        }
    }
}

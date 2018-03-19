using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.CohortApproval.ProiderApproveCohort;
using SFA.DAS.Commitments.Application.Commands.SetPaymentOrder;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Events;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CohortApproval.ProviderApproveCohort
{
    [TestFixture]
    public class WhenAProviderApprovesACohortWhichHasATransferSender : ApproveCohortTestBase<ProviderApproveCohortCommand>
    {
        [SetUp]
        public void SetUp()
        {
            Validator = new ProviderApproveCohortCommandValidator();
            Command = new ProviderApproveCohortCommand { Caller = new Caller(213, CallerType.Provider), CommitmentId = 123, LastUpdatedByName = "Test", LastUpdatedByEmail = "test@email.com", Message = "Some text" };
            SetUpCommonMocks();

            Commitment = CreateCommitment(Command.CommitmentId, 11234, Command.Caller.Id, 1000, "Nice Company");
            Commitment.EditStatus = EditStatus.ProviderOnly;
            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.EmployerAgreed);

            CommitmentRepository.Setup(x => x.GetCommitmentById(Command.CommitmentId)).ReturnsAsync(Commitment);
            SetupSuccessfulOverlapCheck();
            
            Target = new ProviderApproveCohortCommandHandler(Validator, CommitmentRepository.Object, ApprenticeshipRepository.Object, OverlapRules.Object, CurrentDateTime.Object, HistoryRepository.Object, ApprenticeshipEventsList.Object, ApprenticeshipEventsPublisher.Object, Mediator.Object, MessagePublisher.Object);
        }

        [Test]
        public async Task ThenIfTheEmployerHasAlreadyApprovedAMessageIsPublishedToTransferSender()
        {

            await Target.Handle(Command);

            MessagePublisher.Verify(x => x.PublishAsync(It.Is<CohortApprovalByTransferSenderRequested>(y =>
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

    }
}

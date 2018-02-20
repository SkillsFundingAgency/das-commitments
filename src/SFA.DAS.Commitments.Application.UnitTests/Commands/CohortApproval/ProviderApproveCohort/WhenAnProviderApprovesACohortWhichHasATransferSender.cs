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
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
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

            CommitmentRepository.Setup(x => x.GetCommitmentById(Command.CommitmentId)).ReturnsAsync(Commitment);
            SetupSuccessfulOverlapCheck();
            
            Target = new ProviderApproveCohortCommandHandler(Validator, CommitmentRepository.Object, ApprenticeshipRepository.Object, OverlapRules.Object, CurrentDateTime.Object, HistoryRepository.Object, ApprenticeshipEventsList.Object, ApprenticeshipEventsPublisher.Object, Mediator.Object, MessagePublisher.Object);
        }

        [Test]
        public async Task ThenIfTheEmployerHasAlreadyApprovedAnApprovalMessageIsPublishedToTransferSender()
        {
            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.EmployerAgreed);

            await Target.Handle(Command);

            MessagePublisher.Verify(x => x.PublishAsync(It.Is<CommitmentRequiresApprovalByTransferSender>(y =>
                y.ProviderId == Commitment.ProviderId && y.AccountId == Commitment.EmployerAccountId &&
                y.CommitmentId == Commitment.Id && y.TransferSenderId == Commitment.TransferSenderId)), Times.Once);
        }

    }
}

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
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CohortApproval.EmployerApproveCohort
{
    [TestFixture]
    public class WhenAnEmployerApprovesACohortWhichHasATransferSender : ApproveCohortTestBase<EmployerApproveCohortCommand>
    {
        private Mock<IMessagePublisher> _messagePublisher;
        
        [SetUp]
        public void SetUp()
        {
            Validator = new EmployerApproveCohortCommandValidator();
            Command = new EmployerApproveCohortCommand { Caller = new Caller(213, CallerType.Employer), CommitmentId = 123, LastUpdatedByName = "Test", LastUpdatedByEmail = "test@email.com", Message = "Some text" };
            SetUpCommonMocks();
            Commitment = CreateCommitment(Command.CommitmentId, Command.Caller.Id, 234587, 1000, "Nice Company");
            CommitmentRepository.Setup(x => x.GetCommitmentById(Command.CommitmentId)).ReturnsAsync(Commitment);
            SetupSuccessfulOverlapCheck();
            
            _messagePublisher = new Mock<IMessagePublisher>();

            Target = new EmployerApproveCohortCommandHandler(Validator, CommitmentRepository.Object, ApprenticeshipRepository.Object, OverlapRules.Object, CurrentDateTime.Object, HistoryRepository.Object, ApprenticeshipEventsList.Object, ApprenticeshipEventsPublisher.Object, Mediator.Object, _messagePublisher.Object);
        }

        [Test]
        public async Task ThenIfTheProviderHasAlreadyApproved2ApprovalMessagesArePublished()
        {
            Commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.ProviderAgreed);

            await Target.Handle(Command);

            _messagePublisher.Verify(x=>x.PublishAsync(It.IsAny<CohortApprovedByEmployer>()), Times.Once);
            _messagePublisher.Verify(x => x.PublishAsync(It.Is<CommitmentRequiresApprovalByTransferSender>(y =>
                y.ProviderId == Commitment.ProviderId && y.AccountId == Commitment.EmployerAccountId &&
                y.CommitmentId == Commitment.Id && y.TransferSenderId == Commitment.TransferSenderId)), Times.Once);
        }
    }
}

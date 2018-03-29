using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Application.Commands.RejectTransferRequest;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.RejectTransferRequest
{
    [TestFixture]
    public class WhenRejectingATransferRequest
    {
        private AbstractValidator<RejectTransferRequestCommand> _validator;
        private RejectTransferRequestCommand _command;
        private Commitment _commitment;
        private Mock<ICommitmentRepository> _commitmentRepository;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private Mock<IApprenticeshipOverlapRules> _overlapRules;
        private Mock<ICurrentDateTime> _currentDateTime;
        private Mock<IApprenticeshipEventsList> _apprenticeshipEventsList;
        private Mock<IApprenticeshipEventsPublisher> _apprenticeshipEventsPublisher;
        private Mock<IMediator> _mediator;
        private Mock<IMessagePublisher> _messagePublisher;
        private Mock<IHistoryRepository> _historyRepository;
        private RejectTransferRequestCommandHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _validator = new RejectTransferRequestValidator();
            _commitmentRepository = new Mock<ICommitmentRepository>();
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _overlapRules = new Mock<IApprenticeshipOverlapRules>();
            _currentDateTime = new Mock<ICurrentDateTime>();
            _apprenticeshipEventsList = new Mock<IApprenticeshipEventsList>();
            _apprenticeshipEventsPublisher = new Mock<IApprenticeshipEventsPublisher>();
            _mediator = new Mock<IMediator>();
            _messagePublisher = new Mock<IMessagePublisher>();
            _historyRepository = new Mock<IHistoryRepository>();

            var fixture = new Fixture();
            _command = fixture.Build<RejectTransferRequestCommand>().Create();
            _commitment = fixture.Build<Commitment>()
                .With(x => x.TransferSenderId, _command.TransferSenderId)
                .With(x => x.EmployerAccountId, _command.TransferReceiverId)
                .With(x => x.TransferApprovalStatus, TransferApprovalStatus.Pending)
                .With(x => x.EditStatus, EditStatus.Both).Create();

            _commitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(_commitment);
            _commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.ProviderAgreed);

            _sut = new RejectTransferRequestCommandHandler(_validator, _commitmentRepository.Object,
                _apprenticeshipRepository.Object, _overlapRules.Object, _currentDateTime.Object,
                _apprenticeshipEventsList.Object, _apprenticeshipEventsPublisher.Object, _mediator.Object,
                _messagePublisher.Object, _historyRepository.Object);
        }

        [Test]
        public async Task ThenIfTheTransferSenderRejectsCohortEnsureRespositoryIsCalled()
        {
            await _sut.Handle(_command);

            _commitmentRepository.Verify(x => x.SetTransferApproval(_command.CommitmentId, TransferApprovalStatus.TransferRejected, _command.UserEmail, _command.UserName));
        }

        [Test]
        public async Task ThenIfTheTransferSenderRejectsCohortEnsureMessagePublisherSendsRejectedMessageAndNotApprovedMessage()
        {
            await _sut.Handle(_command);

            _messagePublisher.Verify(x => x.PublishAsync(It.Is<CohortRejectedByTransferSender>(p =>
                p.UserName == _command.UserName && p.UserEmail == _command.UserEmail &&
                p.CommitmentId == _command.CommitmentId &&
                p.ReceivingEmployerAccountId ==
                _command.TransferReceiverId &&
                p.SendingEmployerAccountId ==
                _command.TransferSenderId)));

            _messagePublisher.Verify(x => x.PublishAsync(It.IsAny<CohortApprovedByTransferSender>()), Times.Never);
        }

        [Test]
        public async Task ThenThrowExceptionIfCommitmentTransferSenderDoesntMatchCommandValue()
        {
            _commitment.TransferSenderId = 988;
            Assert.ThrowsAsync<UnauthorizedException>(() => _sut.Handle(_command));
        }

        [Test]
        public async Task ThenThrowExceptionIfCommitmentEmployerAccountIdNotMatchingTransferReceiverId()
        {
            _commitment.EmployerAccountId = 19989809;
            Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Handle(_command));
        }

        [Test]
        public async Task ThenThrowExceptionIfCommitmentStatusIsDeleted()
        {
            _commitment.CommitmentStatus = CommitmentStatus.Deleted;
            Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Handle(_command));
        }

        [Test]
        public async Task ThenThrowExceptionIfTransferApprovalStatusIsNotPending()
        {
            _commitment.TransferApprovalStatus = TransferApprovalStatus.TransferApproved;
            Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Handle(_command));
        }

        [TestCase(EditStatus.EmployerOnly)]
        [TestCase(EditStatus.ProviderOnly)]
        [TestCase(EditStatus.Neither)]
        public async Task ThenThrowExceptionIfEditStatusIsNotSetToNeither(EditStatus status)
        {
            _commitment.EditStatus = status;
            Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Handle(_command));
        }
    }
}

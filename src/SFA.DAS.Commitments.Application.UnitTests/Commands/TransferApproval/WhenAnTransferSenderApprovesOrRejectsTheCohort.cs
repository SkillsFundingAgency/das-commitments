using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Application.Commands.SetPaymentOrder;
using SFA.DAS.Commitments.Application.Commands.TransferApproval;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.TransferApproval
{
    [TestFixture]
    public class WhenAnTransferSenderApprovesOrRejectsTheCohort 
    {
        private AbstractValidator<TransferApprovalCommand> _validator;
        private TransferApprovalCommand _command;
        private Commitment _commitment;
        private Mock<ICommitmentRepository> _commitmentRepository;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private Mock<IApprenticeshipOverlapRules> _overlapRules;
        private Mock<ICurrentDateTime> _currentDateTime;
        private Mock<IApprenticeshipEventsList> _apprenticeshipEventsList;
        private Mock<IApprenticeshipEventsPublisher> _apprenticeshipEventsPublisher;
        private Mock<IMediator> _mediator;
        private Mock<IMessagePublisher> _messagePublisher;
        private TransferApprovalCommandHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _validator = new TransferApprovalValidator();
            _commitmentRepository = new Mock<ICommitmentRepository>();
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _overlapRules = new Mock<IApprenticeshipOverlapRules>();
            _currentDateTime = new Mock<ICurrentDateTime>();
            _apprenticeshipEventsList = new Mock<IApprenticeshipEventsList>();
            _apprenticeshipEventsPublisher = new Mock<IApprenticeshipEventsPublisher>();
            _mediator = new Mock<IMediator>();
            _messagePublisher = new Mock<IMessagePublisher>();

            var fixture = new Fixture();
            _command = fixture.Build<TransferApprovalCommand>()
                .With(x => x.TransferStatus, TransferApprovalStatus.TransferRejected).Create();
            _commitment = fixture.Build<Commitment>()
                .With(x => x.TransferSenderId, _command.TransferSenderId)
                .With(x => x.EmployerAccountId, _command.TransferReceiverId)
                .With(x => x.TransferApprovalStatus, TransferApprovalStatus.Pending)
                .With(x => x.EditStatus, EditStatus.Both).Create();

            _commitmentRepository.Setup(x=>x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(_commitment);
            _commitment.Apprenticeships.ForEach(x => x.AgreementStatus = AgreementStatus.ProviderAgreed);

            _sut = new TransferApprovalCommandHandler(_validator, _commitmentRepository.Object,
                _apprenticeshipRepository.Object, _overlapRules.Object, _currentDateTime.Object,
                _apprenticeshipEventsList.Object, _apprenticeshipEventsPublisher.Object, _mediator.Object,
                _messagePublisher.Object);
        }

        [Test]
        public async Task ThenIfTheTransferSenderRejectsCohortEnsureRespositoryIsCalled()
        {
            await _sut.Handle(_command);

            _commitmentRepository.Verify(x=>x.SetTransferApproval(_command.CommitmentId, _command.TransferStatus, _command.UserEmail, _command.UserName));
        }

        [Test]
        public async Task ThenIfTheTransferSenderApprovesCohortEnsureApprenticeshipUpdatesAreUpdatedInRepository()
        {
            _command.TransferStatus = TransferApprovalStatus.TransferApproved;
            await _sut.Handle(_command);

            _commitment.Apprenticeships.ForEach(
                x=> x.PaymentStatus.Should().Be(PaymentStatus.Active));
            _apprenticeshipRepository.Verify(x=>x.UpdateApprenticeshipStatuses(_commitment.Apprenticeships));
        }

        [Test]
        public async Task ThenIfTheTransferSenderApprovesCohortEnsurePriceHistoryIsUpdated()
        {
            _command.TransferStatus = TransferApprovalStatus.TransferApproved;
            await _sut.Handle(_command);
            _apprenticeshipRepository.Verify(x => x.CreatePriceHistoryForApprenticeshipsInCommitment(_commitment.Id));
        }

        [Test]
        public async Task ThenIfTheTransferSenderApprovesCohortEnsureEventsPublisherIsCalledForApprovedEvents()
        {
            _command.TransferStatus = TransferApprovalStatus.TransferApproved;
            await _sut.Handle(_command);
            _apprenticeshipEventsPublisher.Verify(x => x.Publish(It.IsAny<IApprenticeshipEventsList>()));
        }

        [Test]
        public async Task ThenIfTheTransferSenderApprovesCohortEnsureReorderPayementsCommandIsCalled()
        {
            _command.TransferStatus = TransferApprovalStatus.TransferApproved;
            await _sut.Handle(_command);
            _mediator.Verify(x => x.SendAsync(It.Is<SetPaymentOrderCommand>(p => p.AccountId == _commitment.EmployerAccountId)));
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
            _commitment.EmployerAccountId = 98;
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
            _commitment.TransferApprovalStatus =  TransferApprovalStatus.TransferApproved;
            Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Handle(_command));
        }

        [Test]
        public async Task ThenThrowExceptionIfEditStatusIsNotSetToNeither()
        {
            _commitment.EditStatus = EditStatus.EmployerOnly;
            Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Handle(_command));
        }

    }
}

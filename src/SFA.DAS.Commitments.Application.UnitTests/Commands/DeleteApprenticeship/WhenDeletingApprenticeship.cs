using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.DeleteApprenticeship;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.DeleteApprenticeship
{
    [TestFixture]
    public sealed class WhenDeletingApprenticeship
    {
        private Mock<ICommitmentRepository> _mockCommitmentRepository;
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRepository;
        private Mock<IApprenticeshipEvents> _mockApprenticeshipEvents;
        private Mock<IHistoryRepository> _mockHistoryRepository;
        private Mock<IV2EventsPublisher> _mockV2EventsPublisher;
        private AbstractValidator<DeleteApprenticeshipCommand> _validator;
        private DeleteApprenticeshipCommandHandler _handler;
        private DeleteApprenticeshipCommand _validCommand;
        private Apprenticeship _apprenticeship;
        IEnumerable<HistoryItem> _historyResult;

        [SetUp]
        public void Setup()
        {
            _mockCommitmentRepository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _mockApprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _mockHistoryRepository = new Mock<IHistoryRepository>();
            _mockV2EventsPublisher = new Mock<IV2EventsPublisher>();

            _mockHistoryRepository.Setup(x => x.InsertHistory(It.IsAny<IEnumerable<HistoryItem>>()))
                .Callback((object o) => { _historyResult = o as IEnumerable<HistoryItem>; })
                .Returns(() => Task.CompletedTask);

            _mockV2EventsPublisher.Setup(x => x.PublishApprenticeshipDeleted(It.IsAny<Commitment>(), It.IsAny<Apprenticeship>()))
                .Returns(() => Task.CompletedTask);

            _validator = new DeleteApprenticeshipValidator();
            _handler = new DeleteApprenticeshipCommandHandler(_mockCommitmentRepository.Object,
                _mockApprenticeshipRepository.Object, _validator, Mock.Of<ICommitmentsLogger>(),
                _mockApprenticeshipEvents.Object, _mockHistoryRepository.Object, _mockV2EventsPublisher.Object);

            _validCommand = new DeleteApprenticeshipCommand { ApprenticeshipId = 2, Caller = new Domain.Caller { Id = 123, CallerType = Domain.CallerType.Provider }, UserName = "Bob", UserId = "User" };

            _apprenticeship = new Apprenticeship { PaymentStatus = PaymentStatus.PendingApproval, ProviderId = 123, EmployerAccountId = 123 };
            _mockApprenticeshipRepository.Setup(r => r.GetApprenticeship(It.IsAny<long>())).ReturnsAsync(_apprenticeship);
        }

        [TearDown]
        public void TearDown()
        {
            _historyResult = null;
        }

        [Test]
        public void ShouldNotAllowDeleteIfApprenticesAreNotPreApprovedState()
        {
            var testCommitment = new Commitment
            {
                ProviderId = 123,
                Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship { PaymentStatus = PaymentStatus.PendingApproval },
                    new Apprenticeship { PaymentStatus = PaymentStatus.PendingApproval }
                }
            };

            _apprenticeship.PaymentStatus = PaymentStatus.Active;

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            Func<Task> act = async () => await _handler.Handle(_validCommand);

            act.ShouldThrow<UnauthorizedException>().And.Message.Should().Contain("cannot be deleted when payment status is");
        }

        [Test]
        public void ShouldNotAllowDeleteIfProviderIsNotAssociatedToCommitments()
        {
            var testCommitment = new Commitment
            {
                ProviderId = 123,
                Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship { PaymentStatus = PaymentStatus.PendingApproval },
                    new Apprenticeship { PaymentStatus = PaymentStatus.PendingApproval }
                }
            };

            _apprenticeship.PaymentStatus = PaymentStatus.Active;

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            _apprenticeship.ProviderId = 555;
            
            Func<Task> act = async () => await _handler.Handle(_validCommand);

            act.ShouldThrow<UnauthorizedException>().And.Message.Should().Contain("Provider 123 not authorised");
        }

        [Test]
        public void ShouldNotAllowDeleteIfEmployerIsNotAssociatedToCommitments()
        {
            var testCommitment = new Commitment
            {
                ProviderId = 123,
                Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship { PaymentStatus = PaymentStatus.PendingApproval },
                    new Apprenticeship { PaymentStatus = PaymentStatus.PendingApproval }
                }
            };

            _apprenticeship.PaymentStatus = PaymentStatus.Active;

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            _apprenticeship.EmployerAccountId = 555;

            _validCommand.Caller.CallerType = Domain.CallerType.Employer;

            Func<Task> act = async () => await _handler.Handle(_validCommand);

            act.ShouldThrow<UnauthorizedException>().And.Message.Should().Contain("Employer 123 not authorised");
        }

        [Test]
        public void ShouldNotAllowProviderToDeleteIfCommitmentIsWithEmployer()
        {
            var testCommitment = new Commitment
            {
                ProviderId = 123,
                EditStatus = EditStatus.EmployerOnly
            };

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            Func<Task> act = async () => await _handler.Handle(_validCommand);

            act.ShouldThrow<UnauthorizedException>().And.Message.Should().Contain("Provider 123 not allowed");
        }

        [Test]
        public void ShouldNotAllowEmployerToDeleteIfCommitmentIsWithProvider()
        {
            var testCommitment = new Commitment
            {
                EmployerAccountId = 123,
                EditStatus = EditStatus.ProviderOnly
            };

            _validCommand.Caller.CallerType = Domain.CallerType.Employer;

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            Func<Task> act = async () => await _handler.Handle(_validCommand);

            act.ShouldThrow<UnauthorizedException>().And.Message.Should().Contain("Employer 123 not allowed");
        }

        [Test]
        public async Task ShouldPublishApprenticeshipDeletedEvents()
        {
            var testCommitment = new Commitment
            {
                ProviderId = 123,
            };

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            await _handler.Handle(_validCommand);

            _mockApprenticeshipEvents.Verify(x => x.PublishDeletionEvent(testCommitment, _apprenticeship, "APPRENTICESHIP-DELETED"), Times.Once);
        }

        [Test]
        public async Task ShouldCreateAHistoryRecord()
        {
            var testCommitment = new Commitment
            {
                ProviderId = 123,
            };
            var expectedOriginalState = JsonConvert.SerializeObject(testCommitment);

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            await _handler.Handle(_validCommand);

            Assert.AreEqual(1, _historyResult.Count());

            Assert.AreEqual(1, _historyResult.Count(item =>
                item.ChangeType == CommitmentChangeType.DeletedApprenticeship.ToString() &&
                item.CommitmentId == testCommitment.Id &&
                item.ApprenticeshipId == null &&
                item.OriginalState == expectedOriginalState &&
                item.UpdatedByRole == _validCommand.Caller.CallerType.ToString() &&
                item.UpdatedState == expectedOriginalState &&
                item.UserId == _validCommand.UserId &&
                item.ProviderId == testCommitment.ProviderId &&
                item.EmployerAccountId == testCommitment.EmployerAccountId &&
                item.UpdatedByName == _validCommand.UserName
            ));
            
        }


        [Test]
        public async Task ThenCohortTransferStatusIsResetIfRejected()
        {
            //Arrange
            var testCommitment = new Commitment
            {
                ProviderId = 123,
                TransferApprovalStatus = TransferApprovalStatus.TransferRejected
            };

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            //Act
            await _handler.Handle(_validCommand);

            //Assert
            _mockCommitmentRepository.Verify(x => x.UpdateCommitment(It.Is<Commitment>(c =>
                c.TransferApprovalStatus == null
                && c.TransferApprovalActionedOn == null
            )), Times.Once);
        }

        [Test]
        public async Task ThenCohortIsUpdated()
        {
            //Arrange
            var testCommitment = new Commitment
            {
                ProviderId = 123
            };

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            //Act
            await _handler.Handle(_validCommand);

            //Assert
            _mockCommitmentRepository.Verify(x => x.UpdateCommitment(It.IsAny<Commitment>()), Times.Once);
        }

        [Test]
        public async Task ThenCohortTransferStatusIsResetIfPreviouslyRejected()
        {
            //Arrange
            var testCommitment = new Commitment
            {
                ProviderId = 123,
                TransferApprovalStatus = TransferApprovalStatus.TransferRejected
            };

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            //Act
            await _handler.Handle(_validCommand);

            //Assert
            _mockCommitmentRepository.Verify(x =>
                x.UpdateCommitment(It.Is<Commitment>(c => c.TransferApprovalStatus == null)), Times.Once);
        }

        [Test]
        public async Task ThenApprenticeshipDeletedEventIsPublished()
        {
            //Arrange
            var testCommitment = new Commitment
            {
                ProviderId = 123,
                TransferApprovalStatus = TransferApprovalStatus.TransferApproved
            };

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            //Act
            await _handler.Handle(_validCommand);

            //Assert
            _mockV2EventsPublisher.Verify(x => x.PublishApprenticeshipDeleted(testCommitment, _apprenticeship));
        }
    }
}

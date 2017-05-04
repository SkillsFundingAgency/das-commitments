using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.DeleteCommitment;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.DeleteCommitment
{
    [TestFixture]
    public sealed class WhenDeletingCommitment
    {
        private Mock<ICommitmentRepository> _mockCommitmentRepository;
        private Mock<IApprenticeshipEvents> _mockApprenticeshipEvents;
        private Mock<IHistoryRepository> _mockHistoryRepository;
        private AbstractValidator<DeleteCommitmentCommand> _validator;
        private DeleteCommitmentCommandHandler _handler;
        private DeleteCommitmentCommand _validCommand;

        [SetUp]
        public void Setup()
        {
            _mockCommitmentRepository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _mockHistoryRepository = new Mock<IHistoryRepository>();
            _validator = new DeleteCommitmentValidator();
            _handler = new DeleteCommitmentCommandHandler(_mockCommitmentRepository.Object, _validator, Mock.Of<ICommitmentsLogger>(), _mockApprenticeshipEvents.Object, _mockHistoryRepository.Object);

            _validCommand = new DeleteCommitmentCommand { CommitmentId = 2, Caller = new Domain.Caller { Id = 123, CallerType = Domain.CallerType.Provider }, UserId = "User", UserName = "Bob"};
        }

        [Test]
        public void ShouldNotAllowDeleteIfApprenticesAreNotPreApprovedState()
        {
            var testCommitment = new Commitment
            {
                ProviderId = 123,
                Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship { PaymentStatus = PaymentStatus.Active },
                    new Apprenticeship { PaymentStatus = PaymentStatus.PendingApproval },
                    new Apprenticeship { PaymentStatus = PaymentStatus.PendingApproval }
                }
            };

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            Func<Task> act = async () => await _handler.Handle(_validCommand);

            act.ShouldThrow<UnauthorizedException>().And.Message.Should().Contain("Commitment cannot be deleted");
        }

        [Test]
        public void ShouldNotAllowDeleteIfProviderIsNotAssociatedToCommitments()
        {
            var testCommitment = new Commitment
            {
                ProviderId = 555
            };

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            Func<Task> act = async () => await _handler.Handle(_validCommand);

            act.ShouldThrow<UnauthorizedException>().And.Message.Should().Contain("Provider 123 unauthorized");
        }

        [Test]
        public void ShouldNotAllowDeleteIfEmployerIsNotAssociatedToCommitments()
        {
            var testCommitment = new Commitment
            {
                EmployerAccountId = 555
            };

            _validCommand.Caller.CallerType = Domain.CallerType.Employer;

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            Func<Task> act = async () => await _handler.Handle(_validCommand);

            act.ShouldThrow<UnauthorizedException>().And.Message.Should().Contain("Employer 123 unauthorized");
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

            act.ShouldThrow<UnauthorizedException>().And.Message.Should().Contain("Provider 123 unauthorized");
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

            act.ShouldThrow<UnauthorizedException>().And.Message.Should().Contain("Employer 123 unauthorized");
        }

        [Test]
        public void ShouldNotAllowDeleteIfCommitmentBeenAgreedByBothParties()
        {
            var testCommitment = new Commitment
            {
                EmployerAccountId = 123,
                EditStatus = EditStatus.Both
            };

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            Func<Task> act = async () => await _handler.Handle(_validCommand);

            act.ShouldThrow<UnauthorizedException>().And.Message.Should().Contain("Provider 123 unauthorized");
        }

        [Test]
        public async Task ShouldPublishApprenticeshipDeletedEvents()
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

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            await _handler.Handle(_validCommand);

            _mockApprenticeshipEvents.Verify(x => x.BulkPublishDeletionEvent(testCommitment, testCommitment.Apprenticeships, "APPRENTICESHIP-DELETED"), Times.Once);
        }

        [Test]
        public async Task ThenAHistoryRecordIsCreated()
        {
            var testCommitment = new Commitment
            {
                ProviderId = 123,
                Id = 43857,
                Apprenticeships = new List<Apprenticeship>()
            };
            var expectedOriginalState = JsonConvert.SerializeObject(testCommitment);

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            await _handler.Handle(_validCommand);

            _mockHistoryRepository.Verify(
                x =>
                    x.InsertHistory(
                        It.Is<IEnumerable<HistoryItem>>(
                            y =>
                                y.First().EntityId == testCommitment.Id &&
                                y.First().ChangeType == CommitmentChangeType.Deleted.ToString() &&
                                y.First().EntityType == "Commitment" &&
                                y.First().OriginalState == expectedOriginalState &&
                                y.First().UpdatedByRole == _validCommand.Caller.CallerType.ToString() &&
                                y.First().UpdatedState == null &&
                                y.First().UserId == _validCommand.UserId &&
                                y.First().UpdatedByName == _validCommand.UserName)), Times.Once);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.DeleteApprenticeship;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.DeleteApprenticeship
{
    [TestFixture]
    public sealed class WhenDeletingApprenticeship
    {
        private Mock<ICommitmentRepository> _mockCommitmentRepository;
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRepository;
        private Mock<IApprenticeshipEvents> _mockApprenticeshipEvents;
        private Mock<IHistoryRepository> _mockHistoryRepository;
        private AbstractValidator<DeleteApprenticeshipCommand> _validator;
        private DeleteApprenticeshipCommandHandler _handler;
        private DeleteApprenticeshipCommand _validCommand;
        private Apprenticeship _apprenticeship;

        [SetUp]
        public void Setup()
        {
            _mockCommitmentRepository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _mockApprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _mockHistoryRepository = new Mock<IHistoryRepository>();
            _validator = new DeleteApprenticeshipValidator();
            _handler = new DeleteApprenticeshipCommandHandler(_mockCommitmentRepository.Object, _mockApprenticeshipRepository.Object, _validator, Mock.Of<ICommitmentsLogger>(), _mockApprenticeshipEvents.Object, _mockHistoryRepository.Object);

            _validCommand = new DeleteApprenticeshipCommand { ApprenticeshipId = 2, Caller = new Domain.Caller { Id = 123, CallerType = Domain.CallerType.Provider } };

            _apprenticeship = new Apprenticeship { PaymentStatus = PaymentStatus.PendingApproval, ProviderId = 123, EmployerAccountId = 123 };
            _mockApprenticeshipRepository.Setup(r => r.GetApprenticeship(It.IsAny<long>())).ReturnsAsync(_apprenticeship);
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
            _apprenticeship.ProviderId = 555;
            
            Func<Task> act = async () => await _handler.Handle(_validCommand);

            act.ShouldThrow<UnauthorizedException>().And.Message.Should().Contain("Provider 123 unauthorized");
        }

        [Test]
        public void ShouldNotAllowDeleteIfEmployerIsNotAssociatedToCommitments()
        {
            _apprenticeship.EmployerAccountId = 555;

            _validCommand.Caller.CallerType = Domain.CallerType.Employer;

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

            _mockHistoryRepository.Verify(
                x =>
                    x.InsertHistory(
                        It.Is<IEnumerable<HistoryItem>>(
                            y =>
                                y.First().EntityId == testCommitment.Id &&
                                y.First().ChangeType == CommitmentChangeType.DeletedApprenticeship.ToString() &&
                                y.First().EntityType == "Commitment" &&
                                y.First().OriginalState == expectedOriginalState &&
                                y.First().UpdatedByRole == _validCommand.Caller.CallerType.ToString() &&
                                y.First().UpdatedState == expectedOriginalState &&
                                y.First().UserId == _validCommand.UserId)), Times.Once);
        }
    }
}

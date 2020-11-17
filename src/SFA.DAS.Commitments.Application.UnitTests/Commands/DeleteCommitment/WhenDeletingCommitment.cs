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
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

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
        private Mock<IMessagePublisher> _mockMessagePublisher;
        private Mock<IV2EventsPublisher> _mockV2EventsPublisher;


        [SetUp]
        public void Setup()
        {
            _mockCommitmentRepository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _mockHistoryRepository = new Mock<IHistoryRepository>();
            _validator = new DeleteCommitmentValidator();
            _mockMessagePublisher = new Mock<IMessagePublisher>();
            _mockV2EventsPublisher = new Mock<IV2EventsPublisher>();

            _handler = new DeleteCommitmentCommandHandler(_mockCommitmentRepository.Object, _validator, Mock.Of<ICommitmentsLogger>(), _mockApprenticeshipEvents.Object, _mockHistoryRepository.Object, _mockMessagePublisher.Object, _mockV2EventsPublisher.Object);

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

            act.ShouldThrow<UnauthorizedException>().And.Message.Should().Contain("Provider 123 not authorised");
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
        public void ShouldNotAllowDeleteIfCommitmentBeenAgreedByBothParties()
        {
            var testCommitment = new Commitment
            {
                EmployerAccountId = 123,
                EditStatus = EditStatus.Both
            };

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            Func<Task> act = async () => await _handler.Handle(_validCommand);

            act.ShouldThrow<UnauthorizedException>().And.Message.Should().Contain("Provider 123 not authorised");
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
                                y.First().ChangeType == CommitmentChangeType.Deleted.ToString() &&
                                y.First().CommitmentId == testCommitment.Id &&
                                y.First().ApprenticeshipId == null &&
                                y.First().OriginalState == expectedOriginalState &&
                                y.First().UpdatedByRole == _validCommand.Caller.CallerType.ToString() &&
                                y.First().UpdatedState == null &&
                                y.First().UserId == _validCommand.UserId &&
                                y.First().ProviderId == testCommitment.ProviderId &&
                                y.First().EmployerAccountId == testCommitment.EmployerAccountId &&
                                y.First().UpdatedByName == _validCommand.UserName)), Times.Once);
        }

        [Test]
        public async Task ShouldPublishMessageIfProviderApprovedCohortDeletedByEmployer()
        {
            const long commitmentId = 12345;
            const long employerAccountId = 123;
            const long providerId = 456;

            var testCommitment = new Commitment
            {
                Id = commitmentId,
                EmployerAccountId = employerAccountId,
                ProviderId = providerId,
                Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship { AgreementStatus = AgreementStatus.ProviderAgreed },
                    new Apprenticeship { AgreementStatus = AgreementStatus.ProviderAgreed }
                }
            };

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(commitmentId)).ReturnsAsync(testCommitment);

            _validCommand.CommitmentId = commitmentId;
            _validCommand.Caller.CallerType = Domain.CallerType.Employer;
            _validCommand.Caller.Id = employerAccountId;

            await _handler.Handle(_validCommand);

            _mockMessagePublisher.Verify(x => x.PublishAsync(
                It.Is<ProviderCohortApprovalUndoneByEmployerUpdate>(p => p.AccountId == employerAccountId && p.ProviderId == providerId && p.CommitmentId == commitmentId)), Times.Once);
        }

        [Test]
        public async Task ShouldNotPublishMessageIfCohortDeletedByProvider()
        {
            const long commitmentId = 12345;
            const long employerAccountId = 123;
            const long providerId = 456;

            var testCommitment = new Commitment
            {
                Id = commitmentId,
                EmployerAccountId = employerAccountId,
                ProviderId = providerId,
                Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship { AgreementStatus = AgreementStatus.EmployerAgreed },
                    new Apprenticeship { AgreementStatus = AgreementStatus.EmployerAgreed }
                }
            };

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(commitmentId)).ReturnsAsync(testCommitment);

            _validCommand.CommitmentId = commitmentId;
            _validCommand.Caller.CallerType = Domain.CallerType.Provider;
            _validCommand.Caller.Id = providerId;

            await _handler.Handle(_validCommand);

            _mockMessagePublisher.Verify(x => x.PublishAsync(
                It.IsAny<ProviderCohortApprovalUndoneByEmployerUpdate>()), Times.Never);
        }

        [Test]
        public async Task ShouldNotPublishMessageIfCohortDeletedByEmployerAndNotAgreed()
        {
            const long commitmentId = 12345;
            const long employerAccountId = 123;
            const long providerId = 456;

            var testCommitment = new Commitment
            {
                Id = commitmentId,
                EmployerAccountId = employerAccountId,
                ProviderId = providerId,
                Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship { AgreementStatus = AgreementStatus.NotAgreed },
                    new Apprenticeship { AgreementStatus = AgreementStatus.NotAgreed }
                }
            };

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(commitmentId)).ReturnsAsync(testCommitment);

            _validCommand.CommitmentId = commitmentId;
            _validCommand.Caller.CallerType = Domain.CallerType.Employer;
            _validCommand.Caller.Id = employerAccountId;

            await _handler.Handle(_validCommand);

            _mockMessagePublisher.Verify(x => x.PublishAsync(
                It.IsAny<ProviderCohortApprovalUndoneByEmployerUpdate>()), Times.Never);
        }

        [Test]
        public async Task ShouldPublishV2EventForEachApprenticeship()
        {
            var testCommitment = new Commitment
            {
                ProviderId = 123,
                CommitmentStatus = CommitmentStatus.New,
                EditStatus = EditStatus.ProviderOnly,
                Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship { PaymentStatus = PaymentStatus.PendingApproval, AgreementStatus = AgreementStatus.NotAgreed},
                    new Apprenticeship { PaymentStatus = PaymentStatus.PendingApproval, AgreementStatus = AgreementStatus.NotAgreed },
                    new Apprenticeship { PaymentStatus = PaymentStatus.PendingApproval, AgreementStatus = AgreementStatus.NotAgreed }
                }
            };

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            await _handler.Handle(_validCommand);

            _mockV2EventsPublisher.Verify(ep => ep.PublishApprenticeshipDeleted(testCommitment, testCommitment.Apprenticeships[0]), Times.Once);
            _mockV2EventsPublisher.Verify(ep => ep.PublishApprenticeshipDeleted(testCommitment, testCommitment.Apprenticeships[1]), Times.Once);
            _mockV2EventsPublisher.Verify(ep => ep.PublishApprenticeshipDeleted(testCommitment, testCommitment.Apprenticeships[2]), Times.Once);
        }

        [Test]
        public async Task ShouldPublishMessageIfChangeOfProviderCohortIsDeletedByProvider()
        {
            var testCommitment = new Commitment
            {
                Id = 2,
                ProviderId = 123,
                CommitmentStatus = CommitmentStatus.New,
                EditStatus = EditStatus.ProviderOnly,
                Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship { PaymentStatus = PaymentStatus.PendingApproval, AgreementStatus = AgreementStatus.NotAgreed},
                },
                ChangeOfPartyRequestId = 100
            };

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            await _handler.Handle(_validCommand);

            _mockV2EventsPublisher.Verify(e => e.PublishProviderRejectedChangeOfProviderCohort(testCommitment), Times.Once );
        }

        [TestCase(100, CallerType.Employer)]
        [TestCase(null, CallerType.Provider)]
        public async Task ShouldNotPublishMessageChangeOfProviderCohortIsDeletedIfDeletedByEmployerOrIsNotCopRequest(long? changeOfProviderId, CallerType callerType)
        {
            var testCommitment = new Commitment
            {
                Id = 2,
                ProviderId = 123,
                EmployerAccountId = 123,
                CommitmentStatus = CommitmentStatus.New,
                EditStatus = EditStatus.Both,
                Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship { PaymentStatus = PaymentStatus.PendingApproval, AgreementStatus = AgreementStatus.NotAgreed},
                },
                ChangeOfPartyRequestId = changeOfProviderId
            };

            _validCommand.Caller.CallerType = callerType;

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            await _handler.Handle(_validCommand);

            _mockV2EventsPublisher.Verify(e => e.PublishProviderRejectedChangeOfProviderCohort(testCommitment), Times.Never);
        }
    }
}

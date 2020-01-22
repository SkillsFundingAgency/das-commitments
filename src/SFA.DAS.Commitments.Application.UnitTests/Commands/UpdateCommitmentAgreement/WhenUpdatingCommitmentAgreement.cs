using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateCommitmentAgreement
{
    [TestFixture]
    public sealed class WhenUpdatingCommitmentAgreement
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRespository;
        private UpdateCommitmentAgreementCommandHandler _handler;
        private UpdateCommitmentAgreementCommand _validCommand;
        private Mock<IApprenticeshipEventsList> _mockApprenticeshipEventsList;
        private Mock<IApprenticeshipEventsPublisher> _mockApprenticeshipEventsPublisher;
        private Mock<IHistoryRepository> _mockHistoryRepository;
        private Mock<IMessagePublisher> _messagePublisher;
        private Mock<INotificationsPublisher> _notificationsPublisher;

        [SetUp]
        public void Setup()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();
            _mockApprenticeshipRespository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<ApprenticeshipResult>());
            _mockApprenticeshipEventsList = new Mock<IApprenticeshipEventsList>();
            _mockApprenticeshipEventsPublisher = new Mock<IApprenticeshipEventsPublisher>();
            _mockHistoryRepository = new Mock<IHistoryRepository>();
            _messagePublisher = new Mock<IMessagePublisher>();
            _notificationsPublisher = new Mock<INotificationsPublisher>();

            _handler = new UpdateCommitmentAgreementCommandHandler(
                _mockCommitmentRespository.Object,
                _mockApprenticeshipRespository.Object,
                new ApprenticeshipUpdateRules(), 
                Mock.Of<ICommitmentsLogger>(),
                new UpdateCommitmentAgreementCommandValidator(),
                _mockApprenticeshipEventsList.Object,
                _mockApprenticeshipEventsPublisher.Object,
                _mockHistoryRepository.Object,
                _messagePublisher.Object,
                _notificationsPublisher.Object);

            _validCommand = new UpdateCommitmentAgreementCommand
            {
                Caller = new Domain.Caller { Id = 444, CallerType = Domain.CallerType.Employer },
                LatestAction = LastAction.Amend,
                CommitmentId = 123L,
                LastUpdatedByName = "Test Tester",
                LastUpdatedByEmail = "test@tester.com"
            };
        }

        [Test]
        public void ShouldThrowExceptionIfActionIsNotSetToValidValue()
        {
            _validCommand.LatestAction = (Domain.Entities.LastAction)99;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrowExactly<ValidationException>();
        }

        [Test]
        public void ShouldThrowExceptionIfCallerNotSet()
        {
            _validCommand.Caller = null;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrowExactly<ValidationException>();
        }

        [Test]
        public void ShouldThrowExceptionIfCallerIdNotSet()
        {
            _validCommand.Caller.Id = 0;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrowExactly<ValidationException>();
        }

        [Test]
        public void ShouldThrowExceptionIfCallerTypeNotValid()
        {
            _validCommand.Caller.CallerType = (CallerType)99;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrowExactly<ValidationException>();
        }

        [Test]
        public void ShouldThrowExceptionIfCommitmentIdIsInvalid()
        {
            _validCommand.CommitmentId = 0;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrowExactly<ValidationException>();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void ShouldThrowExceptionIfLastUpdatedByNameIsNotSet(string value)
        {
            _validCommand.LastUpdatedByName = value;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrowExactly<ValidationException>();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        [TestCase("ffdsfdfdsf")]
        [TestCase("#@%^%#$@#$@#.com")]
        [TestCase("@example.com")]
        [TestCase("Joe Smith <email @example.com>")]
        [TestCase("email.example.com")]
        [TestCase("email@example @example.com")]
        [TestCase(".email @example.com")]
        [TestCase("email.@example.com")]
        [TestCase("email..email @example.com")]
        [TestCase("email@example.com (Joe Smith)")]
        [TestCase("email @example")]
        [TestCase("email@-example.com")]
        //[TestCase("email@example.web")] -- This is being accepted by regex
        //[TestCase("email@111.222.333.44444")] -- This is being accepted by regex
        [TestCase("email @example..com")]
        [TestCase("Abc..123@example.com")]
        [TestCase("“(),:;<>[\\] @example.comjust\"not\"right @example.com")]
        [TestCase("this\\ is'really'not\\allowed @example.com")]
        public void ShouldThrowExceptionIfLastUpdatedByEmailIsNotValid(string value)
        {
            _validCommand.LastUpdatedByEmail = value;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrowExactly<ValidationException>();
        }

        [Test]
        public async Task ThenIfAnApprenticeshipAgreementStatusIsUpdatedTheApprenticeshipStatusesAreUpdated()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly, ProviderId = 325 };
            var apprenticeship = new Apprenticeship { AgreementStatus = AgreementStatus.ProviderAgreed, PaymentStatus = PaymentStatus.PendingApproval, Id = 1234 };
            commitment.Apprenticeships.Add(apprenticeship);

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.LatestAction = LastAction.Amend;

            await _handler.Handle(_validCommand);
            
            _mockApprenticeshipRespository.Verify(x => x.UpdateApprenticeshipStatuses(It.Is<List<Apprenticeship>>(y => y.First().AgreementStatus == AgreementStatus.NotAgreed)), Times.Once);
        }

        [Test]
        public async Task ThenIfAnApprenticeshipPaymentStatusIsUpdatedTheApprenticeshipStatusesAreUpdated()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly };
            var apprenticeship = new Apprenticeship { AgreementStatus = AgreementStatus.NotAgreed, PaymentStatus = PaymentStatus.Active, Id = 1234 };
            commitment.Apprenticeships.Add(apprenticeship);

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            var updatedApprenticeship = new Apprenticeship();
            _mockApprenticeshipRespository.Setup(x => x.GetApprenticeship(apprenticeship.Id)).ReturnsAsync(updatedApprenticeship);

            _validCommand.LatestAction = LastAction.Amend;

            await _handler.Handle(_validCommand);

            _mockApprenticeshipRespository.Verify(x => x.UpdateApprenticeshipStatuses(It.Is<List<Apprenticeship>>(y => y.First().PaymentStatus == PaymentStatus.PendingApproval)), Times.Once);
        }

        [Test]
        public async Task ThenIfAnApprenticeshipIsUpdatedWithoutBeingApprovedAnEventIsPublishedWithoutAnEffectiveFromDate()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly, ProviderId = 2345 };
            var apprenticeship = new Apprenticeship { AgreementStatus = AgreementStatus.ProviderAgreed, PaymentStatus = PaymentStatus.PendingApproval, Id = 1234 };
            commitment.Apprenticeships.Add(apprenticeship);

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.LatestAction = LastAction.Amend;

            await _handler.Handle(_validCommand);
            _mockApprenticeshipEventsList.Verify(x => x.Add(commitment, apprenticeship, "APPRENTICESHIP-AGREEMENT-UPDATED", null, null), Times.Once);
            _mockApprenticeshipEventsPublisher.Verify(x => x.Publish(_mockApprenticeshipEventsList.Object), Times.Once);
        }

        [Test]
        public async Task ThenIfNoMessageIsProvidedThenAnEmptyMessageIsSaved()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly, ProviderId = 1234 };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.Message = null;

            //Act
            await _handler.Handle(_validCommand);

            //Assert
            _mockCommitmentRespository.Verify(
                x =>
                    x.SaveMessage(_validCommand.CommitmentId,
                        It.Is<Message>(m => m.Author == _validCommand.LastUpdatedByName && m.CreatedBy == _validCommand.Caller.CallerType && m.Text == string.Empty)), Times.Once);
        }

        [Test]
        public async Task ThenIfAMessageIsProvidedThenTheMessageIsSaved()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly, ProviderId = 1234 };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.Message = "New Message";

            //Act
            await _handler.Handle(_validCommand);

            //Assert
            _mockCommitmentRespository.Verify(
                x =>
                    x.SaveMessage(_validCommand.CommitmentId,
                        It.Is<Message>(m => m.Author == _validCommand.LastUpdatedByName && m.CreatedBy == _validCommand.Caller.CallerType && m.Text == _validCommand.Message)), Times.Once);
        }

        [Test]
        public async Task ThenIfTheCallerIsTheEmployerThenTheCommitmentStatusesAreUpdatedCorrectly()
        {
            var commitment = new Commitment { Id = 123L, ProviderId = 333, ProviderCanApproveCommitment = false, EditStatus = EditStatus.EmployerOnly, EmployerAccountId = 444 };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.Caller.CallerType = CallerType.Employer;
            
            await _handler.Handle(_validCommand);

            _mockCommitmentRespository.Verify(
                x =>
                    x.UpdateCommitment(It.Is<Commitment>(
                            y => y.EditStatus == EditStatus.ProviderOnly 
                                && y.CommitmentStatus == CommitmentStatus.Active
                                && y.TransferApprovalStatus == null
                                && y.LastUpdatedByEmployerEmail == _validCommand.LastUpdatedByEmail
                                && y.LastUpdatedByEmployerName == _validCommand.LastUpdatedByName
                                && y.LastAction == (Domain.Entities.LastAction)_validCommand.LatestAction)), Times.Once);
        }

        [Test]
        public async Task ThenIfTheCallerIsTheProviderThenTheCommitmentStatusesAreUpdatedCorrectly()
        {
            var commitment = new Commitment { Id = 123L, ProviderId = 444, ProviderCanApproveCommitment = false, EditStatus = EditStatus.ProviderOnly};
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.Caller.CallerType = CallerType.Provider;
            
            await _handler.Handle(_validCommand);

            _mockCommitmentRespository.Verify(
                x =>
                    x.UpdateCommitment(It.Is<Commitment>(
                            y => y.EditStatus == EditStatus.EmployerOnly
                                && y.CommitmentStatus == CommitmentStatus.Active
                                && y.TransferApprovalStatus == null
                                && y.LastUpdatedByProviderEmail == _validCommand.LastUpdatedByEmail
                                && y.LastUpdatedByProviderName == _validCommand.LastUpdatedByName
                                && y.LastAction == (Domain.Entities.LastAction)_validCommand.LatestAction)), Times.Once);
        }

        [Test]
        public async Task ThenIfTheCommitmentIsSentForReviewThenAHistoryRecordIsCreated()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly, ProviderId = 1234 };
            var expectedOriginalState = JsonConvert.SerializeObject(commitment);

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            await _handler.Handle(_validCommand);

            var expectedNewState = JsonConvert.SerializeObject(commitment);

            _mockHistoryRepository.Verify(
                x =>
                    x.InsertHistory(
                        It.Is<IEnumerable<HistoryItem>>(
                            y =>
                                y.First().ChangeType == CommitmentChangeType.SentForReview.ToString() &&
                                y.First().CommitmentId == commitment.Id &&
                                y.First().ApprenticeshipId == null &&
                                y.First().OriginalState == expectedOriginalState &&
                                y.First().UpdatedByRole == _validCommand.Caller.CallerType.ToString() &&
                                y.First().UpdatedState == expectedNewState &&
                                y.First().ProviderId == commitment.ProviderId &&
                                y.First().EmployerAccountId == commitment.EmployerAccountId &&
                                y.First().UserId == _validCommand.UserId)), Times.Once);
        }

        [Test]
        public async Task ThenWhenACommitmentHasBeenApprovedByTheProviderAndTheEmployerReturnsTheCommitmentAnEventIsCreated()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly, ProviderId = 1234 };
            var apprenticeship = new Apprenticeship { AgreementStatus = AgreementStatus.ProviderAgreed, StartDate = DateTime.Now.AddMonths(1), Cost = 1000 };
            commitment.Apprenticeships = new List<Apprenticeship> { apprenticeship };

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);
            _mockApprenticeshipRespository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<ApprenticeshipResult>());

            _validCommand.LatestAction = LastAction.Amend;
            await _handler.Handle(_validCommand);

            _messagePublisher.Verify(
                x =>
                    x.PublishAsync(
                        It.Is<ApprovedCohortReturnedToProvider>(y =>
                            y.ProviderId == commitment.ProviderId && y.AccountId == commitment.EmployerAccountId &&
                            y.CommitmentId == commitment.Id)));
        }

        [Test]
        public async Task ThenIfCallerIsProviderAndLastActionIsNone_NotifyProviderAmendedCohortIsNotCalled()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.ProviderOnly, ProviderId = 325 };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.LatestAction = LastAction.None;
            _validCommand.Caller.CallerType = CallerType.Provider;
            _validCommand.Caller.Id = 325;

            await _handler.Handle(_validCommand);

            _notificationsPublisher.Verify(x => x.ProviderAmendedCohort(It.IsAny<Commitment>()), Times.Never);
        }

        [TestCase(LastAction.Amend)]
        [TestCase(LastAction.AmendAfterRejected)]
        public async Task ThenIfCallerIsProviderAndLastActionIs_NotifyProviderAmendedCohortIsNotCalled(LastAction lastAction)
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.ProviderOnly, ProviderId = 325 };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.LatestAction = lastAction;
            _validCommand.Caller.CallerType = CallerType.Provider;
            _validCommand.Caller.Id = 325;

            await _handler.Handle(_validCommand);

            _notificationsPublisher.Verify(x => x.ProviderAmendedCohort(commitment), Times.Once);
        }

    }
}

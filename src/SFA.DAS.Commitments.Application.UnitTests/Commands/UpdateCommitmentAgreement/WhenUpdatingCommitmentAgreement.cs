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

using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
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
        private Mock<IMediator> _mockMediator;
        private Mock<IApprenticeshipEventsList> _mockApprenticeshipEventsList;
        private Mock<IApprenticeshipEventsPublisher> _mockApprenticeshipEventsPublisher;
        private Mock<IHistoryRepository> _mockHistoryRepository;
        private Mock<ICurrentDateTime> _currentDateTime;
        private Mock<IMessagePublisher> _messagePublisher;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>()))
                .ReturnsAsync(new GetOverlappingApprenticeshipsResponse
                {
                    Data = new List<ApprenticeshipResult>()
                });

            _currentDateTime = new Mock<ICurrentDateTime>();
            _currentDateTime.Setup(x => x.Now).Returns(new DateTime(2018,1,1));

            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();
            _mockApprenticeshipRespository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<ApprenticeshipResult>());
            _mockApprenticeshipEventsList = new Mock<IApprenticeshipEventsList>();
            _mockApprenticeshipEventsPublisher = new Mock<IApprenticeshipEventsPublisher>();
            _mockHistoryRepository = new Mock<IHistoryRepository>();
            _messagePublisher = new Mock<IMessagePublisher>();
            _handler = new UpdateCommitmentAgreementCommandHandler(
                _mockCommitmentRespository.Object,
                _mockApprenticeshipRespository.Object,
                new ApprenticeshipUpdateRules(), 
                Mock.Of<ICommitmentsLogger>(),
                _mockMediator.Object,
                new UpdateCommitmentAgreementCommandValidator(),
                _mockApprenticeshipEventsList.Object,
                _mockApprenticeshipEventsPublisher.Object,
                _mockHistoryRepository.Object,
                _currentDateTime.Object,
                _messagePublisher.Object);

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
        public void ShouldThrowAnExceptionIfEmployerTriesToApproveWhenCommitmentNotComplete()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = false, EditStatus = EditStatus.EmployerOnly };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.Caller.Id = 444;
            _validCommand.Caller.CallerType = Domain.CallerType.Employer;
            _validCommand.LatestAction = LastAction.Approve;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };
            act.ShouldThrow<InvalidOperationException>().WithMessage("Commitment 123 cannot be approved because apprentice information is incomplete");
        }

        [Test]
        public void ShouldThrowAnExceptionIfProviderTriesToApproveWhenCommitmentNotComplete()
        {
            var commitment = new Commitment { Id = 123L, ProviderId = 333, ProviderCanApproveCommitment = false, EditStatus = EditStatus.ProviderOnly };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.Caller.Id = 333;
            _validCommand.Caller.CallerType = Domain.CallerType.Provider;
            _validCommand.LatestAction = LastAction.Approve;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };
            act.ShouldThrow<InvalidOperationException>().WithMessage("Commitment 123 cannot be approved because apprentice information is incomplete");
        }


        [Test]
        public async Task ThenIfApprovingCommitmentThenMediatorIsCalledToCheckForOverlappingApprenticeships()
        {
            //Arrange
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.LatestAction = LastAction.Approve;

            //Act
            await _handler.Handle(_validCommand);

            //Assert
            _mockMediator.Verify(x => x.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>()), Times.Once);
        }

        [Test]
        public async Task ThenIfNotApprovingCommitmentThenMediatorIsNotCalledToCheckForOverlappingApprenticeships()
        {
            //Arrange
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly, ProviderId = 1234 };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.LatestAction = LastAction.Amend;

            //Act
            await _handler.Handle(_validCommand);

            //Assert
            _mockMediator.Verify(x => x.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>()), Times.Never);
        }

        [Test]
        public void ThenIfApprovingCommitmentThenIfThereAreOverlappingApprenticeshipsThenThrowAnException()
        {
            //Arrange
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>()))
                .ReturnsAsync(new GetOverlappingApprenticeshipsResponse
                {
                    Data = new List<ApprenticeshipResult>
                    {
                        new ApprenticeshipResult()
                    }
                });

            _validCommand.LatestAction = LastAction.Approve;

            //Act
            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            //Assert
            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public async Task ThenIfAnApprenticeshipAgreementStatusIsUpdatedTheApprenticeshipStatusesAreUpdated()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly };
            var apprenticeship = new Apprenticeship { AgreementStatus = AgreementStatus.NotAgreed, PaymentStatus = PaymentStatus.PendingApproval, Id = 1234 };
            commitment.Apprenticeships.Add(apprenticeship);

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.LatestAction = LastAction.Approve;

            await _handler.Handle(_validCommand);
            
            _mockApprenticeshipRespository.Verify(x => x.UpdateApprenticeshipStatuses(It.Is<List<Apprenticeship>>(y => y.First().AgreementStatus == AgreementStatus.EmployerAgreed)), Times.Once);
        }

        [Test]
        public async Task ThenIfAnApprenticeshipAgreementStatusIsBothAgreedTheAgreedOnDateIsUpdated()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly, ProviderId = 1234 };
            var apprenticeship = new Apprenticeship { AgreementStatus = AgreementStatus.ProviderAgreed, PaymentStatus = PaymentStatus.PendingApproval, Id = 1234, StartDate = new DateTime(2018,1,11), Cost = 1000};
            commitment.Apprenticeships.Add(apprenticeship);

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            var updatedApprenticeship = new Apprenticeship();
            _mockApprenticeshipRespository.Setup(x => x.GetApprenticeship(apprenticeship.Id)).ReturnsAsync(updatedApprenticeship);
            _mockApprenticeshipRespository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.Is<IEnumerable<string>>(y => y.First() == apprenticeship.ULN))).ReturnsAsync(new List<ApprenticeshipResult>());

            _validCommand.LatestAction = LastAction.Approve;

            await _handler.Handle(_validCommand);

            _mockApprenticeshipRespository.Verify(x => x.UpdateApprenticeshipStatuses(It.Is<List<Apprenticeship>>(y => y.First().AgreedOn.Value.Date == new DateTime(2018,1,1).Date)), Times.Once);
        }

        [Test]
        public async Task ThenIfAnApprenticeshipAgreementStatusIsBothAgreedAndTheAgreedOnDateIsAlreadySetTheAgreedOnDateIsNotUpdated()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly, ProviderId = 1234 };
            var expectedAgreedOnDate = DateTime.Now.AddDays(-10);
            var apprenticeship = new Apprenticeship { AgreementStatus = AgreementStatus.ProviderAgreed, PaymentStatus = PaymentStatus.PendingApproval, Id = 1234, AgreedOn = expectedAgreedOnDate, StartDate = DateTime.Now.AddDays(10), Cost = 1000 };
            commitment.Apprenticeships.Add(apprenticeship);

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _mockApprenticeshipRespository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.Is<IEnumerable<string>>(y => y.First() == apprenticeship.ULN))).ReturnsAsync(new List<ApprenticeshipResult>());

            _validCommand.LatestAction = LastAction.Approve;

            await _handler.Handle(_validCommand);

            _mockApprenticeshipRespository.Verify(x => x.UpdateApprenticeshipStatuses(It.Is<List<Apprenticeship>>(y => y.First().AgreedOn.Value == expectedAgreedOnDate)), Times.Once);
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

            _validCommand.LatestAction = LastAction.Approve;

            await _handler.Handle(_validCommand);

            _mockApprenticeshipRespository.Verify(x => x.UpdateApprenticeshipStatuses(It.Is<List<Apprenticeship>>(y => y.First().PaymentStatus == PaymentStatus.PendingApproval)), Times.Once);
        }

        [Test]
        public async Task ThenIfAnApprenticeshipIsUpdatedWithoutBeingApprovedAnEventIsPublishedWithoutAnEffectiveFromDate()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly };
            var apprenticeship = new Apprenticeship { AgreementStatus = AgreementStatus.NotAgreed, PaymentStatus = PaymentStatus.PendingApproval, Id = 1234 };
            commitment.Apprenticeships.Add(apprenticeship);

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.LatestAction = LastAction.Approve;

            await _handler.Handle(_validCommand);
            _mockApprenticeshipEventsList.Verify(x => x.Add(commitment, apprenticeship, "APPRENTICESHIP-AGREEMENT-UPDATED", null, null), Times.Once);
            _mockApprenticeshipEventsPublisher.Verify(x => x.Publish(_mockApprenticeshipEventsList.Object), Times.Once);
        }

        [Test]
        public async Task ThenIfAnApprenticeshipIsApprovedAndTheLearnerHasNoPreviousApprenticeshipsAnEventIsPublishedWithTheFirstOfTheStartMonthAsTheEffectiveFromDate()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly, ProviderId = 1234 };
            var apprenticeship = new Apprenticeship { AgreementStatus = AgreementStatus.ProviderAgreed, PaymentStatus = PaymentStatus.PendingApproval, Id = 1234, StartDate = DateTime.Now, ULN = "1234567", Cost = 1000 };
            commitment.Apprenticeships.Add(apprenticeship);

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _mockApprenticeshipRespository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.Is<IEnumerable<string>>(y => y.First() == apprenticeship.ULN))).ReturnsAsync(new List<ApprenticeshipResult>());

            _validCommand.LatestAction = LastAction.Approve;

            await _handler.Handle(_validCommand);

            var expectedStartDate = new DateTime(apprenticeship.StartDate.Value.Year, apprenticeship.StartDate.Value.Month, 1);
            _mockApprenticeshipEventsList.Verify(x => x.Add(commitment, apprenticeship, "APPRENTICESHIP-AGREEMENT-UPDATED", expectedStartDate, null), Times.Once);
            _mockApprenticeshipEventsPublisher.Verify(x => x.Publish(_mockApprenticeshipEventsList.Object), Times.Once);
        }

        [Test]
        public async Task ThenIfAnApprenticeshipIsApprovedAndTheLearnerHasAPreviousApprenticeshipStoppedInThePreviousMonthAnEventIsPublishedWithTheFirstOfTheStartMonthAsTheEffectiveFromDate()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly, ProviderId = 1234 };
            var apprenticeship = new Apprenticeship
            {
                AgreementStatus = AgreementStatus.ProviderAgreed,
                PaymentStatus = PaymentStatus.PendingApproval,
                Id = 1234,
                StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 13),
                ULN = "1234567",
                Cost = 1000
            };
            commitment.Apprenticeships.Add(apprenticeship);

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _mockApprenticeshipRespository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.Is<IEnumerable<string>>(y => y.First() == apprenticeship.ULN)))
                .ReturnsAsync(new List<ApprenticeshipResult>
                {
                    new ApprenticeshipResult { StartDate = apprenticeship.StartDate.Value.AddMonths(-2), StopDate = apprenticeship.StartDate.Value.AddMonths(-1), Uln = apprenticeship.ULN}
                });

            _validCommand.LatestAction = LastAction.Approve;

            await _handler.Handle(_validCommand);

            var expectedStartDate = new DateTime(apprenticeship.StartDate.Value.Year, apprenticeship.StartDate.Value.Month, 1);
            _mockApprenticeshipEventsList.Verify(x => x.Add(commitment, apprenticeship, "APPRENTICESHIP-AGREEMENT-UPDATED", expectedStartDate, null), Times.Once);
            _mockApprenticeshipEventsPublisher.Verify(x => x.Publish(_mockApprenticeshipEventsList.Object), Times.Once);
        }

        [Test]
        public async Task ThenIfAnApprenticeshipIsApprovedAndTheLearnerHasAPreviousApprenticeshipStoppedInTheStartMonthAnEventIsPublishedWithTheEffectiveFromDateBeingADayAfterTheStoppedDate()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly, ProviderId = 1234 };
            var apprenticeship = new Apprenticeship
            {
                AgreementStatus = AgreementStatus.ProviderAgreed,
                PaymentStatus = PaymentStatus.PendingApproval,
                Id = 1234,
                StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 13),
                ULN = "1234567",
                Cost = 1000
            };
            commitment.Apprenticeships.Add(apprenticeship);

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            var stoppedDate = apprenticeship.StartDate.Value.AddDays(-5);
            _mockApprenticeshipRespository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.Is<IEnumerable<string>>(y => y.First() == apprenticeship.ULN)))
                .ReturnsAsync(new List<ApprenticeshipResult>
                {
                    new ApprenticeshipResult { StartDate = apprenticeship.StartDate.Value.AddMonths(-4), StopDate = apprenticeship.StartDate.Value.AddMonths(-3), Uln = apprenticeship.ULN },
                    new ApprenticeshipResult { StartDate = apprenticeship.StartDate.Value.AddMonths(-2), StopDate = stoppedDate, Uln = apprenticeship.ULN }
                });

            _validCommand.LatestAction = LastAction.Approve;

            await _handler.Handle(_validCommand);

            var expectedStartDate = stoppedDate.AddDays(1);
            _mockApprenticeshipEventsList.Verify(x => x.Add(commitment, apprenticeship, "APPRENTICESHIP-AGREEMENT-UPDATED", expectedStartDate, null), Times.Once);
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
                                && y.LastUpdatedByEmployerEmail == _validCommand.LastUpdatedByEmail
                                && y.LastUpdatedByEmployerName == _validCommand.LastUpdatedByName
                                && y.LastAction == (Domain.Entities.LastAction)_validCommand.LatestAction)), Times.Once);
        }

        [Test]
        public async Task ThenIfTheCallerIsTheProviderThenTheCommitmentStatusesAreUpdatedCorrectly()
        {
            var commitment = new Commitment { Id = 123L, ProviderId = 444, ProviderCanApproveCommitment = false, EditStatus = EditStatus.ProviderOnly };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.Caller.CallerType = CallerType.Provider;
            
            await _handler.Handle(_validCommand);

            _mockCommitmentRespository.Verify(
                x =>
                    x.UpdateCommitment(It.Is<Commitment>(
                            y => y.EditStatus == EditStatus.EmployerOnly
                                && y.CommitmentStatus == CommitmentStatus.Active
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
        public async Task ThenIfTheCommitmentIsSentForFinalApprovalThenAHistoryRecordIsCreated()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly, ProviderId = 1234 };
            commitment.Apprenticeships.Add(new Apprenticeship { AgreementStatus = AgreementStatus.ProviderAgreed, StartDate = DateTime.Now.AddMonths(1), Cost = 1000 });
            var expectedOriginalState = JsonConvert.SerializeObject(commitment);

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);
            _mockApprenticeshipRespository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<ApprenticeshipResult>());

            _validCommand.LatestAction = LastAction.Approve;
            await _handler.Handle(_validCommand);

            var expectedNewState = JsonConvert.SerializeObject(commitment);

            _mockHistoryRepository.Verify(
                x =>
                    x.InsertHistory(
                        It.Is<IEnumerable<HistoryItem>>(
                            y =>
                                y.First().ChangeType == CommitmentChangeType.FinalApproval.ToString() &&
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
        public async Task ThenIfTheCommitmentIsSentForApprovalThenAHistoryRecordIsCreated()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly };
            var expectedOriginalState = JsonConvert.SerializeObject(commitment);

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.LatestAction = LastAction.Approve;

            await _handler.Handle(_validCommand);

            var expectedNewState = JsonConvert.SerializeObject(commitment);

            _mockHistoryRepository.Verify(
                x =>
                    x.InsertHistory(
                        It.Is<IEnumerable<HistoryItem>>(
                            y =>
                                y.First().ChangeType == CommitmentChangeType.SentForApproval.ToString() &&
                                y.First().CommitmentId == commitment.Id &&
                                y.First().ApprenticeshipId == null &&
                                y.First().OriginalState == expectedOriginalState &&
                                y.First().UpdatedByRole == _validCommand.Caller.CallerType.ToString() &&
                                y.First().UpdatedState == expectedNewState &&
                                y.First().UserId == _validCommand.UserId &&
                                y.First().ProviderId == commitment.ProviderId &&
                                y.First().EmployerAccountId == commitment.EmployerAccountId &&
                                y.First().UpdatedByName == _validCommand.LastUpdatedByName)), Times.Once);
        }

        [Test]
        public async Task ThenIfACommitmentIsSentForApprovalByTheProviderAnEventIsCreated()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, ProviderCanApproveCommitment = true, EditStatus = EditStatus.ProviderOnly, ProviderId = 1234 };
            var apprenticeship = new Apprenticeship { AgreementStatus = AgreementStatus.NotAgreed, StartDate = DateTime.Now.AddMonths(1), Cost = 1000 };
            commitment.Apprenticeships = new List<Apprenticeship> {apprenticeship};
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.LatestAction = LastAction.Approve;
            _validCommand.Caller.CallerType = CallerType.Provider;
            _validCommand.Caller.Id = commitment.ProviderId.Value;

            await _handler.Handle(_validCommand);

            _messagePublisher.Verify(
                x =>
                    x.PublishAsync(
                        It.Is<CohortApprovalRequestedByProvider>(y =>
                            y.ProviderId == commitment.ProviderId && y.AccountId == commitment.EmployerAccountId &&
                            y.CommitmentId == commitment.Id)));
        }

        [Test]
        public async Task ThenIfTheCommitmentIsApprovedByTheEmployerAnEventIsCreated()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly, ProviderId = 1234 };
            var apprenticeship = new Apprenticeship { AgreementStatus = AgreementStatus.ProviderAgreed, StartDate = DateTime.Now.AddMonths(1), Cost = 1000 };
            commitment.Apprenticeships = new List<Apprenticeship> { apprenticeship };

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);
            _mockApprenticeshipRespository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<ApprenticeshipResult>());

            _validCommand.LatestAction = LastAction.Approve;
            await _handler.Handle(_validCommand);

            _messagePublisher.Verify(
                x =>
                    x.PublishAsync(
                        It.Is<CohortApprovedByEmployer>(y =>
                            y.ProviderId == commitment.ProviderId && y.AccountId == commitment.EmployerAccountId &&
                            y.CommitmentId == commitment.Id)));
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
    }
}

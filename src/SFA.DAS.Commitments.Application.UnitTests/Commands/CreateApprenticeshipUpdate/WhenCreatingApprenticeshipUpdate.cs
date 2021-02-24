using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types.Validation;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;
using SFA.DAS.Reservations.Api.Types;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateApprenticeshipUpdate
{
    [TestFixture]
    public class WhenCreatingApprenticeshipUpdate
    {
        private Mock<CreateApprenticeshipUpdateValidator> _validator;

        private Mock<IApprenticeshipUpdateRepository> _apprenticeshipUpdateRepository;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private Mock<IMediator> _mediator;
        private Mock<IHistoryRepository> _historyRepository;
        private Mock<ICommitmentRepository> _commitmentRepository;
        private Mock<ICurrentDateTime> _mockCurrentDateTime;
        private Mock<IMessagePublisher> _messagePublisher;
        private Mock<IApprenticeshipEventsList> _apprenticeshipEventsList;
        private Mock<IApprenticeshipEventsPublisher> _apprenticeshipEventsPublisher;
        private Mock<IReservationValidationService> _reservationsValidationService;
        private Mock<IV2EventsPublisher> _v2EventsPublisher;

        private CreateApprenticeshipUpdateCommandHandler _handler;
        private Apprenticeship _existingApprenticeship;

        [SetUp]
        public void Arrange()
        {
            _validator = new Mock<CreateApprenticeshipUpdateValidator>();
            _apprenticeshipUpdateRepository = new Mock<IApprenticeshipUpdateRepository>();
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _mediator = new Mock<IMediator>();
            _historyRepository = new Mock<IHistoryRepository>();
            _commitmentRepository = new Mock<ICommitmentRepository>();
            _mockCurrentDateTime = new Mock<ICurrentDateTime>();
            _messagePublisher = new Mock<IMessagePublisher>();
            _apprenticeshipEventsList = new Mock<IApprenticeshipEventsList>();
            _apprenticeshipEventsPublisher = new Mock<IApprenticeshipEventsPublisher>();
            _reservationsValidationService = new Mock<IReservationValidationService>();
            _v2EventsPublisher = new Mock<IV2EventsPublisher>();

            _validator.Setup(x => x.Validate(It.IsAny<CreateApprenticeshipUpdateCommand>()))
                .Returns(() => new ValidationResult());

            _apprenticeshipUpdateRepository.Setup(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync((ApprenticeshipUpdate)null);

            _apprenticeshipUpdateRepository.Setup(x => x.CreateApprenticeshipUpdate(It.IsAny<ApprenticeshipUpdate>(), It.IsAny<Apprenticeship>()))
                .Returns(() => Task.FromResult(new Unit()));

            _existingApprenticeship = new Apprenticeship
            {
                EmployerAccountId = 1,
                ProviderId = 2,
                ULN = " 123",
                StartDate = new DateTime(DateTime.Now.Year + 2, 5, 1),
                EndDate = new DateTime(DateTime.Now.Year + 2, 9, 1),
                Id = 3,
                CommitmentId = 974
            };

 			_apprenticeshipRepository.Setup(x => x.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(_existingApprenticeship);

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>()))
                .ReturnsAsync(new GetOverlappingApprenticeshipsResponse
                {
                    Data = new List<ApprenticeshipResult>()
                });

            _commitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(new Commitment());
            _mockCurrentDateTime.SetupGet(x => x.Now).Returns(DateTime.UtcNow);

            _reservationsValidationService
                .Setup(rvs => rvs.CheckReservation(It.IsAny<ReservationValidationServiceRequest>()))
                .ReturnsAsync(new ReservationValidationResult());

            _v2EventsPublisher
                .Setup(x => x.PublishApprenticeshipUlnUpdatedEvent(It.IsAny<Apprenticeship>()))
                .Returns(Task.CompletedTask);

            _handler = new CreateApprenticeshipUpdateCommandHandler(
                _validator.Object, 
                _apprenticeshipUpdateRepository.Object, 
                Mock.Of<ICommitmentsLogger>(),
                _apprenticeshipRepository.Object, 
                _mediator.Object, 
                _historyRepository.Object, 
                _commitmentRepository.Object, 
                _mockCurrentDateTime.Object,
                _messagePublisher.Object,
                _apprenticeshipEventsList.Object,
                _apprenticeshipEventsPublisher.Object,
                _reservationsValidationService.Object,
                _v2EventsPublisher.Object);
        }

        [Test]
        public async Task ThenTheRequestIsValidated()
        {
            //Arrange
            var command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(1, CallerType.Employer),
                ApprenticeshipUpdate = new ApprenticeshipUpdate()
            };

            //Act
            await _handler.Handle(command);

            //Assert
            _validator.Verify(x => x.Validate(It.IsAny<CreateApprenticeshipUpdateCommand>()), Times.Once);
        }

        [Test]
        public void ThenIfTheProviderFailsAuthorisationThenAnExceptionIsThrown()
        {
            //Arrange
            var command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(666, CallerType.Provider),
                ApprenticeshipUpdate = new ApprenticeshipUpdate()
            };

            //Act && Assert
            Func<Task> act = async () => await _handler.Handle(command);
            act.ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public void ThenIfTheEmployerFailsAuthorisationThenAnExceptionIsThrown()
        {
            //Arrange
            var command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(666, CallerType.Employer),
                ApprenticeshipUpdate = new ApprenticeshipUpdate()
            };

            //Act && Assert
            Func<Task> act = async () => await _handler.Handle(command);
            act.ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public void ThenIfTheRequestIsInvalidThenAValidationFailureExceptionIsThrown()
        {
            //Arrange
            _validator.Setup(x => x.Validate(It.IsAny<CreateApprenticeshipUpdateCommand>()))
                .Returns(() =>
                        new ValidationResult(new List<ValidationFailure>
                        {
                            new ValidationFailure("Error", "Error Message")
                        }));

            var request = new CreateApprenticeshipUpdateCommand();

            //Act && Assert
            Func<Task> act = async () => await _handler.Handle(request);
            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public async Task ThenTheRepositoryIsCalledToCreateRecord()
        {
            //Arrange
            var command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(2, CallerType.Provider),
                ApprenticeshipUpdate = new ApprenticeshipUpdate()
            };

            //Act
            await _handler.Handle(command);

            //Assert
            _apprenticeshipUpdateRepository.Verify(x => x.CreateApprenticeshipUpdate(It.IsAny<ApprenticeshipUpdate>(), It.IsAny<Apprenticeship>()), Times.Once);
        }

        [Test]
        public async Task ThenTheRepositoryIsCalledToCreateRecordWhenStarted()
        {
            //Arrange
            var command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(2, CallerType.Provider),
                ApprenticeshipUpdate = new ApprenticeshipUpdate()
            };

            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(new Apprenticeship
                {
                    EmployerAccountId = 1,
                    ProviderId = 2,
                    ULN = "123",
                    StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1),
                    EndDate = new DateTime(DateTime.Now.Year + 1, 5, 1),
                    Id = 3
                });

            //Act
            await _handler.Handle(command);

            //Assert
            _apprenticeshipUpdateRepository.Verify(x => x.CreateApprenticeshipUpdate(It.IsAny<ApprenticeshipUpdate>(), It.IsAny<Apprenticeship>()), Times.Once);
        }

        [Test]
        public void ThenIfTheApprenticeshipAlreadyHasAPendingUpdateThenAnExceptionIsThrown()
        {
            //Arrange
            _apprenticeshipUpdateRepository.Setup(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(new ApprenticeshipUpdate());

            var command = new CreateApprenticeshipUpdateCommand
            {
                ApprenticeshipUpdate = new ApprenticeshipUpdate()
            };

            //Act && Assert
            Func<Task> act = async () => await _handler.Handle(command);
            act.ShouldThrow<ValidationException>();

            //Assert
            _apprenticeshipUpdateRepository.Verify(x => x.CreateApprenticeshipUpdate(It.IsAny<ApprenticeshipUpdate>(), It.IsAny<Apprenticeship>()), Times.Never);
        }

        [Test]
        public async Task ThenIfTheUpdateContainsNoDataForImmediateEffectThenTheApprenticeshipIsNotUpdated()
        {
            //Arrange
            var command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(2, CallerType.Provider),
                ApprenticeshipUpdate = new ApprenticeshipUpdate
                {
                    FirstName = "Test",
                    LastName = "Tester"
                }
            };

            //Act
            await _handler.Handle(command);

            //Assert
            _apprenticeshipUpdateRepository.Verify(x => x.CreateApprenticeshipUpdate(
                It.Is<ApprenticeshipUpdate>(y=> y != null),
                It.Is<Apprenticeship>(y=> y == null)),
                Times.Once);
        }

        [Test]
        public async Task ThenIfTheUpdateContainsNoDataForApprovalThenTheUpdateIsNotCreated()
        {
            //Arrange
            var command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(2, CallerType.Provider),
                ApprenticeshipUpdate = new ApprenticeshipUpdate
                {
                }
            };

            //Act
            await _handler.Handle(command);

            //Assert
            _apprenticeshipUpdateRepository.Verify(x => x.CreateApprenticeshipUpdate(
                It.Is<ApprenticeshipUpdate>(y => y == null),
                It.Is<Apprenticeship>(y => y != null)),
                Times.Never);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledToCheckOverlappingApprenticeships()
        {
            //Arrange
            var command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(2, CallerType.Provider),
                ApprenticeshipUpdate = new ApprenticeshipUpdate
                {
                    ULN = "123"
                }
            };

            //Act
            await _handler.Handle(command);

            //Assert
            _mediator.Verify(x => x.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>()), Times.Once);
        }

        [Test]
        public void ThenIfApprenticeHasStarted_And_DataLockExists_ValidationFailureExceptionIsThrown_IfCostIsChanged()
        {
            _mockCurrentDateTime.SetupGet(x => x.Now).Returns(new DateTime(2017, 7, 13));

            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(new Apprenticeship
                {
                    EmployerAccountId = 1,
                    ProviderId = 2,
                    ULN = " 123",
                    StartDate = new DateTime(2017, 6, 1),
                    EndDate = new DateTime(2017, 5, 1),
                    Id = 3,
                    HasHadDataLockSuccess = true
                });

            var request = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(2, CallerType.Provider),
                ApprenticeshipUpdate =
                                      new ApprenticeshipUpdate
                                      {
                                          Id = 5,
                                          ApprenticeshipId = 42,
                                          Cost = 1234
                                      }
            };

            //Act && Assert
            Func<Task> act = async () => await _handler.Handle(request);
            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public void ThenIfApprenticeHasStarted_And_DataLockSuccessExist_ValidationFailureExceptionIsThrown_IfUpdated_TrainingCode()
        {
            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(new Apprenticeship
                {
                    EmployerAccountId = 1,
                    ProviderId = 2,
                    ULN = " 123",
                    StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1),
                    EndDate = new DateTime(DateTime.Now.Year + 1, 5, 1),
                    Id = 3,
                    HasHadDataLockSuccess = true
                });

            var request = new CreateApprenticeshipUpdateCommand
            {
                ApprenticeshipUpdate =
                new ApprenticeshipUpdate
                {
                    Id = 5,
                    ApprenticeshipId = 42,
                    TrainingCode = "abc-123"
                },
                Caller = new Caller(2, CallerType.Provider)
            };

            //Act && Assert
            Func<Task> act = async () => await _handler.Handle(request);
            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public async Task ThenIfApprenticeHasStarted_UpdateApprenticeship_IfUpdated_TrainingCode_Or_Cost()
        {
            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(new Apprenticeship
                {
                    EmployerAccountId = 1,
                    ProviderId = 2,
                    LegalEntityId = "12345",
                    ULN = " 123",
                    StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1),
                    EndDate = new DateTime(DateTime.Now.Year + 1, 5, 1),
                    Id = 42
                });


            var trainingCode = "abc-123";
            var cost = 1500;
            var request = new CreateApprenticeshipUpdateCommand
            {
                ApprenticeshipUpdate =
                                      new ApprenticeshipUpdate
                                      {
                                          Id = 5,
                                          ApprenticeshipId = 42,
                                          TrainingCode = trainingCode,
                                          Cost = cost
                                      },
                Caller = new Caller(1, CallerType.Employer)
            };

            //Act && Assert
            await _handler.Handle(request);

            _apprenticeshipUpdateRepository.Verify(
                x => x.CreateApprenticeshipUpdate(
                    It.Is<ApprenticeshipUpdate>(u => u.ApprenticeshipId == 42 && u.TrainingCode == trainingCode && u.Cost == cost),
                    It.IsAny<Apprenticeship>()));
        }


        [Test]
        public async Task ThenIfTheUpdateContainsNoDataForImmediateEffectTheNoHistoryIsCreated()
        {
            //Arrange
            var command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(2, CallerType.Provider),
                ApprenticeshipUpdate = new ApprenticeshipUpdate
                {
                    FirstName = "Test",
                    LastName = "Tester"
                }
            };

            //Act
            await _handler.Handle(command);

            //Assert
            _historyRepository.Verify(x => x.InsertHistory(It.IsAny<IEnumerable<HistoryItem>>()), Times.Never);
        }
		
		 [Test]
        public async Task ThenIfTheApprenticeshipIsWaitingToStartThenTheChangeWillBeEffectiveFromTheApprenticeshipStartDate()
        {
            //Arrange
            var command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(2, CallerType.Provider),
                ApprenticeshipUpdate = new ApprenticeshipUpdate
                {
                    ULN = "123",
                    Cost = 100
                }
            };

            //Act
            await _handler.Handle(command);

            //Assert
            _apprenticeshipUpdateRepository.Verify(
                x => x.CreateApprenticeshipUpdate(
                    It.Is<ApprenticeshipUpdate>(u => u.EffectiveFromDate == _existingApprenticeship.StartDate),
                    It.IsAny<Apprenticeship>()));
        }

        [Test]
        public async Task ThenIfTheUpdateContainsDataForImmediateEffectTheHistoryRecordsAreCreated()
        {
            //Arrange
            var command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(2, CallerType.Provider),
                ApprenticeshipUpdate = new ApprenticeshipUpdate
                {
                    ULN = "Test",
                    ApprenticeshipId = 3
                }
            };

            var testCommitment = new Commitment { Id = 7643 };
            var expectedOriginalCommitmentState = JsonConvert.SerializeObject(testCommitment);
            _commitmentRepository.Setup(x => x.GetCommitmentById(_existingApprenticeship.CommitmentId)).ReturnsAsync(testCommitment);

            var expectedOriginalApprenticeshipState = JsonConvert.SerializeObject(_existingApprenticeship);

            //Act
            await _handler.Handle(command);

            var expectedNewApprenticeshipState = JsonConvert.SerializeObject(_existingApprenticeship);

            //Assert
            _historyRepository.Verify(
                x =>
                    x.InsertHistory(
                        It.Is<IEnumerable<HistoryItem>>(
                            y =>
                                y.First().ChangeType == CommitmentChangeType.EditedApprenticeship.ToString() &&
                                y.First().CommitmentId == testCommitment.Id &&
                                y.First().ApprenticeshipId == null &&
                                y.First().OriginalState == expectedOriginalCommitmentState &&
                                y.First().UpdatedByRole == command.Caller.CallerType.ToString() &&
                                y.First().UpdatedState == expectedOriginalCommitmentState &&
                                y.First().UserId == command.UserId &&
                                y.First().ProviderId == _existingApprenticeship.ProviderId &&
                                y.First().EmployerAccountId == _existingApprenticeship.EmployerAccountId &&
                                y.First().UpdatedByName == command.UserName)), Times.Once);

            _historyRepository.Verify(
                x =>
                    x.InsertHistory(
                        It.Is<IEnumerable<HistoryItem>>(
                            y =>
                                y.Last().ChangeType == ApprenticeshipChangeType.Updated.ToString() &&
                                y.Last().CommitmentId == null &&
                                y.Last().ApprenticeshipId == _existingApprenticeship.Id &&
                                y.Last().OriginalState == expectedOriginalApprenticeshipState &&
                                y.Last().UpdatedByRole == command.Caller.CallerType.ToString() &&
                                y.Last().UpdatedState == expectedNewApprenticeshipState &&
                                y.Last().UserId == command.UserId &&
                                y.Last().ProviderId == _existingApprenticeship.ProviderId &&
                                y.Last().EmployerAccountId == _existingApprenticeship.EmployerAccountId &&
                                y.Last().UpdatedByName == command.UserName)), Times.Once);
        }

        [Test]
        public async Task ThenIfAIfAPendingUpdateIsCreatedThenAnApprentieceshipUpdateCreatedEventIsCreated()
        {
            var apprenticeship = new Apprenticeship
            {
                EmployerAccountId = 1,
                ProviderId = 2,
                LegalEntityId = "12345",
                ULN = " 123",
                StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1),
                EndDate = new DateTime(DateTime.Now.Year + 1, 5, 1),
                Id = 42
            };
            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(It.IsAny<long>())).ReturnsAsync(apprenticeship);


            var trainingCode = "abc-123";
            var request = new CreateApprenticeshipUpdateCommand { ApprenticeshipUpdate = new ApprenticeshipUpdate { Id = 5, ApprenticeshipId = 42, TrainingCode = trainingCode }, Caller = new Caller(1, CallerType.Employer) };

            await _handler.Handle(request);

            _messagePublisher.Verify(
                x =>
                    x.PublishAsync(
                        It.Is<ApprenticeshipUpdateCreated>(
                            y => y.ApprenticeshipId == apprenticeship.Id && y.AccountId == apprenticeship.EmployerAccountId && y.ProviderId == apprenticeship.ProviderId)), Times.Once);
        }

        [Test]
        public async Task ThenIfTheCommandChangesTheULNThenAnEventIsPublishedAsItIsUpdatedImmediately()
        {
            //Arrange
            var command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(2, CallerType.Provider),
                ApprenticeshipUpdate = new ApprenticeshipUpdate
                {
                    ULN = "NewValue",
                    ApprenticeshipId = 3,
                    EffectiveFromDate = DateTime.Now
                }
            };

            var apprenticeship = new Apprenticeship
            {
                EmployerAccountId = 1,
                ProviderId = 2,
                ULN = "OldValue",
                StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1),
                EndDate = new DateTime(DateTime.Now.Year + 1, 5, 1),
            };

            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(It.IsAny<long>())).ReturnsAsync(apprenticeship);
            
            await _handler.Handle(command);

            _apprenticeshipEventsList.Verify(x=>x.Add(It.IsAny<Commitment>(), It.Is<Apprenticeship>(p=>p.ULN == "NewValue"), "APPRENTICESHIP-UPDATED", 
                It.Is<DateTime?>(p=>p == _mockCurrentDateTime.Object.Now), null));
            _apprenticeshipEventsPublisher.Verify(x=>x.Publish(_apprenticeshipEventsList.Object));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ThenTheReservationIsValidatedIfItHasAValue(bool setReservationId)
        {
            _existingApprenticeship.ReservationId = setReservationId ? Guid.NewGuid() : (Guid?)null;

            _reservationsValidationService
                .Setup(rc => rc.CheckReservation(
                        It.Is<ReservationValidationServiceRequest>(msg => msg.ReservationId == _existingApprenticeship.ReservationId)))
                .ReturnsAsync(new ReservationValidationResult());

            var command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(2, CallerType.Provider),
                ApprenticeshipUpdate = new ApprenticeshipUpdate()
            };

            await _handler.Handle(command);

            _reservationsValidationService
                .Verify(rc => rc.CheckReservation(
                        It.Is<ReservationValidationServiceRequest>(msg => msg.ReservationId == _existingApprenticeship.ReservationId)),
                    Times.Once());
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ThenAnExceptionIsThrownIfReservationExistsAndFailsValidation(bool failValidation)
        {
            _existingApprenticeship.ReservationId = Guid.NewGuid();

            var failures = failValidation ? new[] { new ReservationValidationError("Property", "Reason") } : new ReservationValidationError[0];

            var validationFailures = new ReservationValidationResult(failures);

            _reservationsValidationService
                .Setup(rc => rc.CheckReservation(
                    It.Is<ReservationValidationServiceRequest>(msg => msg.ReservationId == _existingApprenticeship.ReservationId)))
                .ReturnsAsync(validationFailures);

            var command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(2, CallerType.Provider),
                ApprenticeshipUpdate = new ApprenticeshipUpdate()
            };

            if (failValidation)
            {
                Assert.ThrowsAsync<ValidationException>(() =>
                    _handler.Handle(command));
            }
            else
            {
                await _handler.Handle(command);
            }
        }

        public async Task ThenUpdatedDateIsUsedToValidateReservationIfDateUpdated()
        {
            _existingApprenticeship.ReservationId = Guid.NewGuid();

            var command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(2, CallerType.Provider),
                ApprenticeshipUpdate = new ApprenticeshipUpdate
                {
                    StartDate = DateTime.Now.AddMonths(3)
                }
            };

            _existingApprenticeship.StartDate = null;

            await _handler.Handle(command);

            _reservationsValidationService.Verify(rvs => rvs.CheckReservation(It.Is<ReservationValidationServiceRequest>(request => request.StartDate == command.ApprenticeshipUpdate.StartDate.Value)), Times.Once);
        }

        public async Task ThenExistingDateIsUsedToValidateReservationIfDateNotUpdated()
        {
            _existingApprenticeship.ReservationId = Guid.NewGuid();


            var command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(2, CallerType.Provider),
                ApprenticeshipUpdate = new ApprenticeshipUpdate
                {
                    StartDate = null
                }
            };

            _existingApprenticeship.StartDate = DateTime.Now.AddMonths((4));

            await _handler.Handle(command);

            _reservationsValidationService.Verify(rvs => rvs.CheckReservation(It.Is<ReservationValidationServiceRequest>(request => request.StartDate == _existingApprenticeship.StartDate.Value)), Times.Once);
        }

        [Test]
        public void ThenShouldThrowExceptionIfAStartDateIsNotAvailable()
        {
            _existingApprenticeship.ReservationId = Guid.NewGuid();

            var command = new CreateApprenticeshipUpdateCommand
            {
                Caller = new Caller(2, CallerType.Provider),
                ApprenticeshipUpdate = new ApprenticeshipUpdate
                {
                    StartDate = null
                }
            };

            _existingApprenticeship.StartDate = null;

            Assert.ThrowsAsync<InvalidOperationException>(() =>_handler.Handle(command));
        }
    }
}

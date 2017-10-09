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
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;

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

            _validator.Setup(x => x.Validate(It.IsAny<CreateApprenticeshipUpdateCommand>()))
                .Returns(() => new ValidationResult());

            _apprenticeshipUpdateRepository.Setup(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(null);

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

            _handler = new CreateApprenticeshipUpdateCommandHandler(
                _validator.Object, 
                _apprenticeshipUpdateRepository.Object, 
                Mock.Of<ICommitmentsLogger>(),
                _apprenticeshipRepository.Object, 
                _mediator.Object, 
                _historyRepository.Object, 
                _commitmentRepository.Object, 
                _mockCurrentDateTime.Object);
        }

        [Test]
        public async Task ThenTheRequesIsValidated()
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
        public void ThenIfApprenticeHasStarted_ValidationFailureExceptionIsThrown_IfUlnHasChanged()
        {
            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(new Apprenticeship
                {
                    EmployerAccountId = 1,
                    ProviderId = 2,
                    ULN = " 123",
                    StartDate = DateTime.Now.AddMonths(-1),
                    EndDate = new DateTime(DateTime.Now.Year + 1, 5, 1),
                    Id = 3
                });

            var request = new CreateApprenticeshipUpdateCommand
                              {
                                  ApprenticeshipUpdate =
                                      new ApprenticeshipUpdate
                                          {
                                              Id = 5,
                                              ApprenticeshipId = 42,
                                              ULN = "1112223301"
                                          }
                              };

            //Act && Assert
            Func<Task> act = async () => await _handler.Handle(request);
            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public void ThenIfApprenticeHasStarted_ValidationFailureExceptionIsThrown_IfUpdated_StartDate()
        {
            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(new Apprenticeship
                {
                    EmployerAccountId = 1,
                    ProviderId = 2,
                    ULN = " 123",
                    StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1),
                    EndDate = new DateTime(DateTime.Now.Year + 1, 5, 1),
                    Id = 3
                });

            var request = new CreateApprenticeshipUpdateCommand
                              {
                                  ApprenticeshipUpdate =
                                      new ApprenticeshipUpdate
                                          {
                                              Id = 5,
                                              ApprenticeshipId = 42,
                                              StartDate = new DateTime(DateTime.Now.Year + 2, 5, 1)
                                          }
                              };

            //Act && Assert
            Func<Task> act = async () => await _handler.Handle(request);
            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public void ThenIfApprenticeHasStarted_ValidationFailureExceptionIsThrown_IfUpdatedEndDate()
        {
            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(new Apprenticeship
                {
                    EmployerAccountId = 1,
                    ProviderId = 2,
                    ULN = " 123",
                    StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1),
                    EndDate = new DateTime(DateTime.Now.Year + 1, 5, 1),
                    Id = 3
                });

            var request = new CreateApprenticeshipUpdateCommand
            {
                ApprenticeshipUpdate =
                                      new ApprenticeshipUpdate
                                      {
                                          Id = 5,
                                          ApprenticeshipId = 42,
                                          EndDate = new DateTime(DateTime.Now.Year + 2, 5, 1)
                                      }
            };

            //Act && Assert
            Func<Task> act = async () => await _handler.Handle(request);
            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public void ThenIfApprenticeHasStarted_And_DataLockExsist_ValidationFailureExceptionIsThrown_IfCostIsChanged()
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
                                y.First().EntityId == testCommitment.Id &&
                                y.First().ChangeType == CommitmentChangeType.EditedApprenticeship.ToString() &&
                                y.First().EntityType == "Commitment" &&
                                y.First().OriginalState == expectedOriginalCommitmentState &&
                                y.First().UpdatedByRole == command.Caller.CallerType.ToString() &&
                                y.First().UpdatedState == expectedOriginalCommitmentState &&
                                y.First().UserId == command.UserId &&
                                y.First().UpdatedByName == command.UserName)), Times.Once);

            _historyRepository.Verify(
                x =>
                    x.InsertHistory(
                        It.Is<IEnumerable<HistoryItem>>(
                            y =>
                                y.Last().EntityId == _existingApprenticeship.Id &&
                                y.Last().ChangeType == ApprenticeshipChangeType.Updated.ToString() &&
                                y.Last().EntityType == "Apprenticeship" &&
                                y.Last().OriginalState == expectedOriginalApprenticeshipState &&
                                y.Last().UpdatedByRole == command.Caller.CallerType.ToString() &&
                                y.Last().UpdatedState == expectedNewApprenticeshipState &&
                                y.Last().UserId == command.UserId &&
                                y.Last().UpdatedByName == command.UserName)), Times.Once);
        }
    }
}

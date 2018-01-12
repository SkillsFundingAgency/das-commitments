using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.TriageDataLock;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.TriageDataLock
{
    [TestFixture]
    public class WhenTriagingDataLock
    {
        private TriageDataLockCommandHandler _handler;
        private Mock<AbstractValidator<TriageDataLockCommand>> _validator;
        private Mock<IDataLockRepository> _dataLockRepository;
        private Mock<IApprenticeshipUpdateRepository> _apprenticeshipUpdateRepository;
        private Mock<IMessagePublisher> _messagePublisher;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;

        [SetUp]
        public void Arrange()
        {
            _validator = new Mock<AbstractValidator<TriageDataLockCommand>>();
            _validator.Setup(x => x.Validate(It.IsAny<TriageDataLockCommand>()))
                .Returns(() => new ValidationResult());

            _dataLockRepository = new Mock<IDataLockRepository>();
            _dataLockRepository.Setup(x => x.GetDataLock(It.IsAny<long>()))
                .ReturnsAsync(new DataLockStatus
                {
                    IlrEffectiveFromDate = new DateTime(2018, 5, 1),
                    ErrorCode = DataLockErrorCode.Dlock07
                });

            _dataLockRepository.Setup(x => x.UpdateDataLockTriageStatus(It.IsAny<long>(), It.IsAny<TriageStatus>()))
                .Returns(() => Task.FromResult(1L));
            _dataLockRepository.Setup(x => x.GetDataLocks(It.IsAny<long>(), It.IsAny<bool>())).ReturnsAsync(new List<DataLockStatus>());

            _apprenticeshipUpdateRepository = new Mock<IApprenticeshipUpdateRepository>();
            _apprenticeshipUpdateRepository.Setup(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(null);

            _messagePublisher = new Mock<IMessagePublisher>();

            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(It.IsAny<long>())).ReturnsAsync(new Apprenticeship());

            _handler = new TriageDataLockCommandHandler(
                _validator.Object,  
                _dataLockRepository.Object,
                _apprenticeshipUpdateRepository.Object,
                Mock.Of<ICommitmentsLogger>(),
                _messagePublisher.Object,
                _apprenticeshipRepository.Object);
        }

        [Test]
        public async Task ThenTheCommandIsValidated()
        {
            //Arrange
            var command = new TriageDataLockCommand();

            //Act
            await _handler.Handle(command);

            //Assert
            _validator.Verify(x => x.Validate(It.IsAny<TriageDataLockCommand>()), Times.Once);
        }


        [Test]
        public async Task ThenTheRepositoryIsCalledToRetrieveDataLock()
        {
            //Arrange
            var command = new TriageDataLockCommand();

            //Act
            await _handler.Handle(command);

            //Assert
            _dataLockRepository.Verify(x => x.GetDataLock(It.IsAny<long>()));
        }

        [Test]
        public async Task ThenTheRepositoryIsCalledToUpdateTriageStatus()
        {
            //Arrange
            var command = new TriageDataLockCommand
            {
                TriageStatus = TriageStatus.Change
            };

            //Act
            await _handler.Handle(command);

            //Assert
            _dataLockRepository.Verify(x => x.UpdateDataLockTriageStatus(
                It.IsAny<long>(), It.IsAny<TriageStatus>()),
                Times.Once);
        }

        [Test]
        public async Task ThenIfTriageStatusIsUnchangedThenNoUpdateIsMade()
        {
            //Arrange
            var command = new TriageDataLockCommand
            {
                TriageStatus = TriageStatus.Restart
            };

            _dataLockRepository.Setup(x => x.GetDataLock(It.IsAny<long>()))
                .ReturnsAsync(new DataLockStatus
                {
                    TriageStatus = TriageStatus.Restart
                });

            //Act
            await _handler.Handle(command);

            //Assert
            _dataLockRepository.Verify(x => x.UpdateDataLockTriageStatus(
                It.IsAny<long>(), It.IsAny<TriageStatus>()),
                Times.Never);
        }

        [Test]
        public void ThenIfAnOutstandingApprenticeshipUpdateExistsThenAnExceptionIsThrown()
        {
            //Arrange
            _apprenticeshipUpdateRepository.Setup(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(new ApprenticeshipUpdate());

            var command = new TriageDataLockCommand
            {
                TriageStatus = TriageStatus.Restart
            };

            //Act & Assert
            Func<Task> act = async () => await _handler.Handle(command);

            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public async Task ThenIfATriageStatusIsUpdatedToChangeAndTheApprenticeshipHasNoChangesRequiringApprovalAnEventIsSent()
        {
            //Arrange
            var command = new TriageDataLockCommand
            {
                ApprenticeshipId = 0,
                DataLockEventId = 456,
                TriageStatus = TriageStatus.Change
            };

            var existingDataLocks = new List<DataLockStatus>
            {
                new DataLockStatus { TriageStatus = TriageStatus.Unknown, IsResolved = false, Status = Status.Fail, ApprenticeshipId = command.ApprenticeshipId }
            };
            _dataLockRepository.Setup(x => x.GetDataLocks(command.ApprenticeshipId, false)).ReturnsAsync(existingDataLocks);

            var apprenticeship = new Apprenticeship { EmployerAccountId = 45453, ProviderId = 94443, Id = command.ApprenticeshipId };
            _apprenticeshipRepository.Setup(x => x.GetApprenticeship(command.ApprenticeshipId)).ReturnsAsync(apprenticeship);

            //Act
            await _handler.Handle(command);

            //Assert
            _messagePublisher.Verify(
                x =>
                    x.PublishAsync(
                        It.Is<DataLockTriageRequiresApproval>(
                            m => m.AccountId == apprenticeship.EmployerAccountId && m.ProviderId == apprenticeship.ProviderId && m.ApprenticeshipId == apprenticeship.Id)), Times.Once);
        }

        [Test]
        public async Task ThenIfATriageStatusIsUpdatedToChangeAndTheApprenticeshipAlreadyHasAChangeRequiringApprovalThenNoEventIsSent()
        {
            //Arrange
            var command = new TriageDataLockCommand
            {
                ApprenticeshipId = 0,
                DataLockEventId = 456,
                TriageStatus = TriageStatus.Change
            };

            var existingDataLocks = new List<DataLockStatus>
            {
                new DataLockStatus { TriageStatus = TriageStatus.Change, IsResolved = false, Status = Status.Fail, ApprenticeshipId = command.ApprenticeshipId }
            };
            _dataLockRepository.Setup(x => x.GetDataLocks(command.ApprenticeshipId, false)).ReturnsAsync(existingDataLocks);

            //Act
            await _handler.Handle(command);

            //Assert
            _messagePublisher.Verify(x => x.PublishAsync(It.IsAny<DataLockTriageRequiresApproval>()), Times.Never);
        }

        [Test]
        public async Task ThenIfATriageStatusIsNotChangeThenNoEventIsSent()
        {
            //Arrange
            var command = new TriageDataLockCommand
            {
                ApprenticeshipId = 0,
                DataLockEventId = 456,
                TriageStatus = TriageStatus.FixIlr
            };

            //Act
            await _handler.Handle(command);

            //Assert
            _messagePublisher.Verify(x => x.PublishAsync(It.IsAny<DataLockTriageRequiresApproval>()), Times.Never);
        }
    }
}

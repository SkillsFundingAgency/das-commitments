using System;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateDataLockTriageStatus;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateDataLock
{
    [TestFixture]
    public class WhenUpdatingDataLock
    {
        private UpdateDataLockTriageStatusCommandHandler _handler;
        private Mock<AbstractValidator<UpdateDataLockTriageStatusCommand>> _validator;
        private Mock<IDataLockRepository> _dataLockRepository;

        [SetUp]
        public void Arrange()
        {
            _validator = new Mock<AbstractValidator<UpdateDataLockTriageStatusCommand>>();
            _validator.Setup(x => x.Validate(It.IsAny<UpdateDataLockTriageStatusCommand>()))
                .Returns(() => new ValidationResult());

            _dataLockRepository = new Mock<IDataLockRepository>();
            _dataLockRepository.Setup(x => x.GetDataLock(It.IsAny<long>()))
                .ReturnsAsync(new DataLockStatus());

            _dataLockRepository.Setup(x => x.UpdateDataLockTriageStatus(It.IsAny<long>(), It.IsAny<TriageStatus>(), It.IsAny<ApprenticeshipUpdate>()))
                .Returns(() => Task.FromResult(1L));

            _handler = new UpdateDataLockTriageStatusCommandHandler(_validator.Object, _dataLockRepository.Object);
        }

        [Test]
        public async Task ThenTheCommandIsValidated()
        {
            //Arrange
            var command = new UpdateDataLockTriageStatusCommand();

            //Act
            await _handler.Handle(command);

            //Assert
            _validator.Verify(x => x.Validate(It.IsAny<UpdateDataLockTriageStatusCommand>()), Times.Once);
        }


        [Test]
        public async Task ThenTheRepositoryIsCalledToRetrieveDataLock()
        {
            //Arrange
            var command = new UpdateDataLockTriageStatusCommand();

            //Act
            await _handler.Handle(command);

            //Assert
            _dataLockRepository.Verify(x => x.GetDataLock(It.IsAny<long>()));
        }

        [Test]
        public async Task ThenTheRepositoryIsCalledToUpdateTriageStatus()
        {
            //Arrange
            var command = new UpdateDataLockTriageStatusCommand
            {
                TriageStatus = Api.Types.DataLock.Types.TriageStatus.Restart
            };

            //Act
            await _handler.Handle(command);

            //Assert
            _dataLockRepository.Verify(x => x.UpdateDataLockTriageStatus(
                It.IsAny<long>(), It.IsAny<TriageStatus>(), It.IsAny<ApprenticeshipUpdate>()),
                Times.Once);
        }

        [Test]
        public async Task ThenIfTriageStatusIsUnchangedThenNoUpdateIsMade()
        {
            //Arrange
            var command = new UpdateDataLockTriageStatusCommand
            {
                TriageStatus = Api.Types.DataLock.Types.TriageStatus.Restart
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
                It.IsAny<long>(), It.IsAny<TriageStatus>(), It.IsAny<ApprenticeshipUpdate>()),
                Times.Never);
        }

        [Test]
        public async Task ThenIfTriageStatusIsChangeAChangeOfCircumstancesIsCreated()
        {
            //Arrange
            var command = new UpdateDataLockTriageStatusCommand
            {
                ApprenticeshipId = 1,
                UserId = "USER",
                DataLockEventId = 2,
                TriageStatus = Api.Types.DataLock.Types.TriageStatus.Unknown
            };

            _dataLockRepository.Setup(x => x.GetDataLock(It.IsAny<long>()))
                .ReturnsAsync(new DataLockStatus
                {
                    ApprenticeshipId = 1,
                    DataLockEventId = 2,
                    TriageStatus = TriageStatus.Change
                });

            //Act
            await _handler.Handle(command);

            //Assert
            _dataLockRepository.Verify(x => x.UpdateDataLockTriageStatus(
                It.IsAny<long>(), It.IsAny<TriageStatus>(),
                It.Is<ApprenticeshipUpdate>(
                    u => u.ApprenticeshipId == 1
                    && u.Originator == Originator.Provider
                    //todo: finish these assertions
                )),
                Times.Once);
        }
    }
}

using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;

using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateDataLockTriageStatus;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateDataLock
{
    [TestFixture]
    public class WhenUpdatingDataLock
    {
        private UpdateDataLockTriageStatusCommandHandler _handler;
        private Mock<AbstractValidator<UpdateDataLockTriageStatusCommand>> _validator;
        private Mock<IDataLockRepository> _dataLockRepository;
        private Mock<IApprenticeshipUpdateRepository> _apprenticeshipUpdateRepository;

        [SetUp]
        public void Arrange()
        {
            _validator = new Mock<AbstractValidator<UpdateDataLockTriageStatusCommand>>();
            _validator.Setup(x => x.Validate(It.IsAny<UpdateDataLockTriageStatusCommand>()))
                .Returns(() => new ValidationResult());

            _dataLockRepository = new Mock<IDataLockRepository>();
            _dataLockRepository.Setup(x => x.GetDataLock(It.IsAny<long>()))
                .ReturnsAsync(new DataLockStatus
                {
                    IlrEffectiveFromDate = new DateTime(2018, 5, 1),
                    ErrorCode = DataLockErrorCode.Dlock03
                });

            _dataLockRepository.Setup(x => x.UpdateDataLockTriageStatus(It.IsAny<long>(), It.IsAny<TriageStatus>()))
                .Returns(() => Task.FromResult(1L));

            _apprenticeshipUpdateRepository = new Mock<IApprenticeshipUpdateRepository>();
            _apprenticeshipUpdateRepository.Setup(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(null);

            _handler = new UpdateDataLockTriageStatusCommandHandler(
                _validator.Object,  
                _dataLockRepository.Object,
                _apprenticeshipUpdateRepository.Object,
                Mock.Of<ICommitmentsLogger>());
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
                It.IsAny<long>(), It.IsAny<TriageStatus>()),
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
                It.IsAny<long>(), It.IsAny<TriageStatus>()),
                Times.Never);
        }

        [Test]
        public void ThenIfAnOutstandingApprenticeshipUpdateExistsThenAnExceptionIsThrown()
        {
            //Arrange
            _apprenticeshipUpdateRepository.Setup(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(new ApprenticeshipUpdate());

            var command = new UpdateDataLockTriageStatusCommand
            {
                TriageStatus = Api.Types.DataLock.Types.TriageStatus.Restart
            };

            //Act & Assert
            Func<Task> act = async () => await _handler.Handle(command);

            act.ShouldThrow<ValidationException>();
        }
    }
}

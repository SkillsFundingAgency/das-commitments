using System;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateDataLockTriageStatus;
using SFA.DAS.Commitments.Domain.Data;
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
            _dataLockRepository.Setup(x => x.UpdateDataLockTriageStatus(It.IsAny<long>(), It.IsAny<TriageStatus>()))
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
        public async Task ThenTheRepositoryIsCalled()
        {
            //Arrange
            var command = new UpdateDataLockTriageStatusCommand();

            //Act
            await _handler.Handle(command);

            //Assert
            _dataLockRepository.Verify(x => x.UpdateDataLockTriageStatus(
                It.IsAny<long>(), It.IsAny<TriageStatus>()));
        }

        [Test]
        public async Task ThenIfTriageStatusIsChangeThenCreateAChangeOfCircumstances()
        {
            throw new NotImplementedException();
        }
    }
}

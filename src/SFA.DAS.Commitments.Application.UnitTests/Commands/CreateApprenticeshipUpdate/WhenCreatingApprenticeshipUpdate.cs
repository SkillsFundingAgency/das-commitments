using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate;
using SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateApprenticeshipUpdate
{
    [TestFixture()]
    public class WhenCreatingApprenticeshipUpdate
    {
        private Mock<CreateApprenticeshipUpdateValidator> _validator;

        private Mock<IApprenticeshipUpdateRepository> _repository;

        private CreateApprenticeshipUpdateCommandHandler _handler;

        [SetUp]
        public void Arrange()
        {
            _validator = new Mock<CreateApprenticeshipUpdateValidator>();
            _validator.Setup(x => x.Validate(It.IsAny<CreateApprenticeshipUpdateCommand>()))
                .Returns(() => new ValidationResult());

            _repository = new Mock<IApprenticeshipUpdateRepository>();
            _repository.Setup(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(null);

            _repository.Setup(x => x.CreateApprenticeshipUpdate(It.IsAny<ApprenticeshipUpdate>()))
                .Returns(() => Task.FromResult(new Unit()));

            _handler = new CreateApprenticeshipUpdateCommandHandler(_validator.Object, _repository.Object, Mock.Of<ICommitmentsLogger>());
        }

        [Test]
        public async Task ThenTheRequesIsValidated()
        {
            //Arrange
            var command = new CreateApprenticeshipUpdateCommand
            {
                ApprenticeshipUpdate = new PendingApprenticeshipUpdatePlaceholder()
            };

            //Act
            await _handler.Handle(command);

            //Assert
            _validator.Verify(x => x.Validate(It.IsAny<CreateApprenticeshipUpdateCommand>()), Times.Once);
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
                ApprenticeshipUpdate = new PendingApprenticeshipUpdatePlaceholder()
            };

            //Act
            await _handler.Handle(command);

            //Assert
            _repository.Verify(x => x.CreateApprenticeshipUpdate(It.IsAny<ApprenticeshipUpdate>()), Times.Once);
        }

        [Test]
        public void ThenIfTheApprenticeshipAlreadyHasAPendingUpdateThenAnExceptionIsThrown()
        {
            //Arrange
            _repository.Setup(x => x.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(new ApprenticeshipUpdate());

            var command = new CreateApprenticeshipUpdateCommand
            {
                ApprenticeshipUpdate = new PendingApprenticeshipUpdatePlaceholder()
            };

            //Act && Assert
            Func<Task> act = async () => await _handler.Handle(command);
            act.ShouldThrow<InvalidOperationException>();

            //Assert
            _repository.Verify(x => x.CreateApprenticeshipUpdate(It.IsAny<ApprenticeshipUpdate>()), Times.Never);
        }
    }
}

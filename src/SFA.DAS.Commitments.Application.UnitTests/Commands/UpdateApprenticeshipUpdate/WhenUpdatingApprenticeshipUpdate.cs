using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;

using MediatR;

using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Types.Validation;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipUpdate;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipUpdate
{
    [TestFixture]
    public class WhenUpdatingApprenticeshipUpdate
    {
        UpdateApprenticeshipUpdateCommandHandler _sut;

        private Mock<AbstractValidator<UpdateApprenticeshipUpdateCommand>> _validator;
        private Mock<IApprenticeshipUpdateRepository> _repository;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;

        private Mock<IMediator> _mediator;

        [SetUp]
        public void SetUp()
        {
            _validator = new Mock<AbstractValidator<UpdateApprenticeshipUpdateCommand>>();
            _repository = new Mock<IApprenticeshipUpdateRepository>();
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _mediator = new Mock<IMediator>();

            _validator.Setup(x => x.Validate(It.IsAny<UpdateApprenticeshipUpdateCommand>()))
                .Returns(() => new ValidationResult());

            _apprenticeshipRepository.Setup(m => m.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(
                    new Apprenticeship
                    {
                        EmployerAccountId = 555,
                        ProviderId = 666,
                        ULN = " 123",
                        StartDate = new DateTime(2018, 5, 1),
                        EndDate = new DateTime(2018, 9, 1),
                        Id = 3
                    });

            _mediator.Setup(m => m.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>()))
                .ReturnsAsync(new GetOverlappingApprenticeshipsResponse { Data = new List<OverlappingApprenticeship>() });

            _repository.Setup(m => m.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(new ApprenticeshipUpdate { ApprenticeshipId = 5, Id = 42 });

            _sut = new UpdateApprenticeshipUpdateCommandHandler(_validator.Object, _repository.Object, _apprenticeshipRepository.Object, _mediator.Object, new UpdateApprenticeshipUpdateMapper());
        }

        [Test]
        public async Task ThenTheRequestIsValidated()
        {
            await _sut.Handle(new UpdateApprenticeshipUpdateCommand {Caller = new Caller(555, CallerType.Employer)});

            _validator.Verify(m => m.Validate(It.IsAny<UpdateApprenticeshipUpdateCommand>()), Times.Once);
        }

        [Test]
        public void ThenIfTheEmployerFailsAuthorisationThenAnExceptionIsThrown()
        {
            var command = new UpdateApprenticeshipUpdateCommand
            {
                Caller = new Caller(444, CallerType.Employer)
            };

            Func<Task> act = async () => await _sut.Handle(command);
            act.ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public void ThenIfTheProviderFailsAuthorisationThenAnExceptionIsThrown()
        {
            var command = new UpdateApprenticeshipUpdateCommand
            {
                Caller = new Caller(444, CallerType.Provider)
            };

            Func<Task> act = async () => await _sut.Handle(command);
            act.ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public void ThenIfTheRequestIsInvalidThenAValidationFailureExceptionIsThrown()
        {
            _validator.Setup(x => x.Validate(It.IsAny<UpdateApprenticeshipUpdateCommand>()))
                .Returns(() =>
                        new ValidationResult(new List<ValidationFailure>
                        {
                            new ValidationFailure("Error", "Error Message")
                        }));

            var command = new UpdateApprenticeshipUpdateCommand { Caller = new Caller(555, CallerType.Employer) } ;

            Func<Task> act = async () => await _sut.Handle(command);
            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public async Task ThenTheApprenticeshipWillBeUpdatedIfApproved()
        {
            const long ApprenticeshipId = 5L;
            const string UserId = "user123";

            await _sut.Handle(
                new UpdateApprenticeshipUpdateCommand
                {
                    ApprenticeshipId = ApprenticeshipId,
                    UpdateStatus = ApprenticeshipUpdateStatus.Approved,
                    UserId = UserId,
                    Caller = new Caller(555, CallerType.Employer)
                });

            _repository.Verify(m => m.ApproveApprenticeshipUpdate(42, UserId, It.IsAny<Apprenticeship>(), It.IsAny<Caller>()), Times.Once);
            _repository.Verify(m => m.RejectApprenticeshipUpdate(42, UserId), Times.Never);
            _repository.Verify(m => m.UndoApprenticeshipUpdate(42, UserId), Times.Never);
        }

        [Test]
        public async Task ThenTheApprenticeshipWillBeUpdatedIfRejected()
        {
            const long ApprenticeshipId = 5L;
            const string UserId = "user123";

            await _sut.Handle(
                new UpdateApprenticeshipUpdateCommand
                {
                    ApprenticeshipId = ApprenticeshipId,
                    UpdateStatus = ApprenticeshipUpdateStatus.Rejected,
                    UserId = UserId,
                    Caller = new Caller(555, CallerType.Employer)
                });

            _repository.Verify(m => m.ApproveApprenticeshipUpdate(42, UserId, It.IsAny<Apprenticeship>(), It.IsAny<Caller>()), Times.Never);
            _repository.Verify(m => m.RejectApprenticeshipUpdate(42, UserId), Times.Once);
            _repository.Verify(m => m.UndoApprenticeshipUpdate(42, UserId), Times.Never);
        }

        [Test]
        public async Task ThenTheApprenticeshipWillBeUpdatedIfUndo()
        {
            const long ApprenticeshipId = 5L;
            const string UserId = "user123";

            await _sut.Handle(
                new UpdateApprenticeshipUpdateCommand
                {
                    ApprenticeshipId = ApprenticeshipId,
                    UpdateStatus = ApprenticeshipUpdateStatus.Deleted,
                    UserId = UserId,
                    Caller = new Caller(555, CallerType.Employer)
                });

            _repository.Verify(m => m.ApproveApprenticeshipUpdate(42, UserId, It.IsAny<Apprenticeship>(), It.IsAny<Caller>()), Times.Never);
            _repository.Verify(m => m.RejectApprenticeshipUpdate(42, UserId), Times.Never);
            _repository.Verify(m => m.UndoApprenticeshipUpdate(42, UserId), Times.Once);
        }

        [Test]
        public async Task ThenThereMustBeAPendingUpdate()
        {
            var command = new UpdateApprenticeshipUpdateCommand
            {
                Caller = new Caller(666, CallerType.Provider),
                ApprenticeshipId = 5L,
                UserId = "user123"
            };

            _repository.Setup(m => m.GetPendingApprenticeshipUpdate(5L))
                .ReturnsAsync(null);

            Func<Task> act = async () => await _sut.Handle(command);
            act.ShouldThrow<ValidationException>().WithMessage("No existing apprenticeship update pending for apprenticeship 5");
        }

        [Test]
        public async Task ThenTheOverlappingIsNotChackedIfReject()
        {
            var command = new UpdateApprenticeshipUpdateCommand
            {
                Caller = new Caller(666, CallerType.Provider),
                ApprenticeshipId = 5L,
                UserId = "user123",
                UpdateStatus = ApprenticeshipUpdateStatus.Rejected
            };

            //Act
            await _sut.Handle(command);

            //Arrange
            _mediator.Verify(x => x.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>()), Times.Never);
        }

        [Test]
        public async Task ThenTheOverlappingIsNotChackedIfUndo()
        {
            var command = new UpdateApprenticeshipUpdateCommand
            {
                Caller = new Caller(666, CallerType.Provider),
                ApprenticeshipId = 5L,
                UserId = "user123",
                UpdateStatus = ApprenticeshipUpdateStatus.Deleted
            };

            //Act
            await _sut.Handle(command);

            //Arrange
            _mediator.Verify(x => x.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>()), Times.Never);
        }

        [Test]
        public async Task ThenThereMustBeNoOverlapping()
        {
            var command = new UpdateApprenticeshipUpdateCommand
            {
                Caller = new Caller(666, CallerType.Provider),
                ApprenticeshipId = 5L,
                UserId = "user123",
                UpdateStatus = ApprenticeshipUpdateStatus.Approved
            };

            _mediator.Setup(m => m.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>()))
                .ReturnsAsync(
                    new GetOverlappingApprenticeshipsResponse {
                        Data = new List<OverlappingApprenticeship>
                        {
                            new OverlappingApprenticeship()
                        }
                    }
                );

            Func<Task> act = async () => await _sut.Handle(command);
            act.ShouldThrow<ValidationException>().WithMessage("Unable to create ApprenticeshipUpdate due to overlapping apprenticeship");
        }
    }
}

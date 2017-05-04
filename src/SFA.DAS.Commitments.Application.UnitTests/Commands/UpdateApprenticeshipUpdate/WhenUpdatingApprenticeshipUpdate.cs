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
using SFA.DAS.Commitments.Domain.Interfaces;

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
        private Mock<IApprenticeshipEvents> _apprenticeshipEvents;
        private Mock<ICommitmentRepository> _commitment;

        private DateTime _apprenticeshipStartDate;

        private DateTime _updateCreadtedOn;

        [SetUp]
        public void SetUp()
        {
            _validator = new Mock<AbstractValidator<UpdateApprenticeshipUpdateCommand>>();
            _repository = new Mock<IApprenticeshipUpdateRepository>();
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _mediator = new Mock<IMediator>();
            _apprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _commitment = new Mock<ICommitmentRepository>();

            _updateCreadtedOn = DateTime.Now.AddDays(-2);
            _validator.Setup(x => x.Validate(It.IsAny<UpdateApprenticeshipUpdateCommand>()))
                .Returns(() => new ValidationResult());

            _apprenticeshipStartDate = DateTime.Now.AddYears(2);
            _apprenticeshipRepository.Setup(m => m.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(
                    new Apprenticeship
                    {
                        FirstName = "Original first name",
                        EmployerAccountId = 555,
                        ProviderId = 666,
                        ULN = " 123",
                        StartDate = _apprenticeshipStartDate,
                        EndDate = _apprenticeshipStartDate.AddYears(2),
                        Id = 3
                    });

            _mediator.Setup(m => m.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>()))
                .ReturnsAsync(new GetOverlappingApprenticeshipsResponse { Data = new List<OverlappingApprenticeship>() });

            _repository.Setup(m => m.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(new ApprenticeshipUpdate { ApprenticeshipId = 5, Id = 42, EffectiveFromDate = _apprenticeshipStartDate, CreatedOn = _updateCreadtedOn});

            _sut = new UpdateApprenticeshipUpdateCommandHandler(
                _validator.Object,
                _repository.Object,
                _apprenticeshipRepository.Object,
                _mediator.Object,
                new UpdateApprenticeshipUpdateMapper(),
                _apprenticeshipEvents.Object,
                _commitment.Object
                );
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

            _repository.Verify(m => m.ApproveApprenticeshipUpdate(new ApprenticeshipUpdate {Id = 42}, UserId, It.IsAny<Apprenticeship>(), It.IsAny<Caller>()), Times.Once);
            _repository.Verify(m => m.RejectApprenticeshipUpdate(42, UserId), Times.Never);
            _repository.Verify(m => m.UndoApprenticeshipUpdate(42, UserId), Times.Never);
            _apprenticeshipEvents.Verify(x => x.PublishEvent(It.IsAny<Commitment>(), It.IsAny<Apprenticeship>(), It.IsAny<string>(), null, null), Times.Never);
            _apprenticeshipEvents.Verify(x => x.PublishEvent(It.IsAny<Commitment>(), It.IsAny<Apprenticeship>(), It.IsAny<string>(), null, _apprenticeshipStartDate.AddDays(-1)), Times.Once);
            _apprenticeshipEvents.Verify(x => x.PublishEvent(It.IsAny<Commitment>(), It.IsAny<Apprenticeship>(), It.IsAny<string>(), _apprenticeshipStartDate, null), Times.Once);
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

            _repository.Verify(m => m.ApproveApprenticeshipUpdate(new ApprenticeshipUpdate { Id = 42 }, UserId, It.IsAny<Apprenticeship>(), It.IsAny<Caller>()), Times.Never);
            _repository.Verify(m => m.RejectApprenticeshipUpdate(42, UserId), Times.Once);
            _repository.Verify(m => m.UndoApprenticeshipUpdate(42, UserId), Times.Never);
            _apprenticeshipEvents.Verify(x => x.PublishEvent(It.IsAny<Commitment>(), It.IsAny<Apprenticeship>(), It.IsAny<string>(), null, null), Times.Never);
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

            _repository.Verify(m => m.ApproveApprenticeshipUpdate(new ApprenticeshipUpdate { Id = 42 }, UserId, It.IsAny<Apprenticeship>(), It.IsAny<Caller>()), Times.Never);
            _repository.Verify(m => m.RejectApprenticeshipUpdate(42, UserId), Times.Never);
            _repository.Verify(m => m.UndoApprenticeshipUpdate(42, UserId), Times.Once);
            _apprenticeshipEvents.Verify(x => x.PublishEvent(It.IsAny<Commitment>(), It.IsAny<Apprenticeship>(), It.IsAny<string>(), null, null), Times.Never);
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
            _apprenticeshipEvents.Verify(x => x.PublishEvent(It.IsAny<Commitment>(), It.IsAny<Apprenticeship>(), It.IsAny<string>(), null, null), Times.Never);
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

        // ---------------------------------------------------

        [Test]
        public async Task ThenCallEventsWhen_Started()
        {
            var createdOn = _apprenticeshipStartDate.AddMonths(2);

            _repository.Setup(m => m.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(new ApprenticeshipUpdate {
                    ApprenticeshipId = 5,
                    Id = 42,
                    FirstName = "Updated first name",
                    EffectiveFromDate = createdOn,
                    CreatedOn =  createdOn});

            await _sut.Handle(
                new UpdateApprenticeshipUpdateCommand
                {
                    ApprenticeshipId = 5L,
                    UpdateStatus = ApprenticeshipUpdateStatus.Approved,
                    UserId = "user123",
                    Caller = new Caller(555, CallerType.Employer)
                });

            // Old apprenticeship
            _apprenticeshipEvents.Verify(x => x.PublishEvent(
                It.IsAny<Commitment>(), 
                It.Is<Apprenticeship>(m => 
                       m.StartDate == _apprenticeshipStartDate
                    && m.EndDate == _apprenticeshipStartDate.AddYears(2)
                    && m.FirstName == "Original first name"), 
                It.IsAny<string>(), null, createdOn.AddDays(-1)), Times.Once);

            // New apprenticeship
            _apprenticeshipEvents.Verify(x => x.PublishEvent(
                It.IsAny<Commitment>(),
                It.Is<Apprenticeship>(m => 
                       m.StartDate == _apprenticeshipStartDate
                    && m.EndDate == _apprenticeshipStartDate.AddYears(2)
                    && m.FirstName == "Updated first name" ),
                It.IsAny<string>(), createdOn, null), Times.Exactly(1));
        }

        [Test]
        public async Task ThenCallEventsWhen_WaitingToStart()
        {
            var createdOn = _apprenticeshipStartDate.AddMonths(-2);

            _repository.Setup(m => m.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(new ApprenticeshipUpdate
                {
                    ApprenticeshipId = 5,
                    Id = 42,
                    FirstName = "Updated first name",
                    EffectiveFromDate = _apprenticeshipStartDate,
                    CreatedOn = createdOn
                });

            await _sut.Handle(
                new UpdateApprenticeshipUpdateCommand
                {
                    ApprenticeshipId = 5L,
                    UpdateStatus = ApprenticeshipUpdateStatus.Approved,
                    UserId = "user123",
                    Caller = new Caller(555, CallerType.Employer)
                });

            _repository.Verify(m => m.ApproveApprenticeshipUpdate(It.IsAny<ApprenticeshipUpdate>(), It.IsAny<string>(), 
                It.IsAny<Apprenticeship>(), It.IsAny<Caller>()), Times.Once);

            // Old apprenticeship
            _apprenticeshipEvents.Verify(x => x.PublishEvent(
                It.IsAny<Commitment>(),
                It.Is<Apprenticeship>(m =>
                       m.StartDate == _apprenticeshipStartDate
                    && m.EndDate == _apprenticeshipStartDate.AddYears(2)
                    && m.FirstName == "Original first name"),
                It.IsAny<string>(), null, _apprenticeshipStartDate.AddDays(-1)), Times.Once);

            // New apprenticeship
            _apprenticeshipEvents.Verify(x => x.PublishEvent(
                It.IsAny<Commitment>(),
                It.Is<Apprenticeship>(m =>
                       m.StartDate == _apprenticeshipStartDate
                    && m.EndDate == _apprenticeshipStartDate.AddYears(2)
                    && m.FirstName == "Updated first name"),
                It.IsAny<string>(), _apprenticeshipStartDate, null), Times.Once);
        }

        [Test]
        public async Task ThenCallEventsWhen_WaitingToStart_AndStartEndDateUpdated()
        {
            var createdOn = _apprenticeshipStartDate.AddMonths(-2);
            var newStartDate = _apprenticeshipStartDate.AddDays(5);
            var newEndDate = _apprenticeshipStartDate.AddDays(10);
            _repository.Setup(m => m.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(new ApprenticeshipUpdate
                {
                    ApprenticeshipId = 5,
                    Id = 42,
                    FirstName = "Updated first name",
                    EffectiveFromDate = newStartDate,
                    CreatedOn = createdOn,
                    StartDate = newStartDate,
                    EndDate = newEndDate
                });

            await _sut.Handle(
                new UpdateApprenticeshipUpdateCommand
                {
                    ApprenticeshipId = 5L,
                    UpdateStatus = ApprenticeshipUpdateStatus.Approved,
                    UserId = "user123",
                    Caller = new Caller(555, CallerType.Employer)
                });

            _repository.Verify(m => m.ApproveApprenticeshipUpdate(It.IsAny<ApprenticeshipUpdate>(), It.IsAny<string>(),
                It.Is<Apprenticeship>(p =>
                       p.StartDate == newStartDate
                    && p.EndDate == newEndDate
                    && p.FirstName == "Updated first name"
                    ),
                It.IsAny<Caller>()), Times.Once);

            // Old apprenticeship
            _apprenticeshipEvents.Verify(x => x.PublishEvent(
                It.IsAny<Commitment>(),
                It.Is<Apprenticeship>(m =>
                       m.StartDate == _apprenticeshipStartDate
                    && m.EndDate == _apprenticeshipStartDate.AddYears(2)
                    && m.FirstName == "Original first name"),
                It.IsAny<string>(),
                null,
                newStartDate.AddDays(-1)),
                Times.Once);

            // New apprenticeship
            _apprenticeshipEvents.Verify(x => x.PublishEvent(
                It.IsAny<Commitment>(),
                It.Is<Apprenticeship>(m =>
                       m.StartDate == newStartDate // Keep old start date
                    && m.EndDate == newEndDate // Set new end date from update reqest
                    && m.FirstName == "Updated first name"),
                It.IsAny<string>(), newStartDate, null), Times.Exactly(1));
        }

        /*
         * 
         * 
            When confirming a change to an approved apprenticeship
            When the change was requested:
            if the apprenticeship was "waiting to start" then the change effective date is the start date of the apprenticeship

            if the apprenticeship was "live" then the change effective date is the date that the change was requested
            
        The following occurs when a change of circumstances is approved:

            using the "old" apprenticeship data, an event is emitted with the effective to date set to the change effective date minus 1 day
            apprenticeship data is updated with the requested field changes applied
            using the "new" apprenticeship data, an event is emitted with the effective from date set to the change effective date
            (info) Note: a future iteration will capture a change effective date from the user which represents when the change of circumstances should be applied from - this will clearly identify the user's intent
            When stopping an apprenticeship

         */
    }
}
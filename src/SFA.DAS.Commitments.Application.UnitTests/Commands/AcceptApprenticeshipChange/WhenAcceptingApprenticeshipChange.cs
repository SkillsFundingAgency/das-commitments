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

using SFA.DAS.Commitments.Application.Commands.AcceptApprenticeshipChange;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.AcceptApprenticeshipChange
{
    [TestFixture]
    public class WhenAcceptingApprenticeshipChange
    {
        private AcceptApprenticeshipChangeCommandHandler _sut;
        private Mock<AbstractValidator<AcceptApprenticeshipChangeCommand>> _validator;

        private Mock<IApprenticeshipUpdateRepository> _repository;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private Mock<IMediator> _mediator;
        private Mock<IApprenticeshipEvents> _apprenticeshipEvents;
        private Mock<ICommitmentRepository> _commitment;
        private Mock<IHistoryRepository> _historyRepository;
        private Mock<ICurrentDateTime> _currentDateTime;

        private DateTime _apprenticeshipStartDate;
        private DateTime _effectiveDate;

        private DateTime _updateCreadtedOn;
        private Apprenticeship _apprenticeship;

        [SetUp]
        public void Arrange()
        {
            _validator = new Mock<AbstractValidator<AcceptApprenticeshipChangeCommand>>();
            _repository = new Mock<IApprenticeshipUpdateRepository>();
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _mediator = new Mock<IMediator>();
            _apprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _commitment = new Mock<ICommitmentRepository>();
            _historyRepository = new Mock<IHistoryRepository>();

            _updateCreadtedOn = DateTime.Now.AddDays(-2);
            _effectiveDate = DateTime.Now.AddDays(-2);
            _validator.Setup(x => x.Validate(It.IsAny<AcceptApprenticeshipChangeCommand>())).Returns(() => new ValidationResult());

            _apprenticeshipStartDate = DateTime.Now.AddYears(2);
            _apprenticeship = new Apprenticeship
            {
                FirstName = "Original first name",
                EmployerAccountId = 555,
                ProviderId = 666,
                ULN = " 123",
                StartDate = _apprenticeshipStartDate,
                EndDate = _apprenticeshipStartDate.AddYears(2),
                Id = 3
            };
            _apprenticeshipRepository.Setup(m => m.GetApprenticeship(It.IsAny<long>())).ReturnsAsync(_apprenticeship);

            _mediator.Setup(m => m.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>())).ReturnsAsync(new GetOverlappingApprenticeshipsResponse { Data = new List<ApprenticeshipResult>() });

            _repository.Setup(m => m.GetPendingApprenticeshipUpdate(It.IsAny<long>())).ReturnsAsync(new ApprenticeshipUpdate { ApprenticeshipId = 5, Id = 42, CreatedOn = _updateCreadtedOn, EffectiveFromDate = _effectiveDate });
            _commitment.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(new Commitment());

            _sut = new AcceptApprenticeshipChangeCommandHandler(
                _validator.Object,
                _repository.Object,
                _apprenticeshipRepository.Object,
                _mediator.Object,
                new AcceptApprenticeshipChangeMapper(Mock.Of<ICurrentDateTime>()),
                _apprenticeshipEvents.Object,
                _commitment.Object,
                _historyRepository.Object
                );
        }

        [Test]
        public async Task ThenTheRequestIsValidated()
        {
            await _sut.Handle(new AcceptApprenticeshipChangeCommand { Caller = new Caller(555, CallerType.Employer) });

            _validator.Verify(m => m.Validate(It.IsAny<AcceptApprenticeshipChangeCommand>()), Times.Once);
        }

        [Test]
        public void ThenIfTheEmployerFailsAuthorisationThenAnExceptionIsThrown()
        {
            var command = new AcceptApprenticeshipChangeCommand
            {
                Caller = new Caller(444, CallerType.Employer)
            };

            Func<Task> act = async () => await _sut.Handle(command);
            act.ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public void ThenIfTheProviderFailsAuthorisationThenAnExceptionIsThrown()
        {
            var command = new AcceptApprenticeshipChangeCommand
            {
                Caller = new Caller(444, CallerType.Provider)
            };

            Func<Task> act = async () => await _sut.Handle(command);
            act.ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public void ThenIfTheRequestIsInvalidThenAValidationFailureExceptionIsThrown()
        {
            _validator.Setup(x => x.Validate(It.IsAny<AcceptApprenticeshipChangeCommand>()))
                .Returns(() =>
                        new ValidationResult(new List<ValidationFailure>
                        {
                            new ValidationFailure("Error", "Error Message")
                        }));

            var command = new AcceptApprenticeshipChangeCommand { Caller = new Caller(555, CallerType.Employer) };

            Func<Task> act = async () => await _sut.Handle(command);
            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public async Task ThenTheApprenticeshipWillBeUpdatedIfApproved()
        {
            const long ApprenticeshipId = 5L;
            const string UserId = "user123";

            await _sut.Handle(
                new AcceptApprenticeshipChangeCommand
                {
                    ApprenticeshipId = ApprenticeshipId,
                    UserId = UserId,
                    Caller = new Caller(555, CallerType.Employer)
                });

            _repository.Verify(m => m.ApproveApprenticeshipUpdate(It.Is<ApprenticeshipUpdate>(u => u.Id == 42), UserId, It.IsAny<Apprenticeship>(), It.IsAny<Caller>()), Times.Once);
            _repository.Verify(m => m.RejectApprenticeshipUpdate(It.Is<ApprenticeshipUpdate>(u => u.Id == 42), UserId), Times.Never);
            _repository.Verify(m => m.UndoApprenticeshipUpdate(It.Is<ApprenticeshipUpdate>(u => u.Id == 42), UserId), Times.Never);
            _apprenticeshipEvents.Verify(x => x.PublishEvent(It.IsAny<Commitment>(), It.IsAny<Apprenticeship>(), It.IsAny<string>(), null, null), Times.Never);
            _apprenticeshipEvents.Verify(x => x.PublishEvent(It.IsAny<Commitment>(), It.IsAny<Apprenticeship>(), It.IsAny<string>(), _effectiveDate, null), Times.Once);
        }

        [Test]
        public void ThenThereMustBeAPendingUpdate()
        {
            var command = new AcceptApprenticeshipChangeCommand
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
        public async Task ThenCallEventsWhen_Started()
        {
            var createdOn = _apprenticeshipStartDate.AddMonths(2);

            _repository.Setup(m => m.GetPendingApprenticeshipUpdate(It.IsAny<long>()))
                .ReturnsAsync(new ApprenticeshipUpdate
                {
                    ApprenticeshipId = 5,
                    Id = 42,
                    FirstName = "Updated first name",
                    EffectiveFromDate = createdOn,
                    CreatedOn = createdOn
                });

            await _sut.Handle(
                new AcceptApprenticeshipChangeCommand
                {
                    ApprenticeshipId = 5L,
                    UserId = "user123",
                    Caller = new Caller(555, CallerType.Employer)
                });

            // New apprenticeship
            _apprenticeshipEvents.Verify(x => x.PublishEvent(
                It.IsAny<Commitment>(),
                It.Is<Apprenticeship>(m =>
                       m.StartDate == _apprenticeshipStartDate
                    && m.EndDate == _apprenticeshipStartDate.AddYears(2)
                    && m.FirstName == "Updated first name"),
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
                new AcceptApprenticeshipChangeCommand
                {
                    ApprenticeshipId = 5L,
                    UserId = "user123",
                    Caller = new Caller(555, CallerType.Employer)
                });

            _repository.Verify(m => m.ApproveApprenticeshipUpdate(It.IsAny<ApprenticeshipUpdate>(), It.IsAny<string>(),
                It.IsAny<Apprenticeship>(), It.IsAny<Caller>()), Times.Once);

            // Old apprenticeship
            // As change is effective on start date, no termination event should be emitted
            _apprenticeshipEvents.Verify(x => x.PublishEvent(
                It.IsAny<Commitment>(),
                It.Is<Apprenticeship>(m =>
                       m.StartDate == _apprenticeshipStartDate
                    && m.EndDate == _apprenticeshipStartDate.AddYears(2)
                    && m.FirstName == "Original first name"),
                It.IsAny<string>(), null, _apprenticeshipStartDate.AddDays(-1)), Times.Never);

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
                new AcceptApprenticeshipChangeCommand
                {
                    ApprenticeshipId = 5L,
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

            // New apprenticeship
            _apprenticeshipEvents.Verify(x => x.PublishEvent(
                It.IsAny<Commitment>(),
                It.Is<Apprenticeship>(m =>
                       m.StartDate == newStartDate // Keep old start date
                    && m.EndDate == newEndDate // Set new end date from update reqest
                    && m.FirstName == "Updated first name"),
                It.IsAny<string>(), newStartDate, null), Times.Exactly(1));
        }

        [Test]
        public async Task ThenCreatesHistoryIfApproved()
        {
            var testCommitment = new Commitment { ProviderId = 1234, Id = 9874 };
            var expectedOriginalCommitmentState = JsonConvert.SerializeObject(testCommitment);
            _commitment.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(testCommitment);

            var expectedOriginalApprenticeshipState = JsonConvert.SerializeObject(_apprenticeship);

            var command = new AcceptApprenticeshipChangeCommand
            {
                ApprenticeshipId = 1234,
                UserId = "ABC123",
                Caller = new Caller(555, CallerType.Employer)
            };
            await _sut.Handle(command);

            var expectedNewApprenticeshipState = JsonConvert.SerializeObject(_apprenticeship);

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
                                y.Last().EntityId == _apprenticeship.Id &&
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

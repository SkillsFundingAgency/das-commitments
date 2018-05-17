using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Application.Commands.UndoApprenticeshipChange;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UndoApprenticeshipChange
{
    [TestFixture]
    public class WhenUndoingApprenticeshipChange
    {
        UndoApprenticeshipChangeCommandHandler _sut;

        private Mock<AbstractValidator<UndoApprenticeshipChangeCommand>> _validator;
        private Mock<IApprenticeshipUpdateRepository> _repository;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private Mock<IMediator> _mediator;
        private Mock<IApprenticeshipEvents> _apprenticeshipEvents;
        private Mock<IMessagePublisher> _messagePublisher;

        private DateTime _apprenticeshipStartDate;
        private DateTime _effectiveDate;

        private DateTime _updateCreadtedOn;
        private Apprenticeship _apprenticeship;


        [SetUp]
        public void SetUp()
        {
            _validator = new Mock<AbstractValidator<UndoApprenticeshipChangeCommand>>();
            _repository = new Mock<IApprenticeshipUpdateRepository>();
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _mediator = new Mock<IMediator>();
            _apprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _messagePublisher = new Mock<IMessagePublisher>();

            _updateCreadtedOn = DateTime.Now.AddDays(-2);
            _effectiveDate = DateTime.Now.AddDays(-2);
            _validator.Setup(x => x.Validate(It.IsAny<UndoApprenticeshipChangeCommand>()))
                .Returns(() => new ValidationResult());

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
            
            _sut = new UndoApprenticeshipChangeCommandHandler(
                _validator.Object,
                _repository.Object,
                _apprenticeshipRepository.Object,
                _messagePublisher.Object
                );
        }

        [Test]
        public async Task ThenTheApprenticeshipWillBeUpdatedIfUndo()
        {
            const long ApprenticeshipId = 5L;
            const string UserId = "user123";

            await _sut.Handle(
                new UndoApprenticeshipChangeCommand
                {
                    ApprenticeshipId = ApprenticeshipId,
                    UserId = UserId,
                    Caller = new Caller(555, CallerType.Employer)
                });

            _repository.Verify(m => m.ApproveApprenticeshipUpdate(It.Is<ApprenticeshipUpdate>(u => u.Id == 42), UserId, It.IsAny<Apprenticeship>(), It.IsAny<Caller>()), Times.Never);
            _repository.Verify(m => m.RejectApprenticeshipUpdate(It.Is<ApprenticeshipUpdate>(u => u.Id == 42), UserId), Times.Never);
            _repository.Verify(m => m.UndoApprenticeshipUpdate(It.Is<ApprenticeshipUpdate>(u => u.Id == 42), UserId), Times.Once);
            _apprenticeshipEvents.Verify(x => x.PublishEvent(It.IsAny<Commitment>(), It.IsAny<Apprenticeship>(), It.IsAny<string>(), null, null), Times.Never);
        }

        [Test]
        public void ThenThereMustBeAPendingUpdate()
        {
            var command = new UndoApprenticeshipChangeCommand
            {
                Caller = new Caller(666, CallerType.Provider),
                ApprenticeshipId = 5L,
                UserId = "user123"
            };

            _repository.Setup(m => m.GetPendingApprenticeshipUpdate(5L))
                .ReturnsAsync((ApprenticeshipUpdate)null);

            Func<Task> act = async () => await _sut.Handle(command);
            act.ShouldThrow<ValidationException>().WithMessage("No existing apprenticeship update pending for apprenticeship 5");
        }

        [Test]
        public async Task ThenTheUpdateCancelledEventIsCreated()
        {
            const string UserId = "user123";

            await _sut.Handle(
                new UndoApprenticeshipChangeCommand
                {
                    ApprenticeshipId = _apprenticeship.Id,
                    UserId = UserId,
                    Caller = new Caller(555, CallerType.Employer)
                });

            _messagePublisher.Verify(
                x =>
                    x.PublishAsync(
                        It.Is<ApprenticeshipUpdateCancelled>(
                            y => y.ApprenticeshipId == _apprenticeship.Id && y.AccountId == _apprenticeship.EmployerAccountId && y.ProviderId == _apprenticeship.ProviderId)), Times.Once);
        }
    }
}

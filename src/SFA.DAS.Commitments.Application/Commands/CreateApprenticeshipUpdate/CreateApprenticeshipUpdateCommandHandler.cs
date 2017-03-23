using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate
{
    public class CreateApprenticeshipUpdateCommandHandler : AsyncRequestHandler<CreateApprenticeshipUpdateCommand>
    {
        private readonly AbstractValidator<CreateApprenticeshipUpdateCommand> _validator;
        private readonly IApprenticeshipUpdateRepository _apprenticeshipUpdateRepository;
        private readonly ICommitmentsLogger _logger;

        public CreateApprenticeshipUpdateCommandHandler(AbstractValidator<CreateApprenticeshipUpdateCommand> validator, IApprenticeshipUpdateRepository apprenticeshipUpdateRepository, ICommitmentsLogger logger)
        {
            if(validator==null)
                throw new ArgumentNullException();
            if(apprenticeshipUpdateRepository==null)
                throw new ArgumentNullException();
            if(logger==null)
                throw new ArgumentNullException();

            _validator = validator;
            _apprenticeshipUpdateRepository = apprenticeshipUpdateRepository;
            _logger = logger;
        }

        protected override async Task HandleCore(CreateApprenticeshipUpdateCommand command)
        {
            var validationResult = _validator.Validate(command);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            if (await _apprenticeshipUpdateRepository.GetPendingApprenticeshipUpdate(
                    command.ApprenticeshipUpdate.ApprenticeshipId) != null)
            {
                throw new InvalidOperationException("Unable to create a CreateApprenticeshipUpdate for an Apprenticeship with a pending update");
            }

            await _apprenticeshipUpdateRepository.CreateApprenticeshipUpdate(MapFrom(command.ApprenticeshipUpdate));
        }

        private ApprenticeshipUpdate MapFrom(PendingApprenticeshipUpdatePlaceholder source)
        {
            return new ApprenticeshipUpdate
            {
                Id = source.Id,
                ApprenticeshipId = source.ApprenticeshipId,
                Originator = source.Originator,
                FirstName = source.FirstName,
                LastName = source.LastName,
                DateOfBirth = source.DateOfBirth,
                ULN = source.ULN,
                TrainingCode = source.TrainingCode,
                TrainingType = source.TrainingType,
                TrainingName = source.TrainingName,
                Cost = source.Cost,
                StartDate = source.StartDate,
                EndDate = source.EndDate
            };
        }
    }
}

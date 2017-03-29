using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate
{
    public class CreateApprenticeshipUpdateCommandHandler : AsyncRequestHandler<CreateApprenticeshipUpdateCommand>
    {
        private readonly AbstractValidator<CreateApprenticeshipUpdateCommand> _validator;
        private readonly IApprenticeshipUpdateRepository _apprenticeshipUpdateRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ICommitmentsLogger _logger;

        public CreateApprenticeshipUpdateCommandHandler(AbstractValidator<CreateApprenticeshipUpdateCommand> validator, IApprenticeshipUpdateRepository apprenticeshipUpdateRepository, ICommitmentsLogger logger, IApprenticeshipRepository apprenticeshipRepository)
        { 
            if(validator==null)
                throw new ArgumentNullException(nameof(validator));
            if(apprenticeshipUpdateRepository==null)
                throw new ArgumentNullException(nameof(apprenticeshipUpdateRepository));
            if(logger==null)
                throw new ArgumentNullException(nameof(logger));
            if(apprenticeshipRepository==null)
                throw new ArgumentNullException(nameof(apprenticeshipRepository));

            _validator = validator;
            _apprenticeshipUpdateRepository = apprenticeshipUpdateRepository;
            _logger = logger;
            _apprenticeshipRepository = apprenticeshipRepository;
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
                throw new ValidationException("Unable to create an ApprenticeshipUpdate for an Apprenticeship with a pending update");
            }

            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipUpdate.ApprenticeshipId);

            CheckAuthorisation(command, apprenticeship);

            var immediateUpdate = MapToImmediateApprenticeshipUpdate(command);
            var pendingUpdate = MapToPendingApprenticeshipUpdate(command.ApprenticeshipUpdate);

            await _apprenticeshipUpdateRepository.CreateApprenticeshipUpdate(pendingUpdate, immediateUpdate);
        }

        private Apprenticeship MapToImmediateApprenticeshipUpdate(CreateApprenticeshipUpdateCommand command)
        {
            if(string.IsNullOrWhiteSpace(command.ApprenticeshipUpdate.ULN)
                && string.IsNullOrWhiteSpace(command.ApprenticeshipUpdate.EmployerRef)
                && string.IsNullOrWhiteSpace(command.ApprenticeshipUpdate.ProviderRef))
            {
                return null;
            }

            var result = new Apprenticeship
            {
                Id = command.ApprenticeshipUpdate.ApprenticeshipId,
                ULN = command.ApprenticeshipUpdate.ULN,
                ProviderRef = command.ApprenticeshipUpdate.ProviderRef,
                EmployerRef = command.ApprenticeshipUpdate.EmployerRef
            };

            return result;
        }

        private ApprenticeshipUpdate MapToPendingApprenticeshipUpdate(Api.Types.Apprenticeship.ApprenticeshipUpdate source)
        {
            var result =  new ApprenticeshipUpdate
            {
                Id = source.Id,
                ApprenticeshipId = source.ApprenticeshipId,
                Originator = (Originator) source.Originator,
                FirstName = source.FirstName,
                LastName = source.LastName,
                DateOfBirth = source.DateOfBirth,
                TrainingCode = source.TrainingCode,
                TrainingType = source.TrainingType.HasValue ? (TrainingType) source.TrainingType : default(TrainingType?),
                TrainingName = source.TrainingName,
                Cost = source.Cost,
                StartDate = source.StartDate,
                EndDate = source.EndDate
            };

            return result.HasChanges ? result : null;
        }

        private void CheckAuthorisation(CreateApprenticeshipUpdateCommand command, Apprenticeship apprenticeship)
        {
            switch (command.Caller.CallerType)
            {
                case CallerType.Employer:
                    if(apprenticeship.EmployerAccountId != command.Caller.Id)
                        throw new UnauthorizedException($"Employer {command.Caller.Id} not authorised to update apprenticeship {apprenticeship.Id}");
                    break;
                case CallerType.Provider:
                    if (apprenticeship.ProviderId != command.Caller.Id)
                        throw new UnauthorizedException($"Provider {command.Caller.Id} not authorised to update apprenticeship {apprenticeship.Id}");
                    break;
            }
        }
    }
}

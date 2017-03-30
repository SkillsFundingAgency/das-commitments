﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Api.Types.Validation;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
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
        private readonly IMediator _mediator;

        public CreateApprenticeshipUpdateCommandHandler(AbstractValidator<CreateApprenticeshipUpdateCommand> validator, IApprenticeshipUpdateRepository apprenticeshipUpdateRepository, ICommitmentsLogger logger, IApprenticeshipRepository apprenticeshipRepository, IMediator mediator)
        { 
            if(validator==null)
                throw new ArgumentNullException(nameof(validator));
            if(apprenticeshipUpdateRepository==null)
                throw new ArgumentNullException(nameof(apprenticeshipUpdateRepository));
            if(logger==null)
                throw new ArgumentNullException(nameof(logger));
            if(apprenticeshipRepository==null)
                throw new ArgumentNullException(nameof(apprenticeshipRepository));
            if(mediator == null)
                throw new ArgumentNullException(nameof(mediator));

            _validator = validator;
            _apprenticeshipUpdateRepository = apprenticeshipUpdateRepository;
            _logger = logger;
            _apprenticeshipRepository = apprenticeshipRepository;
            _mediator = mediator;
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
            await CheckOverlappingApprenticeships(command, apprenticeship);

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

        private async Task CheckOverlappingApprenticeships(CreateApprenticeshipUpdateCommand command, Apprenticeship originalApprenticeship)
        {
            var coalesce = new Func<string, string, string>((s, s1) => string.IsNullOrWhiteSpace(s) ? s1 : s);

            var overlapResult = await _mediator.SendAsync(new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>
                {
                    new ApprenticeshipOverlapValidationRequest
                    {
                        ApprenticeshipId = originalApprenticeship.Id,
                        Uln = coalesce(command.ApprenticeshipUpdate.ULN, originalApprenticeship.ULN),
                        StartDate = command.ApprenticeshipUpdate.StartDate ?? originalApprenticeship.StartDate.Value,
                        EndDate = command.ApprenticeshipUpdate.EndDate ?? originalApprenticeship.EndDate.Value
                    }
                }
            });

            if (overlapResult.Data.Any())
            {
                foreach (var overlap in overlapResult.Data)
                {
                    _logger.Info($"ApprenticeshipUpdate overlaps with apprenticeship {overlap.Apprenticeship.Id}");
                }
                throw new ValidationException("Unable to create ApprenticeshipUpdate due to overlapping apprenticeship");
            }
        }
    }
}

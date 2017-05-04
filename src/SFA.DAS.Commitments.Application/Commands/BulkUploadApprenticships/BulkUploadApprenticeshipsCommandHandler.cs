using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using SFA.DAS.Commitments.Api.Types.Validation;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.BulkUploadApprenticships
{
    public sealed class BulkUploadApprenticeshipsCommandHandler : AsyncRequestHandler<BulkUploadApprenticeshipsCommand>
    {
        private BulkUploadApprenticeshipsValidator _validator;
        private ICommitmentsLogger _logger;
        private ICommitmentRepository _commitmentRepository;
        private IMediator _mediator;
        private readonly IHistoryRepository _historyRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private IApprenticeshipEvents _apprenticeshipEvents;

        public BulkUploadApprenticeshipsCommandHandler(ICommitmentRepository commitmentRepository, IApprenticeshipRepository apprenticeshipRepository, BulkUploadApprenticeshipsValidator validator, IApprenticeshipEvents apprenticeshipEvents, ICommitmentsLogger logger, IMediator mediator, IHistoryRepository historyRepository)
        {
            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
            _apprenticeshipEvents = apprenticeshipEvents;
            _logger = logger;
            _mediator = mediator;
            _historyRepository = historyRepository;
        }

        protected override async Task HandleCore(BulkUploadApprenticeshipsCommand command)
        {
            LogMessage(command);

            var watch = Stopwatch.StartNew();
            var validationResult = _validator.Validate(command);
            _logger.Trace($"Validating {command.Apprenticeships.Count} apprentices took {watch.ElapsedMilliseconds} milliseconds");

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            watch = Stopwatch.StartNew();
            var commitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);
            _logger.Trace($"Loading commitment took {watch.ElapsedMilliseconds} milliseconds");
            if (commitment == null)
                throw new ResourceNotFoundException($"Provider { command.Caller.Id } specified a non-existant Commitment { command.CommitmentId}");

            // TODO: This logic can be shared between handlers.
            CheckAuthorization(command, commitment);
            CheckEditStatus(command, commitment);
            CheckCommitmentStatus(commitment);

            var apprenticeships = command.Apprenticeships.Select(x => MapFrom(x, command)).ToList();

            await ValidateOverlaps(apprenticeships);

            watch = Stopwatch.StartNew();
            var insertedApprenticeships = await _apprenticeshipRepository.BulkUploadApprenticeships(command.CommitmentId, apprenticeships);
            _logger.Trace($"Bulk insert of {command.Apprenticeships.Count} apprentices into Db took {watch.ElapsedMilliseconds} milliseconds");

            watch = Stopwatch.StartNew();
            await Task.WhenAll(
                _apprenticeshipEvents.BulkPublishDeletionEvent(commitment, commitment.Apprenticeships, "APPRENTICESHIP-DELETED"),
                _apprenticeshipEvents.BulkPublishEvent(commitment, insertedApprenticeships, "APPRENTICESHIP-CREATED"),
                CreateHistory(commitment, insertedApprenticeships, command.Caller.CallerType, command.UserId, command.UserName)
            );
            _logger.Trace($"Publishing bulk uploads of {command.Apprenticeships.Count} events took {watch.ElapsedMilliseconds} milliseconds");
        }

        private async Task CreateHistory(Commitment commitment, IList<Apprenticeship> insertedApprenticeships, CallerType callerType, string userId, string userName)
        {
            var historyService = new HistoryService(_historyRepository);
            historyService.TrackUpdate(commitment, CommitmentChangeType.BulkUploadedApprenticeships.ToString(), commitment.Id, "Commitment", callerType, userId, userName);
            foreach (var apprenticeship in insertedApprenticeships)
            {
                historyService.TrackInsert(apprenticeship, ApprenticeshipChangeType.Created.ToString(), apprenticeship.Id, "Apprenticeship", callerType, userId, userName);
            }
            await historyService.Save();
        }

        private async Task ValidateOverlaps(List<Apprenticeship> apprenticeships)
        {
            _logger.Info("Performing overlap validation for bulk upload");
            var watch = Stopwatch.StartNew();
            var overlapValidationRequest = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>()
            };

            var i = 0;
            foreach (var apprenticeship in apprenticeships.Where(x => x.StartDate.HasValue && x.EndDate.HasValue && !string.IsNullOrEmpty(x.ULN)))
            {
                overlapValidationRequest.OverlappingApprenticeshipRequests.Add(new ApprenticeshipOverlapValidationRequest
                {
                    ApprenticeshipId = i, //assign a row id, as this value will be zero for files
                    Uln = apprenticeship.ULN,
                    StartDate = apprenticeship.StartDate.Value,
                    EndDate = apprenticeship.EndDate.Value
                });
                i++;
            }
            _logger.Trace($"Building Overlap validation command took {watch.ElapsedMilliseconds} milliseconds");

            watch = Stopwatch.StartNew();

            if (overlapValidationRequest.OverlappingApprenticeshipRequests.Any())
            {
                var overlapResponse = await _mediator.SendAsync(overlapValidationRequest);

                watch.Stop();
                _logger.Trace($"Overlap validation took {watch.ElapsedMilliseconds} milliseconds");

                if (overlapResponse.Data.Any())
                {
                    _logger.Info($"Found {overlapResponse.Data.Count} overlapping errors");
                    var errors = overlapResponse.Data.Select(overlap => new ValidationFailure(string.Empty, overlap.ValidationFailReason.ToString())).ToList();
                    throw new ValidationException(errors);
                }
            }
        }

        private Apprenticeship MapFrom(Api.Types.Apprenticeship.Apprenticeship apprenticeship, BulkUploadApprenticeshipsCommand message)
        {
            var domainApprenticeship = new Apprenticeship
            {
                Id = apprenticeship.Id,
                FirstName = apprenticeship.FirstName,
                LastName = apprenticeship.LastName,
                DateOfBirth = apprenticeship.DateOfBirth,
                NINumber = apprenticeship.NINumber,
                ULN = apprenticeship.ULN,
                CommitmentId = message.CommitmentId,
                PaymentStatus = PaymentStatus.PendingApproval,
                AgreementStatus = AgreementStatus.NotAgreed,
                TrainingType = (TrainingType)apprenticeship.TrainingType,
                TrainingCode = apprenticeship.TrainingCode,
                TrainingName = apprenticeship.TrainingName,
                Cost = apprenticeship.Cost,
                StartDate = apprenticeship.StartDate,
                EndDate = apprenticeship.EndDate
            };

            SetCallerSpecificReference(domainApprenticeship, apprenticeship, message.Caller.CallerType);

            return domainApprenticeship;
        }

        private static void SetCallerSpecificReference(Apprenticeship domainApprenticeship, Api.Types.Apprenticeship.Apprenticeship apiApprenticeship, CallerType callerType)
        {
            if (callerType.IsEmployer())
                domainApprenticeship.EmployerRef = apiApprenticeship.EmployerRef;
            else
                domainApprenticeship.ProviderRef = apiApprenticeship.ProviderRef;
        }

        private void LogMessage(BulkUploadApprenticeshipsCommand command)
        {
            string messageTemplate = $"{command.Caller.CallerType}: {command.Caller.Id} has called BulkUploadApprenticeshipsCommand with {command.Apprenticeships?.Count ?? 0} apprenticeships";

            if (command.Caller.CallerType == CallerType.Employer)
                _logger.Info(messageTemplate, accountId: command.Caller.Id, commitmentId: command.CommitmentId);
            else
                _logger.Info(messageTemplate, providerId: command.Caller.Id, commitmentId: command.CommitmentId);
        }

        private static void CheckAuthorization(BulkUploadApprenticeshipsCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.ProviderId != message.Caller.Id)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} unauthorized to view commitment: {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                default:
                    if (commitment.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} unauthorized to view commitment: {message.CommitmentId}");
                    break;
            }
        }

        private static void CheckCommitmentStatus(Commitment commitment)
        {
            if (commitment.CommitmentStatus != CommitmentStatus.New && commitment.CommitmentStatus != CommitmentStatus.Active)
                throw new InvalidOperationException($"Cannot add apprenticeship in commitment {commitment.Id} because status is {commitment.CommitmentStatus}");
        }

        private static void CheckEditStatus(BulkUploadApprenticeshipsCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.ProviderOnly)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} unauthorized to add apprenticeships to commitment {message.CommitmentId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.EmployerOnly)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} unauthorized to add apprenticeship to commitment {message.CommitmentId}");
                    break;
            }
        }
    }
}

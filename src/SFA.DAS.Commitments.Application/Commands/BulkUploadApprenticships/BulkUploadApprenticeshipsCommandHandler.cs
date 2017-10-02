using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
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

            var validationResult = _validator.Validate(command);
            
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetCommitmentById(command.CommitmentId);
            if (commitment == null)
                throw new ResourceNotFoundException($"Provider { command.Caller.Id } specified a non-existent Commitment { command.CommitmentId}");

            // TODO: This logic can be shared between handlers.
            CheckAuthorization(command, commitment);
            CheckEditStatus(command, commitment);
            CheckCommitmentStatus(commitment);

            var apprenticeships = command.Apprenticeships.Select(x => MapFrom(x, command)).ToList();

            await ValidateOverlaps(apprenticeships);

            var insertedApprenticeships = await _apprenticeshipRepository.BulkUploadApprenticeships(command.CommitmentId, apprenticeships);
            await Task.WhenAll(
                _apprenticeshipEvents.BulkPublishDeletionEvent(commitment, commitment.Apprenticeships, "APPRENTICESHIP-DELETED"),
                _apprenticeshipEvents.BulkPublishEvent(commitment, insertedApprenticeships, "APPRENTICESHIP-CREATED"),
                CreateHistory(commitment, insertedApprenticeships, command.Caller.CallerType, command.UserId, command.UserName)
            );
        }

        private async Task CreateHistory(Commitment commitment, IList<Apprenticeship> insertedApprenticeships, CallerType callerType, string userId, string userName)
        {
            var historyService = new HistoryService(_historyRepository);
            historyService.TrackUpdate(commitment, CommitmentChangeType.BulkUploadedApprenticeships.ToString(), commitment.Id, null, callerType, userId, userName);
            foreach (var apprenticeship in insertedApprenticeships)
            {
                historyService.TrackInsert(apprenticeship, ApprenticeshipChangeType.Created.ToString(), null, apprenticeship.Id , callerType, userId, userName);
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

        // Rename to update apprenticehips status
        private Apprenticeship MapFrom(Apprenticeship apprenticeship, BulkUploadApprenticeshipsCommand message)
        {
            // ToDo: Test
            apprenticeship.CommitmentId = message.CommitmentId;
            apprenticeship.PaymentStatus = PaymentStatus.PendingApproval;
            apprenticeship.AgreementStatus = AgreementStatus.NotAgreed;

            return apprenticeship;
        }

        private void LogMessage(BulkUploadApprenticeshipsCommand command)
        {
            string messageTemplate = $"{command.Caller.CallerType}: {command.Caller.Id} has called BulkUploadApprenticeshipsCommand with {command.Apprenticeships?.Count() ?? 0} apprenticeships";

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
                        throw new UnauthorizedException($"Provider {message.Caller.Id} not authorised to access commitment: {message.CommitmentId}, expected provider {commitment.ProviderId}");
                    break;
                case CallerType.Employer:
                default:
                    if (commitment.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} not authorised to access commitment: {message.CommitmentId}, expected employer {commitment.EmployerAccountId}");
                    break;
            }
        }

        private static void CheckEditStatus(BulkUploadApprenticeshipsCommand message, Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.ProviderOnly)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} not allowed to add apprenticeships to commitment {message.CommitmentId}, expected provider {commitment.ProviderId}");
                    break;
                case CallerType.Employer:
                    if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.EmployerOnly)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} not allowed to add apprenticeship to commitment {message.CommitmentId}, expected employer {commitment.EmployerAccountId}");
                    break;
            }
        }

        private static void CheckCommitmentStatus(Commitment commitment)
        {
            if (commitment.CommitmentStatus != CommitmentStatus.New && commitment.CommitmentStatus != CommitmentStatus.Active)
                throw new InvalidOperationException($"Cannot add apprenticeship to commitment {commitment.Id} because status is {commitment.CommitmentStatus}");
        }
    }
}

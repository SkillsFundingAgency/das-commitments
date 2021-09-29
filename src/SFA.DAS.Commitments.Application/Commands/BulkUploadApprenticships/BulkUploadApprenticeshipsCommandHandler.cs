using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Application.Queries.GetEmailOverlappingApprenticeships;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Api.Types;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.Commitments.Application.Commands.BulkUploadApprenticships
{
    public sealed class BulkUploadApprenticeshipsCommandHandler : AsyncRequestHandler<BulkUploadApprenticeshipsCommand>
    {
        private BulkUploadApprenticeshipsValidator _validator;
        private ICommitmentsLogger _logger;
        private ICommitmentRepository _commitmentRepository;
        private IMediator _mediator;
        private readonly IHistoryRepository _historyRepository;
        private readonly IReservationsApiClient _reservationsApiClient;
        private readonly IEncodingService _encodingService;
        private readonly IV2EventsPublisher _v2EventsPublisher;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ITrainingProgrammeRepository _trainingProgrammeRepository;
        private IApprenticeshipEvents _apprenticeshipEvents;

        public BulkUploadApprenticeshipsCommandHandler(ICommitmentRepository commitmentRepository,
            IApprenticeshipRepository apprenticeshipRepository, BulkUploadApprenticeshipsValidator validator,
            IApprenticeshipEvents apprenticeshipEvents, ICommitmentsLogger logger, IMediator mediator,
            IHistoryRepository historyRepository, IReservationsApiClient reservationsApiClient, 
            IEncodingService encodingService, IV2EventsPublisher v2EventsPublisher,
            ITrainingProgrammeRepository trainingProgrammeRepository)
        {
            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
            _apprenticeshipEvents = apprenticeshipEvents;
            _logger = logger;
            _mediator = mediator;
            _historyRepository = historyRepository;
            _reservationsApiClient = reservationsApiClient;
            _encodingService = encodingService;
            _v2EventsPublisher = v2EventsPublisher;
            _trainingProgrammeRepository = trainingProgrammeRepository;
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

            await ValidateEmailOverlaps(apprenticeships);

            await CalculateApprenticehipVersions(apprenticeships);

            var apprenticeshipsWithReservationIds = await MergeBulkCreatedReservationIdsOnToApprenticeships(apprenticeships, commitment);

            var insertedApprenticeships = await _apprenticeshipRepository.BulkUploadApprenticeships(command.CommitmentId, apprenticeshipsWithReservationIds);
            await Task.WhenAll(
                _apprenticeshipEvents.BulkPublishDeletionEvent(commitment, commitment.Apprenticeships, "APPRENTICESHIP-DELETED"),
                _apprenticeshipEvents.BulkPublishEvent(commitment, insertedApprenticeships, "APPRENTICESHIP-CREATED"),
                CreateHistory(commitment, insertedApprenticeships, command.Caller.CallerType, command.UserId, command.UserName),
                PublishBulkUploadIntoCohortCompleted(commitment, insertedApprenticeships)
            );
        }

        private async Task CalculateApprenticehipVersions(IList<Apprenticeship> apprenticeships)
        {
            void ShiftStandardVersionDatesToBeDividedByMonth(IEnumerable<StandardVersion> standardVersions)
            {
                // Overwrite VersionEarliestStart of all versions to 1st of each month so that if a version starts in the same month the latest version
                // is always chosen in that instances
                // First version doesn't get it's VersionEarliestStart overwritten as that won't have an overlap
                // Last version LatestStartDate doesn't matter as it should be null
                // e.g.
                // 1.0  VersionEarliestStart 9/12/2019 VersionLatestStartDate 14/7/2020
                // 1.1  VersionEarliestStart 15/7/2020 VersionLatestStartDate 19/10/2020
                // 1.2  VersionEarliestStart 20/10/2020 VersionLatestStartDate Null

                // Becomes
                // 1.0  VersionEarliestStart 9/12/2019 VersionLatestStartDate 31/7/2020
                // 1.1  VersionEarliestStart 1/7/2020 VersionLatestStartDate 31/10/2020
                // 1.2  VersionEarliestStart 1/10/2020  VersionLatestStartDate Null

                var first = true;
                foreach (var version in standardVersions)
                {
                    if (!first && version.VersionEarliestStartDate.HasValue)
                    {
                        version.VersionEarliestStartDate = new DateTime(version.VersionEarliestStartDate.Value.Year, version.VersionEarliestStartDate.Value.Month, 1);
                    }

                    if (version.VersionLatestStartDate.HasValue)
                    {
                        var daysInMonth = DateTime.DaysInMonth(version.VersionLatestStartDate.Value.Year, version.VersionLatestStartDate.Value.Month);
                        version.VersionLatestStartDate = new DateTime(version.VersionLatestStartDate.Value.Year, version.VersionLatestStartDate.Value.Month, daysInMonth);
                    }

                    first = false;
                }
            }

            StandardVersion GetCalculatedVersionBasedOnStartDate(List<StandardVersion> standardVersions, DateTime startDate)
            {
                // Given the resetting of dates in the ShiftStandardVersionDatesMethod
                // If an apprentice start date is the 29th October 2020
                // 29/10/2020 is > 1/7/2020  and it's < 31/10/2020 so it initially creates a 1.1 Training Programme
                // 29/10/2020 is > 1/10/2020 and VersionLatestStartDate Is null, so then ovewrites with a 1.2 Training Programme

                // Default to Latest Version to start with then override
                var selectedVersion = standardVersions.Last();

                foreach (var version in standardVersions)
                {
                    if (startDate >= version.VersionEarliestStartDate && (version.VersionLatestStartDate.HasValue == false || startDate <= version.VersionLatestStartDate.Value))
                    {
                        selectedVersion = version;
                    }
                }

                return selectedVersion;
            }

            void PopulateApprenticeshipRecord(StandardVersion selectedVersion, Apprenticeship apprenticeship)
            {
                apprenticeship.StandardUId = selectedVersion.StandardUId;
                apprenticeship.TrainingCourseVersion = selectedVersion.Version;
                apprenticeship.TrainingCourseVersionConfirmed = true;
            }

            var standardVersions = await _trainingProgrammeRepository.GetAllStandardVersions();

            if(standardVersions == null)
            {
                throw new ValidationException("Unable to retrieve Standards to pollinate bulk upload data");
            }

            foreach(var apprenticeship in apprenticeships)
            {
                if(!int.TryParse(apprenticeship.TrainingCode, out var larsCode) || !apprenticeship.StartDate.HasValue)
                {
                    // It's a Framework or no Start Date
                    continue;
                }
                
                // Order by Ascending for correct selection of version
                var versions = standardVersions.Where(s => s.LarsCode == larsCode)
                    .OrderBy(s => s.VersionMajor).ThenBy(t => t.VersionMinor).ToList();

                if(!versions.Any())
                {
                    // Lars code not recognised.
                    _logger.Info($"Found unknown Lars Code");
                    throw new ValidationException(new[] { new ValidationFailure("StdCode", $"Unknown StdCode or No Versions found {larsCode}") });
                }

                if(versions.Count == 1)
                {
                    // Only one version exists
                    PopulateApprenticeshipRecord(versions.Single(), apprenticeship);
                    continue;
                }

                // To Account for unknown Start Day of an Apprenticeship or the fact it's stripped to a month date without a day
                // We don't know the exact start date, so if a version overlaps on a month
                // We take the latest version.
                // N.B IMPORTANT NOTE
                // At this point I believe the start date passed in is the raw data from the bulk upload CSV from the provider
                // So this calculation probably isn't actually needed as we have the date value from the user before it is filtered out.
                // However after Dev discussions it was thought best to mimic behaviour in the rest of the system assuming it *is* the
                // 1st of the month until the migration occurs / system behaviour changes. To Be Continued....
                ShiftStandardVersionDatesToBeDividedByMonth(versions);
                var calculatedVersion = GetCalculatedVersionBasedOnStartDate(versions, apprenticeship.StartDate.Value);
                PopulateApprenticeshipRecord(calculatedVersion, apprenticeship);
            }

        }

        private async Task PublishBulkUploadIntoCohortCompleted(Commitment commitment, IList<Apprenticeship> insertedApprenticeships)
        {
            try
            {
                await _v2EventsPublisher.PublishBulkUploadIntoCohortCompleted(commitment.ProviderId.Value,
                    commitment.Id,
                    (uint) insertedApprenticeships.Count);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error calling PublishBulkUploadIntoCohortCompleted Event");
                throw;
            }
        }

        private async Task<IEnumerable<Apprenticeship>> MergeBulkCreatedReservationIdsOnToApprenticeships(IList<Apprenticeship> apprenticeships, Commitment commitment)
        {
            BulkCreateReservationsRequest BuildBulkCreateRequest()
            {
                return new BulkCreateReservationsRequest
                    {Count = (uint) apprenticeships.Count, TransferSenderId = commitment.TransferSenderId};
            }

            BulkCreateReservationsResult bulkReservations;
            try
            {
                bulkReservations = await _reservationsApiClient.BulkCreateReservations(
                    _encodingService.Decode(commitment.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId),
                    BuildBulkCreateRequest(), CancellationToken.None);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed calling BulkCreateReservations endpoint");
                throw;
            }

            if (bulkReservations.ReservationIds.Length != apprenticeships.Count)
            {
                _logger.Info($"The number of bulk reservations did not match the number of apprentices");
                throw new InvalidOperationException(
                    $"The number of bulk reservations ({bulkReservations.ReservationIds.Length}) does not equal the number of apprenticeships ({apprenticeships.Count})");
            }

            return apprenticeships.Zip(bulkReservations.ReservationIds, (a, r) =>
            {
                a.ReservationId = r;
                return a;
            });
        }

        private async Task CreateHistory(Commitment commitment, IList<Apprenticeship> insertedApprenticeships, CallerType callerType, string userId, string userName)
        {
            var historyService = new HistoryService(_historyRepository);
            historyService.TrackUpdate(commitment, CommitmentChangeType.BulkUploadedApprenticeships.ToString(), commitment.Id, null, callerType, userId, commitment.ProviderId, commitment.EmployerAccountId, userName);
            foreach (var apprenticeship in insertedApprenticeships)
            {
                historyService.TrackInsert(apprenticeship, ApprenticeshipChangeType.Created.ToString(), null, apprenticeship.Id , callerType, userId, apprenticeship.ProviderId, apprenticeship.EmployerAccountId, userName);
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

        private async Task ValidateEmailOverlaps(List<Apprenticeship> apprenticeships)
        {
            _logger.Info("Performing overlap email validation for bulk upload");
            var watch = Stopwatch.StartNew();
            var overlapEmailValidationRequest = new GetEmailOverlappingApprenticeshipsRequest
            {
                OverlappingEmailApprenticeshipRequests = new List<ApprenticeshipEmailOverlapValidationRequest>()
            };

            var i = 0;
            foreach (var apprenticeship in apprenticeships.Where(x => x.StartDate.HasValue && x.EndDate.HasValue && !string.IsNullOrEmpty(x.Email)))
            {
                overlapEmailValidationRequest.OverlappingEmailApprenticeshipRequests.Add(new ApprenticeshipEmailOverlapValidationRequest
                {
                    ApprenticeshipId = i, //assign a row id, as this value will be zero for files
                    Email = apprenticeship.Email,
                    StartDate = apprenticeship.StartDate.Value,
                    EndDate = apprenticeship.EndDate.Value
                });
                i++;
            }
            _logger.Trace($"Building Overlap email validation command took {watch.ElapsedMilliseconds} milliseconds");

            watch = Stopwatch.StartNew();

            if (overlapEmailValidationRequest.OverlappingEmailApprenticeshipRequests.Any())
            {
                var overlapResponse = await _mediator.SendAsync(overlapEmailValidationRequest);

                watch.Stop();
                _logger.Trace($"Overlap email validation took {watch.ElapsedMilliseconds} milliseconds");

                if (overlapResponse.Data.Any())
                {
                    _logger.Info($"Found {overlapResponse.Data.Count} overlapping errors");
                    var errors = overlapResponse.Data.Select(overlap => new ValidationFailure(string.Empty, overlap.OverlapStatus.ToString())).ToList();
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

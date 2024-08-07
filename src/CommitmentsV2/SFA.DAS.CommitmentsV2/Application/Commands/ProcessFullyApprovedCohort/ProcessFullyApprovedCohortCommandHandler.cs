using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ProcessFullyApprovedCohort;

public class ProcessFullyApprovedCohortCommandHandler : IRequestHandler<ProcessFullyApprovedCohortCommand>
{
    private readonly IAccountApiClient _accountApiClient;
    private readonly Lazy<ProviderCommitmentsDbContext> _db;
    private readonly IEventPublisher _eventPublisher;
    private readonly IEncodingService _encodingService;
    private readonly ILogger<ProcessFullyApprovedCohortCommandHandler> _logger;

    public ProcessFullyApprovedCohortCommandHandler(IAccountApiClient accountApiClient,
        Lazy<ProviderCommitmentsDbContext> db,
        IEventPublisher eventPublisher,
        IEncodingService encodingService,
        ILogger<ProcessFullyApprovedCohortCommandHandler> logger)
    {
        _accountApiClient = accountApiClient;
        _db = db;
        _eventPublisher = eventPublisher;
        _encodingService = encodingService;
        _logger = logger;
    }

    public async Task Handle(ProcessFullyApprovedCohortCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ProcessFullyApprovedCohortCommand for Cohort {CohortId}.", request.CohortId);

        var account = await _accountApiClient.GetAccount(request.AccountId);
        var apprenticeshipEmployerType = account.ApprenticeshipEmployerType.ToEnum<ApprenticeshipEmployerType>();

        _logger.LogInformation("Account {AccountId} is of type {ApprenticeshipEmployerType}.", request.AccountId, apprenticeshipEmployerType);

        var creationDate = DateTime.UtcNow;

        await _db.Value.ProcessFullyApprovedCohort(request.CohortId, request.AccountId, apprenticeshipEmployerType);

        var events = await _db.Value.Apprenticeships
            .Where(a => a.Cohort.Id == request.CohortId)
            .Select(a => new ApprenticeshipCreatedEvent
            {
                ApprenticeshipId = a.Id,
                CreatedOn = creationDate,
                AgreedOn = a.Cohort.EmployerAndProviderApprovedOn.Value,
                AccountId = a.Cohort.EmployerAccountId,
                AccountLegalEntityPublicHashedId = a.Cohort.AccountLegalEntity.PublicHashedId,
                AccountLegalEntityId = a.Cohort.AccountLegalEntity.Id,
                LegalEntityName = a.Cohort.AccountLegalEntity.Name,
                ProviderId = a.Cohort.ProviderId,
                TransferSenderId = a.Cohort.TransferSenderId,
                ApprenticeshipEmployerTypeOnApproval = apprenticeshipEmployerType,
                Uln = a.Uln,
                DeliveryModel = a.DeliveryModel ?? DeliveryModel.Regular,
                TrainingType = a.ProgrammeType.Value,
                TrainingCode = a.CourseCode,
                StandardUId = a.StandardUId,
                TrainingCourseOption = a.TrainingCourseOption,
                TrainingCourseVersion = a.TrainingCourseVersion,
                StartDate = a.StartDate.GetValueOrDefault(),
                EndDate = a.EndDate.Value,
                PriceEpisodes = a.PriceHistory
                    .Select(p => new PriceEpisode
                    {
                        FromDate = p.FromDate,
                        ToDate = p.ToDate,
                        Cost = p.Cost,
                        EndPointAssessmentPrice = a.IsOnFlexiPaymentPilot == true ? a.EndPointAssessmentPrice : null,
                        TrainingPrice = a.IsOnFlexiPaymentPilot == true ? a.TrainingPrice : null
                    })
                    .ToArray(),
                ContinuationOfId = a.ContinuationOfId,
                DateOfBirth = a.DateOfBirth.Value,
                ActualStartDate = a.ActualStartDate,
                IsOnFlexiPaymentPilot = a.IsOnFlexiPaymentPilot,
                FirstName = a.FirstName,
                LastName = a.LastName,
                ApprenticeshipHashedId = _encodingService.Encode(a.Id, EncodingType.ApprenticeshipId),
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Created {EventsCount} ApprenticeshipCreatedEvent(s) for Cohort {CohortId}.", events.Count, request.CohortId);

        var tasks = events.Select(apprenticeshipCreatedEvent =>
        {
            _logger.LogInformation("Emitting ApprenticeshipCreatedEvent for Apprenticeship {ApprenticeshipId}", apprenticeshipCreatedEvent.ApprenticeshipId);
            return _eventPublisher.Publish(apprenticeshipCreatedEvent);
        });

        await Task.WhenAll(tasks);

        if (request.ChangeOfPartyRequestId.HasValue)
        {
            await Task.WhenAll(EmitChangeOfPartyEvents(request, events));
        }
    }

    private IEnumerable<Task> EmitChangeOfPartyEvents(ProcessFullyApprovedCohortCommand request, IEnumerable<ApprenticeshipCreatedEvent> events)
    {
        var changeOfPartyEvents = events.Select(apprenticeshipCreatedEvent =>
            new ApprenticeshipWithChangeOfPartyCreatedEvent(apprenticeshipCreatedEvent.ApprenticeshipId, request.ChangeOfPartyRequestId.Value, apprenticeshipCreatedEvent.CreatedOn, request.UserInfo, request.LastApprovedBy)
        );

        return changeOfPartyEvents.Select(changeOfPartyCreatedEvent =>
        {
            _logger.LogInformation("Emitting ApprenticeshipWithChangeOfPartyCreatedEvent for Apprenticeship {ApprenticeshipId}", changeOfPartyCreatedEvent.ApprenticeshipId);
            return _eventPublisher.Publish(changeOfPartyCreatedEvent);
        });
    }
}
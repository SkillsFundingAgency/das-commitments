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

public class ProcessFullyApprovedCohortCommandHandler(
    IAccountApiClient accountApiClient,
    Lazy<ProviderCommitmentsDbContext> db,
    IEventPublisher eventPublisher,
    IEncodingService encodingService,
    ILogger<ProcessFullyApprovedCohortCommandHandler> logger)
    : IRequestHandler<ProcessFullyApprovedCohortCommand>
{
    public async Task Handle(ProcessFullyApprovedCohortCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling ProcessFullyApprovedCohortCommand for Cohort {CohortId}.", request.CohortId);

        var account = await accountApiClient.GetAccount(request.AccountId);
        var apprenticeshipEmployerType = account.ApprenticeshipEmployerType.ToEnum<ApprenticeshipEmployerType>();

        logger.LogInformation("Account {AccountId} is of type {ApprenticeshipEmployerType}.", request.AccountId, apprenticeshipEmployerType);

        var creationDate = DateTime.UtcNow;

        await db.Value.ProcessFullyApprovedCohort(request.CohortId, request.AccountId, apprenticeshipEmployerType);

        var events = await db.Value.Apprenticeships
            .Where(a => a.Cohort.Id == request.CohortId)
            .Join(db.Value.Standards, a => a.StandardUId,s=> s.StandardUId, (a, s) => new { a, s })
            .Select(x => new ApprenticeshipCreatedEvent
            {
                ApprenticeshipId = x.a.Id,
                CreatedOn = creationDate,
                AgreedOn = x.a.Cohort.EmployerAndProviderApprovedOn.Value,
                AccountId = x.a.Cohort.EmployerAccountId,
                AccountLegalEntityPublicHashedId = x.a.Cohort.AccountLegalEntity.PublicHashedId,
                AccountLegalEntityId = x.a.Cohort.AccountLegalEntity.Id,
                LegalEntityName = x.a.Cohort.AccountLegalEntity.Name,
                ProviderId = x.a.Cohort.ProviderId,
                TransferSenderId = x.a.Cohort.TransferSenderId,
                ApprenticeshipEmployerTypeOnApproval = apprenticeshipEmployerType,
                Uln = x.a.Uln,
                DeliveryModel = x.a.DeliveryModel ?? DeliveryModel.Regular,
                TrainingType = x.a.ProgrammeType.Value,
                TrainingCode = x.a.CourseCode,
                StandardUId = x.a.StandardUId,
                TrainingCourseOption = x.a.TrainingCourseOption,
                TrainingCourseVersion = x.a.TrainingCourseVersion,
                StartDate = x.a.StartDate.GetValueOrDefault(),
                EndDate = x.a.EndDate.Value,
                PriceEpisodes = x.a.PriceHistory
                    .Select(p => new PriceEpisode
                    {
                        FromDate = p.FromDate,
                        ToDate = p.ToDate,
                        Cost = p.Cost,
                        EndPointAssessmentPrice = p.AssessmentPrice,
                        TrainingPrice = p.TrainingPrice
                    })
                    .ToArray(),
                ContinuationOfId = x.a.ContinuationOfId,
                DateOfBirth = x.a.DateOfBirth.Value,
                ActualStartDate = x.a.ActualStartDate,
                FirstName = x.a.FirstName,
                LastName = x.a.LastName,
                ApprenticeshipHashedId = encodingService.Encode(x.a.Id, EncodingType.ApprenticeshipId),
                LearnerDataId = x.a.LearnerDataId,
                LearningType = x.s.ApprenticeshipType
            })
            .ToListAsync(cancellationToken);

        logger.LogInformation("Created {EventsCount} ApprenticeshipCreatedEvent(s) for Cohort {CohortId}.", events.Count, request.CohortId);

        var tasks = events.Select(apprenticeshipCreatedEvent =>
        {
            logger.LogInformation("Emitting ApprenticeshipCreatedEvent for Apprenticeship {ApprenticeshipId}", apprenticeshipCreatedEvent.ApprenticeshipId);
            return eventPublisher.Publish(apprenticeshipCreatedEvent);
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
            new ApprenticeshipWithChangeOfPartyCreatedEvent(
                apprenticeshipCreatedEvent.ApprenticeshipId,
                request.ChangeOfPartyRequestId.Value,
                apprenticeshipCreatedEvent.CreatedOn,
                request.UserInfo,
                request.LastApprovedBy)
        );

        return changeOfPartyEvents.Select(changeOfPartyCreatedEvent =>
        {
            logger.LogInformation("Emitting ApprenticeshipWithChangeOfPartyCreatedEvent for Apprenticeship {ApprenticeshipId}", changeOfPartyCreatedEvent.ApprenticeshipId);
            return eventPublisher.Publish(changeOfPartyCreatedEvent);
        });
    }
}
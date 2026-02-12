using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;
using ApprenticeshipEmployerType = SFA.DAS.CommitmentsV2.Types.ApprenticeshipEmployerType;

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
            .Where(apprenticeship => apprenticeship.Cohort.Id == request.CohortId)
            .Join(db.Value.Standards, apprenticeship => apprenticeship.StandardUId, standard => standard.StandardUId, (apprenticeship, standard) => new { apprenticeship, standard })
            .Select(x => new ApprenticeshipCreatedEvent
            {
                ApprenticeshipId = x.apprenticeship.Id,
                CreatedOn = creationDate,
                AgreedOn = x.apprenticeship.Cohort.EmployerAndProviderApprovedOn.Value,
                AccountId = x.apprenticeship.Cohort.EmployerAccountId,
                AccountLegalEntityPublicHashedId = x.apprenticeship.Cohort.AccountLegalEntity.PublicHashedId,
                AccountLegalEntityId = x.apprenticeship.Cohort.AccountLegalEntity.Id,
                LegalEntityName = x.apprenticeship.Cohort.AccountLegalEntity.Name,
                ProviderId = x.apprenticeship.Cohort.ProviderId,
                TransferSenderId = x.apprenticeship.Cohort.TransferSenderId,
                ApprenticeshipEmployerTypeOnApproval = apprenticeshipEmployerType,
                Uln = x.apprenticeship.Uln,
                DeliveryModel = x.apprenticeship.DeliveryModel ?? DeliveryModel.Regular,
                TrainingType = x.apprenticeship.ProgrammeType.Value,
                TrainingCode = x.apprenticeship.CourseCode,
                StandardUId = x.apprenticeship.StandardUId,
                TrainingCourseOption = x.apprenticeship.TrainingCourseOption,
                TrainingCourseVersion = x.apprenticeship.TrainingCourseVersion,
                StartDate = x.apprenticeship.StartDate.GetValueOrDefault(),
                EndDate = x.apprenticeship.EndDate.Value,
                PriceEpisodes = x.apprenticeship.PriceHistory
                    .Select(p => new PriceEpisode
                    {
                        FromDate = p.FromDate,
                        ToDate = p.ToDate,
                        Cost = p.Cost,
                        EndPointAssessmentPrice = p.AssessmentPrice,
                        TrainingPrice = p.TrainingPrice
                    })
                    .ToArray(),
                ContinuationOfId = x.apprenticeship.ContinuationOfId,
                DateOfBirth = x.apprenticeship.DateOfBirth.Value,
                ActualStartDate = x.apprenticeship.ActualStartDate,
                FirstName = x.apprenticeship.FirstName,
                LastName = x.apprenticeship.LastName,
                ApprenticeshipHashedId = encodingService.Encode(x.apprenticeship.Id, EncodingType.ApprenticeshipId),
                LearnerDataId = x.apprenticeship.LearnerDataId,
                LearningType = Enum.Parse<LearningType>(x.standard.ApprenticeshipType, true)
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
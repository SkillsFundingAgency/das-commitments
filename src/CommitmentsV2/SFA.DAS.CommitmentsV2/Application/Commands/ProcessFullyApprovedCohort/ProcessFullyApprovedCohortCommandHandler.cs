using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
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
    CommitmentsV2Configuration configuration,
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

        var apprenticeships = await db.Value.Apprenticeships.Include(x=>x.PriceHistory).Include(x => x.Cohort).ThenInclude(x => x.AccountLegalEntity).Where(a => a.Cohort.Id == request.CohortId).ToListAsync(cancellationToken);

        List<ApprenticeshipCreatedEvent> events;
        if (configuration.IgnoreShortCourses)
        {
            var matches = apprenticeships
            .Where(a => a.Cohort.Id == request.CohortId)
            .Join(db.Value.Standards,
                a => a.StandardUId,
                s => s.StandardUId,
                (a, s) => new { a, s })
            .ToList();
            
            events = matches.Select(x => MapToApprenticeshipCreatedEvent(
                x.a,
                creationDate,
                apprenticeshipEmployerType,
                _ => Enum.Parse<SFA.DAS.Common.Domain.Types.LearningType>(x.s.ApprenticeshipType, true)))
            .ToList();
        }
        else
        {
            var matches = apprenticeships
                .Where(a => a.Cohort.Id == request.CohortId)
                .Join(db.Value.Courses,
                    a => a.CourseCode,
                    c => c.LarsCode,
                    (a, c) => new { a, c })
                .ToList();
            
            events = matches.Select(x => MapToApprenticeshipCreatedEvent(
                    x.a,
                    creationDate,
                    apprenticeshipEmployerType,
                    _ => x.c.LearningType.ToCommonLearningType() ?? SFA.DAS.Common.Domain.Types.LearningType.Apprenticeship))
                .ToList();
        }
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

    private ApprenticeshipCreatedEvent MapToApprenticeshipCreatedEvent(
        Apprenticeship apprenticeship,
        DateTime creationDate,
        ApprenticeshipEmployerType apprenticeshipEmployerType,
        Func<Apprenticeship, Common.Domain.Types.LearningType> learningTypeResolver)
    {
        return new ApprenticeshipCreatedEvent
        {
            ApprenticeshipId = apprenticeship.Id,
            CreatedOn = creationDate,
            AgreedOn = apprenticeship.Cohort.EmployerAndProviderApprovedOn.Value,
            AccountId = apprenticeship.Cohort.EmployerAccountId,
            AccountLegalEntityPublicHashedId = apprenticeship.Cohort.AccountLegalEntity.PublicHashedId,
            AccountLegalEntityId = apprenticeship.Cohort.AccountLegalEntity.Id,
            LegalEntityName = apprenticeship.Cohort.AccountLegalEntity.Name,
            ProviderId = apprenticeship.Cohort.ProviderId,
            TransferSenderId = apprenticeship.Cohort.TransferSenderId,
            ApprenticeshipEmployerTypeOnApproval = apprenticeshipEmployerType,
            Uln = apprenticeship.Uln,
            DeliveryModel = apprenticeship.DeliveryModel ?? DeliveryModel.Regular,
            TrainingType = apprenticeship.ProgrammeType.Value,
            TrainingCode = apprenticeship.CourseCode,
            StandardUId = apprenticeship.StandardUId,
            TrainingCourseOption = apprenticeship.TrainingCourseOption,
            TrainingCourseVersion = apprenticeship.TrainingCourseVersion,
            StartDate = apprenticeship.StartDate.GetValueOrDefault(),
            EndDate = apprenticeship.EndDate.Value,
            PriceEpisodes = apprenticeship.PriceHistory
                .Select(p => new PriceEpisode
                {
                    FromDate = p.FromDate,
                    ToDate = p.ToDate,
                    Cost = p.Cost,
                    EndPointAssessmentPrice = p.AssessmentPrice,
                    TrainingPrice = p.TrainingPrice
                })
                .ToArray(),
            ContinuationOfId = apprenticeship.ContinuationOfId,
            DateOfBirth = apprenticeship.DateOfBirth.Value,
            ActualStartDate = apprenticeship.ActualStartDate,
            FirstName = apprenticeship.FirstName,
            LastName = apprenticeship.LastName,
            ApprenticeshipHashedId = encodingService.Encode(apprenticeship.Id, EncodingType.ApprenticeshipId),
            LearnerDataId = apprenticeship.LearnerDataId,
            LearningType = learningTypeResolver(apprenticeship)
        };
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
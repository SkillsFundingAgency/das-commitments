using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;

public class StopApprenticeshipCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICurrentDateTime currentDate,
    IMessageSession nserviceBusContext,
    IEncodingService encodingService,
    ILogger<StopApprenticeshipCommandHandler> logger,
    CommitmentsV2Configuration commitmentsV2Configuration,
    IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService,
    IEventPublisher eventPublisher)
    : IRequestHandler<StopApprenticeshipCommand>
{
    private const string StopNotificationEmailTemplate = "ProviderApprenticeshipStopNotification";

    public async Task Handle(StopApprenticeshipCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Begin stopping apprenticeShip. Apprenticeship-Id:{ApprenticeshipId}",
                request.ApprenticeshipId);

            CheckPartyIsValid(request.Party);

            var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(request.ApprenticeshipId, cancellationToken);

            apprenticeship.StopApprenticeship(request.StopDate, request.AccountId, request.MadeRedundant, request.UserInfo, currentDate, request.Party);
            await dbContext.Value.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Stopped apprenticeship. Apprenticeship-Id:{ApprenticeshipId}",
                request.ApprenticeshipId);

            await resolveOverlappingTrainingDateRequestService.Resolve(
                request.ApprenticeshipId,
                null,
                OverlappingTrainingDateRequestResolutionType.ApprenticeshipStopped
                );

            logger.LogInformation("Sending email to Provider {ProviderId}, template {StopNotificationEmailTemplate}", apprenticeship.Cohort.ProviderId, StopNotificationEmailTemplate);

            if (apprenticeship.StopDate == request.StopDate)
            {
                var events = new ApprenticeshipStopBackEvent()
                {
                    ApprenticeshipId = null,
                    LearnerDataId = apprenticeship.LearnerDataId,
                    Uln = apprenticeship.Uln,
                    ProviderId = apprenticeship.Cohort.ProviderId
                };

                logger.LogInformation("Emitting stop back event for {ApprenticeshipId}", apprenticeship.Id);
                await eventPublisher.Publish(events);
            }

            await NotifyProvider(
                apprenticeship.Cohort.ProviderId,
                apprenticeship.Id,
                apprenticeship.Cohort.AccountLegalEntity.Name,
                apprenticeship.ApprenticeName, 
                request.StopDate
            );
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error Stopping Apprenticeship with id {ApprenticeshipId}", request.ApprenticeshipId);
            throw;
        }
    }

    private async Task NotifyProvider(long providerId, long apprenticeshipId, string employerName,
        string apprenticeName, DateTime stopDate)
    {
        var sendEmailToProviderCommand = new SendEmailToProviderCommand(providerId, StopNotificationEmailTemplate,
            new Dictionary<string, string>
            {
                {"EMPLOYER", employerName},
                {"APPRENTICE", apprenticeName},
                {"DATE", stopDate.ToString("dd/MM/yyyy")},
                {
                    "URL",
                    $"{commitmentsV2Configuration.ProviderCommitmentsBaseUrl}/{providerId}/apprentices/{encodingService.Encode(apprenticeshipId, EncodingType.ApprenticeshipId)}"
                }
            });

        await nserviceBusContext.Send(sendEmailToProviderCommand);
    }

    private static void CheckPartyIsValid(Party party)
    {
        if (party != Party.Employer)
        {
            throw new DomainException(nameof(party),
                $"StopApprenticeship is restricted to Employers only - {party} is invalid");
        }
    }
}
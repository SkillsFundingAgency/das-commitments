using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;

public class StopApprenticeshipCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICurrentDateTime currentDate,
    IMessageSession nserviceBusContext,
    IEncodingService encodingService,
    ILogger<StopApprenticeshipCommandHandler> logger,
    CommitmentsV2Configuration commitmentsV2Configuration,
    IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService)
    : IRequestHandler<StopApprenticeshipCommand>
{
    private const string StopNotificationEmailTemplate = "ProviderApprenticeshipStopNotification";

    public async Task Handle(StopApprenticeshipCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Begin stopping apprenticeship. Apprenticeship-Id:{ApprenticeshipId}, StopSource:{StopSource}",
                request.ApprenticeshipId, request.StopSource);

            CheckPartyIsValid(request.Party);

            var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(request.ApprenticeshipId, cancellationToken);

            if (request.StopSource == StopSource.Employer && apprenticeship.WithdrawnReasonCode.HasValue)
            {
                throw new DomainException(nameof(apprenticeship.WithdrawnReasonCode),
                    "Apprenticeship was withdrawn in ILR and cannot be stopped by the employer");
            }

            apprenticeship.StopApprenticeship(
                request.StopDate,
                request.AccountId,
                request.MadeRedundant,
                request.UserInfo,
                currentDate,
                request.Party,
                request.StopSource,
                request.WithdrawnReasonCode);

            await dbContext.Value.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Stopped apprenticeship. Apprenticeship-Id:{ApprenticeshipId}, StopSource:{StopSource}",
                request.ApprenticeshipId, request.StopSource);

            await resolveOverlappingTrainingDateRequestService.Resolve(
                request.ApprenticeshipId,
                null,
                OverlappingTrainingDateRequestResolutionType.ApprenticeshipStopped
                );

            if (request.StopSource == StopSource.Ilr)
            {
                await SendLearningHistory(request);
                return;
            }

            logger.LogInformation("Sending email to Provider {ProviderId}, template {StopNotificationEmailTemplate}", apprenticeship.Cohort.ProviderId, StopNotificationEmailTemplate);

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

    private async Task SendLearningHistory(StopApprenticeshipCommand request)
    {
        var command = new StoreLearningHistoryCommand
        {
            ApprenticeshipId = request.ApprenticeshipId,
            Source = LearningSourceType.ILRStatusChange,
            ChangeType = LearningChangeType.AutoApproved,
            LearningKey = request.LearningKey,
            AppliedDate = request.AppliedDate ?? currentDate.UtcNow,
            Description = $"ILR Learner status changed from Live to Withdrawn due to {request.WithdrawnReasonCode}"
        };

        await nserviceBusContext.Send(command);
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
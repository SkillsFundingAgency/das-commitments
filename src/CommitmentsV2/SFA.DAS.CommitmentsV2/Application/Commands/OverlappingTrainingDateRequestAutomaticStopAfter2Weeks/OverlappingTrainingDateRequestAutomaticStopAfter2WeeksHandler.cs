using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.Application.Commands.OverlappingTrainingDateRequestAutomaticStopAfter2Weeks;

public class OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandler(
    Lazy<ProviderCommitmentsDbContext> commitmentsDbContext,
    IMessageSession messageSession,
    ICurrentDateTime currentDateTime,
    CommitmentsV2Configuration configuration,
    ILogger<OverlappingTrainingDateRequestAutomaticStopAfter2WeeksHandler> logger)
    : IRequestHandler<OverlappingTrainingDateRequestAutomaticStopAfter2WeeksCommand>
{
    public const string TemplateId = "ExpiredOverlappingTrainingDateForServiceDesk";

    public async Task Handle(OverlappingTrainingDateRequestAutomaticStopAfter2WeeksCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var currentDate = currentDateTime.UtcNow;

            var pendingRecords = await commitmentsDbContext.Value.OverlappingTrainingDateRequests
                .Include(oltd => oltd.DraftApprenticeship)
                .ThenInclude(draftApprenticeship => draftApprenticeship.Cohort)
                .Include(oltd => oltd.PreviousApprenticeship)
                .ThenInclude(previousApprenticeship => previousApprenticeship.Cohort)
                .Where(x => x.NotifiedServiceDeskOn == null
                            && x.Status == OverlappingTrainingDateRequestStatus.Pending
                            && x.CreatedOn < currentDate.AddDays(-14).Date)
                .ToListAsync(cancellationToken);

            if (pendingRecords != null && pendingRecords.Count != 0)
            {
                var zenDeskCount = 0;
                var stoppingCount = 0;

                foreach (var request in pendingRecords)
                {
                    if (request.DraftApprenticeship != null)
                    {
                        switch (request.PreviousApprenticeship.PaymentStatus)
                        {
                            case PaymentStatus.Withdrawn:
                            case PaymentStatus.Completed:
                                zenDeskCount++;
                                await SendToZenDesk(request);
                                break;
                            case PaymentStatus.Active:
                            case PaymentStatus.Paused:
                                stoppingCount++;
                                await AutoStopOverlappingTrainingDateRequest(request);
                                break;
                            default:
                                logger.LogWarning("Unhandled PaymentStatus for OverlappingTrainingDateRequest ID: {requestId}, PaymentStatus: {paymentStatus}", request.Id, request.PreviousApprenticeship.PaymentStatus);
                                break;
                        }
                    }
                }

                logger.LogInformation("Found {count} OLTDs to send to ZenDesk", zenDeskCount);
                logger.LogInformation("Found {count} OLTDs to send to automatically stop", stoppingCount);

                await commitmentsDbContext.Value.SaveChangesAsync(default);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while automatically stopping overlapping training date requests");
            throw;
        }
    }

    private async Task AutoStopOverlappingTrainingDateRequest(OverlappingTrainingDateRequest request)
    {
        logger.LogInformation("Sending StopApprenticeshipRequest for ApprenticeshipId {PreviousApprenticeshipId}", request.PreviousApprenticeshipId);

        var stopDate = request.DraftApprenticeship.StartDate.Value;

        if (request.PreviousApprenticeship.IsWaitingToStart(currentDateTime))
        {
            stopDate = request.PreviousApprenticeship.StartDate.Value;
        }
        else
        {
            stopDate = SetStopDateIfDraftApprenticeshipHasFutureStartDate(request, stopDate, currentDateTime.UtcNow);
        }

        await messageSession.Send(new AutomaticallyStopOverlappingTrainingDateRequestCommand(
            request.PreviousApprenticeship.Cohort.EmployerAccountId,
            request.PreviousApprenticeshipId,
            stopDate,
            false,
            UserInfo.System,
            Party.Employer));
    }

    private static DateTime SetStopDateIfDraftApprenticeshipHasFutureStartDate(OverlappingTrainingDateRequest request, DateTime stopDate, DateTime today)
    {
        if (request.PreviousApprenticeship.StartDate.Value < today
            && request.PreviousApprenticeship.EndDate.Value > today
            && request.DraftApprenticeship.StartDate.Value > today)
        {
            stopDate = new DateTime(today.Year, today.Month, 1);
        }
        return stopDate;
    }

    private async Task SendToZenDesk(OverlappingTrainingDateRequest request)
    {
        var tokens = new Dictionary<string, string>
        {
            { "RequestCreatedByProviderEmail", string.IsNullOrWhiteSpace(request.RequestCreatedByProviderEmail) ? "Not available" : request.RequestCreatedByProviderEmail },
            { "LastUpdatedByProviderEmail", request.DraftApprenticeship?.Cohort?.LastUpdatedByProviderEmail },
            { "ULN", request.DraftApprenticeship?.Uln },
            { "NewProviderUkprn", request.DraftApprenticeship?.Cohort?.ProviderId.ToString() },
            { "OldProviderUkprn", request.PreviousApprenticeship?.Cohort?.ProviderId.ToString() }
        };

        var emailCommand = new SendEmailCommand(TemplateId, configuration.ZenDeskEmailAddress, tokens);
        await messageSession.Send(emailCommand);
        request.NotifiedServiceDeskOn = currentDateTime.UtcNow;
    }
}
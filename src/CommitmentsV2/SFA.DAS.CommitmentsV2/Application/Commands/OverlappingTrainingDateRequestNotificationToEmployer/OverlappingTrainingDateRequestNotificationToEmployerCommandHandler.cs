using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Application.Commands.OverlappingTrainingDateRequestNotificationToEmployer;

public class OverlappingTrainingDateRequestNotificationToEmployerCommandHandler(
    Lazy<ProviderCommitmentsDbContext> commitmentsDbContext,
    ICurrentDateTime currentDateTime,
    IMessageSession messageSession,
    CommitmentsV2Configuration configuration,
    IEncodingService encodingService,
    ILogger<OverlappingTrainingDateRequestNotificationToEmployerCommandHandler> logger)
    : IRequestHandler<OverlappingTrainingDateRequestNotificationToEmployerCommand>
{
    public const string TemplateId = "ChaseEmployerForOverlappingTrainingDateRequest";

    public async Task Handle(OverlappingTrainingDateRequestNotificationToEmployerCommand request, CancellationToken cancellationToken)
    {
        var currentDate = currentDateTime.UtcNow;

        var pendingRecords = await commitmentsDbContext.Value.OverlappingTrainingDateRequests
            .Include(oltd => oltd.DraftApprenticeship)
            .ThenInclude(draftApprenticeship => draftApprenticeship.Cohort)
            .Include(oltd => oltd.PreviousApprenticeship)
            .ThenInclude(previousApprenticeship => previousApprenticeship.Cohort)
            .Where(x => x.NotifiedServiceDeskOn == null
                        && x.NotifiedEmployerOn == null
                        && x.Status == Types.OverlappingTrainingDateRequestStatus.Pending
                        && x.CreatedOn < currentDate.AddDays(-7).Date)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Found {count} records which chaser email to employer", pendingRecords.Count);

        foreach (var pendingRecord in pendingRecords)
        {
            logger.LogInformation("Sending chaser email to employer - with cohort ref:{PreviousApprenticeshipCohortRef} for apprentice with ULN:{PreviousApprenticeshipUln}", pendingRecord.PreviousApprenticeship.Cohort.Reference, pendingRecord.PreviousApprenticeship.Uln);

            if (pendingRecord.DraftApprenticeship == null)
            {
                continue;
            }

            var tokens = new Dictionary<string, string>
            {
                { "Cohort", pendingRecord.PreviousApprenticeship.Cohort.Reference },
                { "RequestRaisedDate", pendingRecord.CreatedOn.ToString("dd-MM-yyyy") },
                { "Apprentice", pendingRecord.PreviousApprenticeship.FirstName + " " + pendingRecord.PreviousApprenticeship.LastName },
                { "ULN", pendingRecord.PreviousApprenticeship.Uln },
                { "URL", $"{configuration.EmployerCommitmentsBaseUrl}/{encodingService.Encode(pendingRecord.PreviousApprenticeship.Cohort.EmployerAccountId, EncodingType.AccountId)}/apprentices/{encodingService.Encode(pendingRecord.PreviousApprenticeshipId, EncodingType.ApprenticeshipId)}/details" }
            };

            var emailCommand = new SendEmailToEmployerCommand(pendingRecord.PreviousApprenticeship.Cohort.EmployerAccountId, TemplateId, tokens, null, "NAME");

            await messageSession.Send(emailCommand);

            pendingRecord.NotifiedEmployerOn = currentDateTime.UtcNow;
        }

        await commitmentsDbContext.Value.SaveChangesAsync(cancellationToken);
    }
}
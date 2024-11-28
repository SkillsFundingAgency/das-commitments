using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Encoding;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Configuration;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ApprenticeshipResumedEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IEncodingService encodingService,
    ILogger<ApprenticeshipResumedEventHandler> logger,
    CommitmentsV2Configuration commitmentsV2Configuration)
    : IHandleMessages<ApprenticeshipResumedEvent>
{
    public async Task Handle(ApprenticeshipResumedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Received {TypeName} for apprentice {ApprenticeshipId}", nameof(ApprenticeshipResumedEventHandler), message?.ApprenticeshipId);

        if (message != null)
        {
            var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(message.ApprenticeshipId, default);

            var emailToProviderCommand = BuildEmailToProviderCommand(apprenticeship, message.ResumedOn);

            await context.Send(emailToProviderCommand, new SendOptions());
        }
    }

    private SendEmailToProviderCommand BuildEmailToProviderCommand(Apprenticeship apprenticeship, DateTime resumeDate)
    {
        var sendEmailToProviderCommand = new SendEmailToProviderCommand(apprenticeship.Cohort.ProviderId,
            "ProviderApprenticeshipResumeNotification",
            new Dictionary<string, string>
            {
                {"EMPLOYER", apprenticeship.Cohort.AccountLegalEntity.Name},
                {"APPRENTICE",  $"{apprenticeship.FirstName} {apprenticeship.LastName}"},
                {"DATE", resumeDate.ToString("dd/MM/yyyy")},
                {"URL", $"{commitmentsV2Configuration.ProviderCommitmentsBaseUrl}/{apprenticeship.Cohort.ProviderId}/apprentices/{encodingService.Encode(apprenticeship.Id, EncodingType.ApprenticeshipId)}"}
            });

        return sendEmailToProviderCommand;
    }
}
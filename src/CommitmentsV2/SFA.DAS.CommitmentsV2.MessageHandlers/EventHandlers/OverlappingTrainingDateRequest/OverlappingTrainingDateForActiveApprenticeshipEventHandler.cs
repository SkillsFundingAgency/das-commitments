using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Encoding;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest;

public class OverlappingTrainingDateForActiveApprenticeshipEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<OverlappingTrainingDateForActiveApprenticeshipEventHandler> logger,
    IEncodingService encodingService,
    CommitmentsV2Configuration commitmentsV2Configuration)
    : IHandleMessages<OverlappingTrainingDateCreatedEvent>
{
    public async Task Handle(OverlappingTrainingDateCreatedEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("Received {TypeName} for Uln {Uln}", nameof(OverlappingTrainingDateCreatedEvent), message?.Uln);

            if (message != null)
            {
                var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(message.ApprenticeshipId, default);

                var currentApprenticeshipStatus = apprenticeship.GetApprenticeshipStatus(DateTime.UtcNow);

                if (currentApprenticeshipStatus is ApprenticeshipStatus.Live or ApprenticeshipStatus.WaitingToStart or ApprenticeshipStatus.Paused)
                {
                    var sendEmailToEmployerCommand = BuildEmailToEmployerCommand(apprenticeship, message);

                    await context.Send(sendEmailToEmployerCommand, new SendOptions());
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Send message to employer for Uln {Uln}", message?.Uln);
            throw;
        }
    }

    private SendEmailToEmployerCommand BuildEmailToEmployerCommand(Apprenticeship apprenticeship, OverlappingTrainingDateCreatedEvent message)
    {
        var sendEmailToEmployerCommand = new SendEmailToEmployerCommand(apprenticeship.Cohort.EmployerAccountId,
            "EmployerOverlappingTrainingDateForActiveApprenticeship",
            new Dictionary<string, string>
            {
                { "ULN", message.Uln },
                { "APPRENTICENAME", $"{apprenticeship.FirstName} {apprenticeship.LastName}" },
                { "URL", $"{commitmentsV2Configuration.EmployerCommitmentsBaseUrl}/{encodingService.Encode(apprenticeship.Cohort.EmployerAccountId, EncodingType.AccountId)}/apprentices/{encodingService.Encode(apprenticeship.Id, EncodingType.ApprenticeshipId)}/details" }
            }, null, "NAME"
        );

        return sendEmailToEmployerCommand;
    }
}
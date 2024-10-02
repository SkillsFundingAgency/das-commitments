using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Extensions;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest;

public class OverlappingTrainingDateForCompletedApprenticeshipEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<OverlappingTrainingDateForCompletedApprenticeshipEventHandler> logger,
    IEncodingService encodingService,
    CommitmentsV2Configuration commitmentsV2Configuration)
    : IHandleMessages<OverlappingTrainingDateCreatedEvent>
{
    public async Task Handle(OverlappingTrainingDateCreatedEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("Received {TypeName)} for Uln {Uln}",nameof(OverlappingTrainingDateCreatedEvent), message?.Uln);

            if (message != null)
            {
                var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(message.ApprenticeshipId, default);

                var currentApprenticeshipStatus = apprenticeship.GetApprenticeshipStatus(DateTime.UtcNow);

                if (currentApprenticeshipStatus == ApprenticeshipStatus.Completed)
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
            "EmployerOverlappingTrainingDateForCompletedApprenticeship",
            new Dictionary<string, string>
            {
                {"Uln",message.Uln},
                {"Apprentice", $"{apprenticeship.FirstName} {apprenticeship.LastName}"},
                {"EndDate",apprenticeship.EndDate?.ToGdsFormatLongMonthWithoutDay()},
                {"Url", $"{commitmentsV2Configuration.EmployerCommitmentsBaseUrl}/{encodingService.Encode(apprenticeship.Cohort.EmployerAccountId,EncodingType.AccountId)}/apprentices/{encodingService.Encode(apprenticeship.Id, EncodingType.ApprenticeshipId)}/details"}
            }, null, "Name"
        );

        return sendEmailToEmployerCommand;
    }
}
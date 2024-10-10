using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ProviderRejectedChangeOfPartyRequestEventHandler(IEncodingService encodingService, Lazy<ProviderCommitmentsDbContext> dbContext)
    : IHandleMessages<ProviderRejectedChangeOfPartyRequestEvent>
{
    public async Task Handle(ProviderRejectedChangeOfPartyRequestEvent message, IMessageHandlerContext context)
    {
        var changeOfPartyRequest = await dbContext.Value.GetChangeOfPartyRequestAggregate(message.ChangeOfPartyRequestId, default);
           
        if (changeOfPartyRequest.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeProvider)
        {
            var sendEmailCommand = new SendEmailToEmployerCommand(message.EmployerAccountId,
                "TrainingProviderRejectedChangeOfProviderCohort",
                new Dictionary<string, string>
                {
                    { "EmployerName", message.EmployerName },
                    { "TrainingProviderName", message.TrainingProviderName },
                    { "ApprenticeNamePossessive", message.ApprenticeName.EndsWith('s') ? message.ApprenticeName + "'" : message.ApprenticeName + "'s" },
                    { "AccountHashedId", encodingService.Encode(message.EmployerAccountId, EncodingType.AccountId) },
                    { "ApprenticeshipHashedId", encodingService.Encode(changeOfPartyRequest.ApprenticeshipId, EncodingType.ApprenticeshipId) } 
                },
                message.RecipientEmailAddress
            );

            await context.Send(sendEmailCommand, new SendOptions());
        }
    }
}
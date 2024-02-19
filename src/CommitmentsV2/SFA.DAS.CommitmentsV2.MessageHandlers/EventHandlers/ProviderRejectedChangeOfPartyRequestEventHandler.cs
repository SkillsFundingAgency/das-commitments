using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ProviderRejectedChangeOfPartyRequestEventHandler : IHandleMessages<ProviderRejectedChangeOfPartyRequestEvent>
    {
        private readonly IEncodingService _encodingService;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public ProviderRejectedChangeOfPartyRequestEventHandler(IEncodingService encodingService, Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _encodingService = encodingService;
            _dbContext = dbContext;
        }

        public async Task Handle(ProviderRejectedChangeOfPartyRequestEvent message, IMessageHandlerContext context)
        {
            var changeOfPartyRequest = await _dbContext.Value.GetChangeOfPartyRequestAggregate(message.ChangeOfPartyRequestId, default);
           
            if (changeOfPartyRequest.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeProvider)
            {
                var sendEmailCommand = new SendEmailToEmployerCommand(message.EmployerAccountId,
                    "TrainingProviderRejectedChangeOfProviderCohort",
                    new Dictionary<string, string>
                    {
                        { "EmployerName", message.EmployerName },
                        { "TrainingProviderName", message.TrainingProviderName },
                        { "ApprenticeNamePossessive", message.ApprenticeName.EndsWith("s") ? message.ApprenticeName + "'" : message.ApprenticeName + "'s" },
                        { "AccountHashedId", _encodingService.Encode(message.EmployerAccountId, EncodingType.AccountId) },
                        { "ApprenticeshipHashedId", _encodingService.Encode(changeOfPartyRequest.ApprenticeshipId, EncodingType.ApprenticeshipId) } 
                    },
                    message.RecipientEmailAddress
                );

                await context.Send(sendEmailCommand, new SendOptions());
            }
        }
    }
}

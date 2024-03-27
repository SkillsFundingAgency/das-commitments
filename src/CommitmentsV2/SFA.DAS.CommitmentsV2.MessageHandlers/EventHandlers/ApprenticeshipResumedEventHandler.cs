using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Encoding;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Configuration;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipResumedEventHandler : IHandleMessages<ApprenticeshipResumedEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IEncodingService _encodingService;
        private readonly ILogger<ApprenticeshipResumedEventHandler> _logger;
        private readonly CommitmentsV2Configuration _commitmentsV2Configuration;

        public ApprenticeshipResumedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, IEncodingService encodingService,
            ILogger<ApprenticeshipResumedEventHandler> logger, CommitmentsV2Configuration commitmentsV2Configuration)
        {
            _dbContext = dbContext;
            _encodingService = encodingService;
            _logger = logger;
            _commitmentsV2Configuration = commitmentsV2Configuration;
        }

        public async Task Handle(ApprenticeshipResumedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Received {nameof(ApprenticeshipResumedEventHandler)} for apprentice {message?.ApprenticeshipId}");

            if (message != null)
            {
                var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(message.ApprenticeshipId, default);

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
                                  {"URL", $"{_commitmentsV2Configuration.ProviderCommitmentsBaseUrl}/{apprenticeship.Cohort.ProviderId}/apprentices/{_encodingService.Encode(apprenticeship.Id, EncodingType.ApprenticeshipId)}"}
                      });

            return sendEmailToProviderCommand;
        }
    }
}

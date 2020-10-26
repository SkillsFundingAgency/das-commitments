using System;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Encoding;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipResumedEventHandler : IHandleMessages<ApprenticeshipResumedEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IEncodingService _encodingService;
        private readonly ILogger<ApprenticeshipResumedEventHandler> _logger;

        public ApprenticeshipResumedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, IEncodingService encodingService, ILogger<ApprenticeshipResumedEventHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
            _encodingService = encodingService;
        }

        public async Task Handle(ApprenticeshipResumedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Received {nameof(ApprenticeshipResumedEventHandler)} for apprentice {message?.ApprenticeshipId}");

            var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(message.ApprenticeshipId, default);

            var emailToProviderCommand = BuildEmailToProviderCommand(apprenticeship, message.ResumedOn);

            await context.Send(emailToProviderCommand, new SendOptions());
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
                                  {"URL", $"{apprenticeship.Cohort.ProviderId}/apprentices/manage/{_encodingService.Encode(apprenticeship.Id, EncodingType.ApprenticeshipId)}/details"}
                      });

            return sendEmailToProviderCommand;
        }
    }
}

using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Encoding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipPausedEventHandler : IHandleMessages<ApprenticeshipPausedEvent>
    {
        private readonly ILogger<ApprenticeshipPausedEventHandler> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IEncodingService _encodingService;

        public ApprenticeshipPausedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<ApprenticeshipPausedEventHandler> logger, IEncodingService encodingService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _encodingService = encodingService;
        }

        public async Task Handle(ApprenticeshipPausedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Received {nameof(ApprenticeshipPausedEventHandler)} for apprentice {message?.ApprenticeshipId}");

            var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(message.ApprenticeshipId, default);

            var emailToProviderCommand = BuildEmailToProviderCommand(apprenticeship);

            await context.Send(emailToProviderCommand, new SendOptions());
        }

        private SendEmailToProviderCommand BuildEmailToProviderCommand(Apprenticeship apprenticeship)
        {
            var sendEmailToProviderCommand = new SendEmailToProviderCommand(apprenticeship.Cohort.ProviderId,
                "ProviderApprenticeshipPauseNotification",
                      new Dictionary<string, string>
                      {
                                  {"EMPLOYER", apprenticeship.Cohort.AccountLegalEntity.Name},
                                  {"APPRENTICE", $"{apprenticeship.FirstName} {apprenticeship.LastName}"},
                                  {"DATE", apprenticeship.PauseDate?.ToString("dd/MM/yyyy")},
                                  {"URL", $"{apprenticeship.Cohort.ProviderId}/apprentices/manage/{_encodingService.Encode(apprenticeship.Id, EncodingType.ApprenticeshipId)}/details"}
                      });

            return sendEmailToProviderCommand;
        }
    }
}
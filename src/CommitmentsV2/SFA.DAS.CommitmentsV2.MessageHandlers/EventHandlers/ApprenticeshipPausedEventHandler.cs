using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipPausedEventHandler : IHandleMessages<ApprenticeshipPausedEvent>
    {
        private readonly ILogger<ApprenticeshipPausedEventHandler> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IEncodingService _encodingService;
        private readonly CommitmentsV2Configuration _commitmentsV2Configuration;

        private const string EmailTemplateName = "ProviderApprenticeshipPauseNotification";

        public ApprenticeshipPausedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<ApprenticeshipPausedEventHandler> logger,
            IEncodingService encodingService, CommitmentsV2Configuration commitmentsV2Configuration)
        {
            _dbContext = dbContext;
            _logger = logger;
            _encodingService = encodingService;
            _commitmentsV2Configuration = commitmentsV2Configuration;
        }

        public async Task Handle(ApprenticeshipPausedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Received {HandlerName} for apprentice {ApprenticeshipId}", nameof(ApprenticeshipPausedEventHandler), message?.ApprenticeshipId);

            var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(message.ApprenticeshipId, default);

            if (apprenticeship.PaymentStatus != PaymentStatus.Paused)
            {
                _logger.LogWarning("Apprenticeship '{ApprenticeshipId}' has a PaymentStatus of '{Status}' which is not Paused. Exiting.", 
                    apprenticeship.Id, 
                    apprenticeship.PaymentStatus.ToString());
                
                return;
            }

            var emailToProviderCommand = BuildEmailToProviderCommand(apprenticeship);

            await context.Send(emailToProviderCommand, new SendOptions());
        }

        private SendEmailToProviderCommand BuildEmailToProviderCommand(Apprenticeship apprenticeship)
        {
            return new SendEmailToProviderCommand(apprenticeship.Cohort.ProviderId,
                EmailTemplateName,
                new Dictionary<string, string>
                {
                    { "EMPLOYER", apprenticeship.Cohort.AccountLegalEntity.Name },
                    { "APPRENTICE", $"{apprenticeship.FirstName} {apprenticeship.LastName}" },
                    { "DATE", apprenticeship.PauseDate?.ToString("dd/MM/yyyy") },
                    { "URL", $"{_commitmentsV2Configuration.ProviderCommitmentsBaseUrl}/{apprenticeship.Cohort.ProviderId}/apprentices/{_encodingService.Encode(apprenticeship.Id, EncodingType.ApprenticeshipId)}" }
                });
        }
    }
}
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Encoding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class OverlappingTrainingDateResolvedEventHandler : IHandleMessages<OverlappingTrainingDateResolvedEvent>
    {
        private readonly ILogger<OverlappingTrainingDateResolvedEventHandler> _logger;
        private readonly CommitmentsV2Configuration _commitmentsV2Configuration;
        private readonly IEncodingService _encodingService;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        public OverlappingTrainingDateResolvedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
            ILogger<OverlappingTrainingDateResolvedEventHandler> logger,
            IEncodingService encodingService,
            CommitmentsV2Configuration commitmentsV2Configuration)
        {
            _logger = logger;
            _commitmentsV2Configuration = commitmentsV2Configuration;
            _encodingService = encodingService;
            _dbContext = dbContext;
        }

        public async Task Handle(OverlappingTrainingDateResolvedEvent message, IMessageHandlerContext context)
        {
            try
            {
                _logger.LogInformation($"Received {nameof(OverlappingTrainingDateResolvedEvent)} for DraftApprenticeship {message?.ApprenticeshipId}");

                var draftApprenticeship = await _dbContext.Value.GetOLTDResolvedDraftApprenticeshipAggregate(message.CohortId, message.ApprenticeshipId, default);

                var sendEmailToProviderCommand = BuildEmailToEmployerCommand(draftApprenticeship, message);

                await context.Send(sendEmailToProviderCommand, new SendOptions());

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Send message to provider for DraftApprenticeship {message?.ApprenticeshipId}");
                throw;
            }
        }

        private SendEmailToProviderCommand BuildEmailToEmployerCommand(DraftApprenticeship draftApprenticeship, OverlappingTrainingDateResolvedEvent message)
        {
            var sendEmailToProviderCommand = new SendEmailToProviderCommand(draftApprenticeship.Cohort.ProviderId,
                "OverlappingTrainingDateResolved",
                new Dictionary<string, string>
                {
                        {"PROVIDERNAME", draftApprenticeship.Cohort.Provider.Name},
                        {"COHORTREFERENCE",draftApprenticeship.Cohort.Reference},
                        {"URL", $"{_commitmentsV2Configuration.ProviderCommitmentsBaseUrl}/{_encodingService.Encode(draftApprenticeship.Cohort.ProviderId,EncodingType.AccountId)}/apprentices/{_encodingService.Encode(draftApprenticeship.Id, EncodingType.ApprenticeshipId)}/details"}
                }, draftApprenticeship.Cohort.LastUpdatedByProviderEmail
            );

            return sendEmailToProviderCommand;
        }

    }
}


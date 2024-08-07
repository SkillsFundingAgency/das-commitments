﻿using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Encoding;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest
{
    public class OverlappingTrainingDateForActiveApprenticeshipEventHandler : IHandleMessages<OverlappingTrainingDateCreatedEvent>
    {
        private readonly ILogger<OverlappingTrainingDateForActiveApprenticeshipEventHandler> _logger;
        private readonly CommitmentsV2Configuration _commitmentsV2Configuration;
        private readonly IEncodingService _encodingService;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        public OverlappingTrainingDateForActiveApprenticeshipEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
            ILogger<OverlappingTrainingDateForActiveApprenticeshipEventHandler> logger,

            IEncodingService encodingService,
            CommitmentsV2Configuration commitmentsV2Configuration)
        {
            _logger = logger;
            _commitmentsV2Configuration = commitmentsV2Configuration;
            _encodingService = encodingService;
            _dbContext = dbContext;
        }

        public async Task Handle(OverlappingTrainingDateCreatedEvent message, IMessageHandlerContext context)
        {
            try
            {
                _logger.LogInformation($"Received {nameof(OverlappingTrainingDateCreatedEvent)} for Uln {message?.Uln}");

                if (message != null)
                {
                    var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(message.ApprenticeshipId, default);

                    var currentApprenticeshipStatus = apprenticeship.GetApprenticeshipStatus(DateTime.UtcNow);

                    if (currentApprenticeshipStatus == ApprenticeshipStatus.Live
                        || currentApprenticeshipStatus == ApprenticeshipStatus.WaitingToStart
                        || currentApprenticeshipStatus == ApprenticeshipStatus.Paused)
                    {
                        var sendEmailToEmployerCommand = BuildEmailToEmployerCommand(apprenticeship, message);

                        await context.Send(sendEmailToEmployerCommand, new SendOptions());
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Send message to employer for Uln {message?.Uln}");
                throw;
            }
        }

        private SendEmailToEmployerCommand BuildEmailToEmployerCommand(Apprenticeship apprenticeship, OverlappingTrainingDateCreatedEvent message)
        {

            var sendEmailToEmployerCommand = new SendEmailToEmployerCommand(apprenticeship.Cohort.EmployerAccountId,
                "EmployerOverlappingTrainingDateForActiveApprenticeship",
                new Dictionary<string, string>
                {
                        {"ULN",message.Uln},
                        {"APPRENTICENAME", $"{apprenticeship.FirstName} {apprenticeship.LastName}"},
                        {"URL", $"{_commitmentsV2Configuration.EmployerCommitmentsBaseUrl}/{_encodingService.Encode(apprenticeship.Cohort.EmployerAccountId,EncodingType.AccountId)}/apprentices/{_encodingService.Encode(apprenticeship.Id, EncodingType.ApprenticeshipId)}/details"}
                }, null, "NAME"
            );

            return sendEmailToEmployerCommand;
        }

    }
}


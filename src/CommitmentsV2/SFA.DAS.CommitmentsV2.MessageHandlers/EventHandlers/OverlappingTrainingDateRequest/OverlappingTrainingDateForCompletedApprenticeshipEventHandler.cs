﻿using Microsoft.Extensions.Logging;
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
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events.OverlappingTrainingDateRequest
{
    public class OverlappingTrainingDateForCompletedApprenticeshipEventHandler : IHandleMessages<OverlappingTrainingDateEvent>
    {
        private readonly ILogger<OverlappingTrainingDateForCompletedApprenticeshipEventHandler> _logger;
        private readonly CommitmentsV2Configuration _commitmentsV2Configuration;
        private readonly IEncodingService _encodingService;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        public OverlappingTrainingDateForCompletedApprenticeshipEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
            ILogger<OverlappingTrainingDateForCompletedApprenticeshipEventHandler> logger,
            IEncodingService encodingService,
            CommitmentsV2Configuration commitmentsV2Configuration)
        {
            _logger = logger;
            _commitmentsV2Configuration = commitmentsV2Configuration;
            _encodingService = encodingService;
            _dbContext = dbContext;
        }

        public async Task Handle(OverlappingTrainingDateEvent message, IMessageHandlerContext context)
        {
            try
            {
                _logger.LogInformation($"Received {nameof(OverlappingTrainingDateEvent)} for Uln {message?.Uln}");

                var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(message.ApprenticeshipId, default);

                if (apprenticeship.ApprenticeshipStatus == ApprenticeshipStatus.Completed)
                {
                    var sendEmailToEmployerCommand = BuildEmailToEmployerCommand(apprenticeship, message);

                    await context.Send(sendEmailToEmployerCommand, new SendOptions());
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Send message to employer for Uln {message?.Uln}");
                throw;
            }
        }

        private SendEmailToEmployerCommand BuildEmailToEmployerCommand(Apprenticeship apprenticeship, OverlappingTrainingDateEvent message)
        {

            var sendEmailToEmployerCommand = new SendEmailToEmployerCommand(apprenticeship.Cohort.EmployerAccountId,
                "OverlappingTrainingDateForCompletedApprenticeship",
                new Dictionary<string, string>
                {
                        {"Uln",message.Uln},
                        {"Apprentice", $"{apprenticeship.FirstName} {apprenticeship.LastName}"},
                        {"EndDate",apprenticeship.EndDate?.ToString("dd/MM/yyyy")},
                        {"Url", $"{_commitmentsV2Configuration.EmployerCommitmentsBaseUrl}/{_encodingService.Encode(apprenticeship.Cohort.EmployerAccountId,EncodingType.AccountId)}/apprentices/{_encodingService.Encode(apprenticeship.Id, EncodingType.ApprenticeshipId)}/details"}
                },null, "Name"
            );

            return sendEmailToEmployerCommand;
        }

    }
}

﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class CohortWithChangeOfPartyFullyApprovedEventHandler : IHandleMessages<CohortWithChangeOfPartyFullyApprovedEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<CohortWithChangeOfPartyFullyApprovedEventHandler> _logger;

        public CohortWithChangeOfPartyFullyApprovedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<CohortWithChangeOfPartyFullyApprovedEventHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Handle(CohortWithChangeOfPartyFullyApprovedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"CohortWithChangeOfPartyFullyApprovedEvent received for Cohort {message.CohortId}, ChangeOfPartyRequest {message.ChangeOfPartyRequestId}");

            try
            {
                var changeOfPartyRequest = await
                    _dbContext.Value.GetChangeOfPartyRequestAggregate(message.ChangeOfPartyRequestId, default);

                if (changeOfPartyRequest.Status != ChangeOfPartyRequestStatus.Pending)
                {
                    _logger.LogWarning(
                        $"Unable to Approve ChangeOfPartyRequest {message.ChangeOfPartyRequestId} - status is already {changeOfPartyRequest.Status}");
                    return;
                }

                changeOfPartyRequest.Approve(message.ApprovedBy, message.UserInfo);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error processing CohortWithChangeOfPartyFullyApprovedEvent", e);
                throw;
            }
        }
    }
}

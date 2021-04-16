using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Commitments.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class TransferRequestApprovedEventHandler : IHandleMessages<TransferRequestApprovedEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILegacyTopicMessagePublisher _legacyTopicMessagePublisher;
        private readonly ILogger<TransferRequestApprovedEvent> _logger;

        public TransferRequestApprovedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<TransferRequestApprovedEvent> logger)
        {
            _dbContext = dbContext;
            _legacyTopicMessagePublisher = legacyTopicMessagePublisher;
            _logger = logger;
        }

        public async Task Handle(TransferRequestApprovedEvent message, IMessageHandlerContext context)
        {
            try
            {
                var cohort = await _dbContext.Value.Cohorts.Include(c=>c.Apprenticeships).SingleAsync(c => c.Id == message.CohortId);
                cohort.Approve(Party.TransferSender, null, message.UserInfo, message.ApprovedOn, false);

                // Publish legacy event so Tasks can decrement it's counter
                await _legacyTopicMessagePublisher.PublishAsync(new CohortApprovedByTransferSender(message.TransferRequestId,
                    cohort.EmployerAccountId,
                    cohort.Id,
                    cohort.TransferSenderId.Value,
                    message.UserInfo.UserDisplayName,
                    message.UserInfo.UserEmail));
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error when trying to approve Cohort {message.CohortId} for TransferRequest {message.TransferRequestId}");
                throw;
            }
        }
    }
}
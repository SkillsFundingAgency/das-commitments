﻿using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprovedCohortReturnedToProviderEventHandler : IHandleMessages<ApprovedCohortReturnedToProviderEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILegacyTopicMessagePublisher _legacyTopicMessagePublisher;
        private readonly ILogger<ApprovedCohortReturnedToProviderEventHandler> _logger;

        public ApprovedCohortReturnedToProviderEventHandler(IMediator mediator, ILegacyTopicMessagePublisher legacyTopicMessagePublisher, ILogger<ApprovedCohortReturnedToProviderEventHandler> logger)
        {
            _mediator = mediator;
            _legacyTopicMessagePublisher = legacyTopicMessagePublisher;
            _logger = logger;
        }

        public async Task Handle(ApprovedCohortReturnedToProviderEvent message, IMessageHandlerContext context)
        {
            try
            {
                var response = await _mediator.Send(new GetCohortSummaryQuery(message.CohortId));

                await _legacyTopicMessagePublisher.PublishAsync(new ApprovedCohortReturnedToProvider
                {
                    AccountId = response.AccountId,
                    ProviderId = response.ProviderId.Value,
                    CommitmentId = message.CohortId
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when trying to publish ApprovedCohortReturnedToProvider ");
                throw;
            }
        }
    }
}
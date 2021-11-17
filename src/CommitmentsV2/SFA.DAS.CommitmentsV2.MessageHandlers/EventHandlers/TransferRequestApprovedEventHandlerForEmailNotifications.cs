using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class TransferRequestApprovedEventHandlerForEmailNotifications : IHandleMessages<TransferRequestApprovedEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IMediator _mediator;
        private readonly IEncodingService _encodingService;

        public TransferRequestApprovedEventHandlerForEmailNotifications(Lazy<ProviderCommitmentsDbContext> dbContext, IMediator mediator, IEncodingService encodingService)
        {
            _dbContext = dbContext;
            _mediator = mediator;
            _encodingService = encodingService;
        }

        public async Task Handle(TransferRequestApprovedEvent message, IMessageHandlerContext context)
        {
            var db = _dbContext.Value;

            var cohortSummary = await _mediator.Send(new GetCohortSummaryQuery(message.CohortId));

            var cohortReference = _encodingService.Encode(cohortSummary.CohortId, EncodingType.CohortReference);

            var transferRequest = await db.TransferRequests.SingleAsync(x => x.Id == message.TransferRequestId);

            var tasks = new List<Task>();

            if (!transferRequest.AutoApproval) { 
                var sendEmailToEmployerCommand = new SendEmailToEmployerCommand(cohortSummary.AccountId,
                    "SenderApprovedCommitmentEmployerNotification", new Dictionary<string, string>
                    {
                        { "employer_name", cohortSummary.LegalEntityName },
                        { "cohort_reference", cohortReference },
                        { "sender_name", cohortSummary.TransferSenderName }
                    },
                    cohortSummary.LastUpdatedByEmployerEmail);

                tasks.Add(context.Send(sendEmailToEmployerCommand, new SendOptions()));
            }

            var sendEmailToProviderCommand = new SendEmailToProviderCommand(cohortSummary.ProviderId.Value,
                "SenderApprovedCommitmentProviderNotification",
                new Dictionary<string, string>
                {
                    { "cohort_reference", cohortReference }
                },
                cohortSummary.LastUpdatedByProviderEmail);

            tasks.Add(context.Send(sendEmailToProviderCommand, new SendOptions()));

            await Task.WhenAll(tasks);
        }
    }
}

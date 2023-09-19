using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.AddTransferRequest;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class CohortTransferApprovalRequestedEventHandler : IHandleMessages<CohortTransferApprovalRequestedEvent>
    {
        private readonly IMediator _mediator;
        private readonly IEncodingService _encodingService;
        private readonly ILogger<CohortTransferApprovalRequestedEventHandler> _logger;

        public CohortTransferApprovalRequestedEventHandler(IMediator mediator, IEncodingService encodingService, ILogger<CohortTransferApprovalRequestedEventHandler> logger)
        {
            _mediator = mediator;
            _encodingService = encodingService;
            _logger = logger;
        }

        public async Task Handle(CohortTransferApprovalRequestedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation("{TypeName} processing started.", nameof(CohortTransferApprovalRequestedEventHandler));
            
            await _mediator.Send(new AddTransferRequestCommand { CohortId = message.CohortId, LastApprovedByParty = message.LastApprovedByParty });

            var cohortSummary = await _mediator.Send(new GetCohortSummaryQuery(message.CohortId));

            if (message.LastApprovedByParty == Party.Employer)
            {
                _logger.LogInformation("{TypeName} - Last Approved by Party: Employer.", nameof(CohortTransferApprovalRequestedEventHandler));
                //send "TransferPendingFinalApproval" to the Provider to go here
            }
            else if (message.LastApprovedByParty == Party.Provider)
            {
                _logger.LogInformation("{TypeName} - Last Approved by Party: Provider.", nameof(CohortTransferApprovalRequestedEventHandler));
                
                var tokens = new Dictionary<string, string>
                {
                    {"provider_name", cohortSummary.ProviderName },
                    {"sender_name", cohortSummary.TransferSenderName },
                    {"employer_hashed_account", _encodingService.Encode(cohortSummary.AccountId, EncodingType.AccountId) },
                    {"cohort_reference", _encodingService.Encode(cohortSummary.CohortId, EncodingType.CohortReference)}
                };

                await context.Send(new SendEmailToEmployerCommand(cohortSummary.AccountId,
                    "EmployerTransferPendingFinalApproval", tokens,
                    cohortSummary.LastUpdatedByEmployerEmail));
            }
            
            _logger.LogInformation("{TypeName} processing completed.", nameof(CohortTransferApprovalRequestedEventHandler));
        }
    }
}
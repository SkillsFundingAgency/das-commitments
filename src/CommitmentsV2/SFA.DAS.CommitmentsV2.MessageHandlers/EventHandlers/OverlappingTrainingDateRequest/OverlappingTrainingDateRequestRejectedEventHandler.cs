using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Encoding;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest
{
    public class OverlappingTrainingDateRequestRejectedEventHandler : IHandleMessages<OverlappingTrainingDateRequestRejectedEvent>
    {
        private readonly ILogger<OverlappingTrainingDateRequestRejectedEventHandler> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly CommitmentsV2Configuration _commitmentsV2Configuration;

        public OverlappingTrainingDateRequestRejectedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<OverlappingTrainingDateRequestRejectedEventHandler> logger,
        CommitmentsV2Configuration commitmentsV2Configuration)
        {
            _dbContext = dbContext;
            _logger = logger;
            _commitmentsV2Configuration = commitmentsV2Configuration;
        }
        public async Task Handle(OverlappingTrainingDateRequestRejectedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Received {nameof(OverlappingTrainingDateRequestRejectedEvent)} for overlapping training date request {message.OverlappingTrainingDateRequestId}");

            var oltd = await _dbContext.Value.OverlappingTrainingDateRequests
                   .Include(r => r.DraftApprenticeship)
                   .ThenInclude(d => d.Cohort)
                   .Include(r => r.PreviousApprenticeship)
                   .ThenInclude(a => a.Cohort)
                   .SingleOrDefaultAsync(c => c.Id == message.OverlappingTrainingDateRequestId
                  , CancellationToken.None);
            var emailToProviderCommand = BuildEmailToProviderCommand(oltd);

            await context.Send(emailToProviderCommand, new SendOptions());
        }

        private SendEmailToProviderCommand BuildEmailToProviderCommand(Models.OverlappingTrainingDateRequest oltd)
        {
            var sendEmailToProviderCommand = new SendEmailToProviderCommand(oltd.DraftApprenticeship.Cohort.ProviderId,
                "ProviderOverlappingTrainingDateRequestRejected",
                      new Dictionary<string, string>
                      {
                          {"CohortReference", oltd.DraftApprenticeship.Cohort.Reference},
                          {"URL", $"{_commitmentsV2Configuration.ProviderCommitmentsBaseUrl}{oltd.DraftApprenticeship.Cohort.ProviderId}/unapproved/{oltd.DraftApprenticeship.Cohort.Reference}/details" }
                      }, oltd.DraftApprenticeship.Cohort.LastUpdatedByProviderEmail);

            return sendEmailToProviderCommand;
        }
    }
}

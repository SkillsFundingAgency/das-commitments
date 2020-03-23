using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class RecordedAct1CompletionPaymentEventHandler : IHandleMessages<RecordedAct1CompletionPaymentFakeEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<RecordedAct1CompletionPaymentEventHandler> _logger;

        public RecordedAct1CompletionPaymentEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<RecordedAct1CompletionPaymentEventHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Handle(RecordedAct1CompletionPaymentFakeEvent message, IMessageHandlerContext context)
        {
            try
            {
                if (message.ApprenticeshipId.HasValue)
                {
                    var apprentice = await _dbContext.Value.Apprenticeships.Include(x=>x.Cohort).SingleAsync(x => x.Id == message.ApprenticeshipId);

                    switch (apprentice.Status)
                    {
                        case ApprenticeshipStatus.Live:
                            apprentice.Complete(message.EventTime.DateTime);
                            break;
                        case ApprenticeshipStatus.Completed:
                            apprentice.UpdateCompletionDate(message.EventTime.DateTime);
                            break;
                        default:
                            _logger.LogWarning($"Warning {nameof(RecordedAct1CompletionPaymentEventHandler)} - Cannot process CompletionEvent for apprenticeshipId {apprentice.Id} as status is {apprentice.Status}");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error processing RecordedAct1CompletionPaymentEvent", e);
                throw;
            }
        }
    }
}
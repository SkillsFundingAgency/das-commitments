using Microsoft.Extensions.Logging;
using NServiceBus;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Payments.ProviderPayments.Messages;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class RecordedAct1CompletionPaymentEventHandler : IHandleMessages<RecordedAct1CompletionPayment>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<RecordedAct1CompletionPaymentEventHandler> _logger;

        public RecordedAct1CompletionPaymentEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<RecordedAct1CompletionPaymentEventHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Handle(RecordedAct1CompletionPayment message, IMessageHandlerContext context)
        {
            try
            {
                if (message.ApprenticeshipId.HasValue)
                {
                    var apprentice = await _dbContext.Value.Apprenticeships.Include(x=>x.Cohort).SingleAsync(x => x.Id == message.ApprenticeshipId);
                    var status = apprentice.GetApprenticeshipStatus(message.EventTime.UtcDateTime);

                    switch (status)
                    {
                        case ApprenticeshipStatus.Live:
                        case ApprenticeshipStatus.Paused:
                        case ApprenticeshipStatus.Stopped:
                            apprentice.Complete(message.EventTime.UtcDateTime);
                            _logger.LogInformation($"PaymentCompletion - Completed method called for ApprenticeshipId '{message.ApprenticeshipId}' - status prior to completion was {status}");
                            break;
                        case ApprenticeshipStatus.Completed:
                            apprentice.UpdateCompletionDate(message.EventTime.UtcDateTime);
                            _logger.LogInformation($"PaymentCompletion - UpdateCompletionDate method called for ApprenticeshipId '{message.ApprenticeshipId}'");
                            break;
                        default:
                            _logger.LogWarning($"Warning {nameof(RecordedAct1CompletionPaymentEventHandler)} - Cannot process CompletionEvent for apprenticeshipId {apprentice.Id} as status is {status}");
                            break;
                    }
                }
                else
                {
                    _logger.LogWarning("Warning - No Apprenticeship Id found in RecordedAct1CompletionPaymentEvent");
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
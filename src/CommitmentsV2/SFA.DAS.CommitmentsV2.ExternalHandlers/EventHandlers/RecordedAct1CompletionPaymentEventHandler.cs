using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Payments.ProviderPayments.Messages;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;

public class RecordedAct1CompletionPaymentEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<RecordedAct1CompletionPaymentEventHandler> logger)
    : IHandleMessages<RecordedAct1CompletionPayment>
{
    public async Task Handle(RecordedAct1CompletionPayment message, IMessageHandlerContext context)
    {
        try
        {
            if (message.ApprenticeshipId.HasValue)
            {
                var apprentice = await dbContext.Value.Apprenticeships.Include(x => x.Cohort).SingleAsync(x => x.Id == message.ApprenticeshipId);
                var appliedDate = message.EventTime.UtcDateTime;
                var status = apprentice.GetApprenticeshipStatus(appliedDate);

                switch (status)
                {
                    case ApprenticeshipStatus.Live:
                    case ApprenticeshipStatus.Paused:
                    case ApprenticeshipStatus.Stopped:
                        apprentice.Complete(appliedDate);
                        await apprentice.SendStatusChangeHistory(status, appliedDate, LearningSourceType.ILRStatusChange,
                            history => context.Send(history));
                        logger.LogInformation("PaymentCompletion - Completed method called for ApprenticeshipId '{ApprenticeshipId}' - status prior to completion was {Status}", message.ApprenticeshipId, status.ToString());
                        break;
                    case ApprenticeshipStatus.Completed:
                        apprentice.UpdateCompletionDate(appliedDate);
                        logger.LogInformation("PaymentCompletion - UpdateCompletionDate method called for ApprenticeshipId '{ApprenticeshipId}'", message.ApprenticeshipId);
                        break;
                    default:
                        logger.LogWarning("Warning {TypeName} - Cannot process CompletionEvent for apprenticeshipId {ApprenticeId} as status is {Status}", nameof(RecordedAct1CompletionPaymentEventHandler), apprentice.Id, status);
                        break;
                }
            }
            else
            {
                logger.LogWarning("Warning - No Apprenticeship Id found in RecordedAct1CompletionPaymentEvent");
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing RecordedAct1CompletionPaymentEvent");
            throw;
        }
    }
}
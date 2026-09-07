using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Extensions;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;

public class LearningResumedEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<LearningResumedEventHandler> logger,
    IEncodingService encodingService,
    CommitmentsV2Configuration commitmentsV2Configuration)
    : IHandleMessages<LearningResumedEvent>
{
    public async Task Handle(LearningResumedEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation(" Started executing {Event}", nameof(LearningResumedEvent));

            if (message is null)
            {
                logger.LogInformation("Event received null message : {Event}", nameof(LearningResumedEvent));
                return;
            }

            logger.LogInformation("LearningResumedEvent for ApprenticeshipId {ApprenticeshipId} with ResumeDate {ResumeDate}",
                message.ApprenticeshipId, message.ResumeDate);
            var db = dbContext.Value;
            var apprentice = await db.Apprenticeships
                .Include(a => a.Cohort)
                    .ThenInclude(c => c.Provider)
                .SingleOrDefaultAsync(t => t.Id == message.ApprenticeshipId);

            if (apprentice == null)
            {
                throw new DomainException(nameof(apprentice), $"Apprenticeship with Id {message.ApprenticeshipId} not found.");
            }

            if (apprentice.PaymentStatus == PaymentStatus.Active && !apprentice.PauseDate.HasValue)
            {
                logger.LogInformation("Apprenticeship {ApprenticeshipId} is already active and resumed.", apprentice.Id);
            }
            else
            {
                ValidateResumeDate(message.ResumeDate, apprentice);

                apprentice.SetIlrResumed(message.ResumeDate);

                logger.LogInformation("{Event} : Notification Email Sending to Employer for apprenticeship: {apprenticeshipId}", nameof(LearningResumedEvent), message.ApprenticeshipId);
                await SendEmailNotificationToEmployer(context, apprentice.Cohort.EmployerAccountId, apprentice.Cohort.Provider.Name, apprentice.Id);

                var historyCommand = new StoreLearningHistoryCommand
                {
                    ApprenticeshipId = message.ApprenticeshipId,
                    Source = LearningSourceType.ILRStatusChange,
                    ChangeType = LearningChangeType.AutoApproved,
                    LearningKey = message.LearningKey,
                    AppliedDate = message.Created,
                    Description = $"Learning has been resumed on {message.ResumeDate.ToGdsFormat()}"
                };
                await context.Send(historyCommand);

                logger.LogInformation(" Executing {Event} completed", nameof(LearningResumedEvent));
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing LearningResumedEventHandler for ApprenticeshipId {0}", message?.ApprenticeshipId);
            throw;
        }
    }

    private void ValidateResumeDate(DateTime resumeDate, Apprenticeship apprenticeship)
    {
        if (apprenticeship.PaymentStatus == PaymentStatus.Completed || apprenticeship.PaymentStatus == PaymentStatus.Withdrawn)
        {
            throw new DomainException(nameof(resumeDate), "Learning cannot be Resumed if Payment Status is Completed or Withdrawn. Unable to resume apprenticeship");
        }

        if (apprenticeship.StartDate.HasValue && apprenticeship.StartDate.Value.Date >= resumeDate.Date)
        {
            throw new DomainException(nameof(resumeDate), "Invalid resume date. Learner not started.");
        }

        if (apprenticeship.EndDate.HasValue && resumeDate.Date >= apprenticeship.EndDate.Value.Date)
        {
            throw new DomainException(nameof(resumeDate), "Invalid resume date. Resume date cannot be on or after the end date.");
        }

        if (apprenticeship.PauseDate.HasValue && resumeDate.Date < apprenticeship.PauseDate.Value.Date)
        {
            throw new DomainException(nameof(resumeDate), "Invalid resume date. Resume date cannot be before the pausedate.");
        }

        if (!apprenticeship.PauseDate.HasValue && resumeDate.Date != DateTime.MinValue)
        {
            logger.LogInformation("Apprenticeship paused date is missing for apprenticeship {ApprenticeshipId}.", apprenticeship.Id);
        }
    }

    private async Task SendEmailNotificationToEmployer(IMessageHandlerContext context, long accountId, string providerName, long apprenticeshipId)
    {
        var encodedApprenticeshipId = encodingService.Encode(apprenticeshipId, EncodingType.ApprenticeshipId);
        var encodedAccountId = encodingService.Encode(accountId, EncodingType.AccountId);

        var sendEmailToEmployerCommand = new SendEmailToEmployerCommand(
            accountId,
            "EmployerApprenticeshipResumedNotification",
            new Dictionary<string, string>
            {
                {"provider_name", providerName},
                {
                    "url",
                    $"{commitmentsV2Configuration.EmployerCommitmentsBaseUrl}{encodedAccountId}/apprentices/{encodedApprenticeshipId}/details"
                }
            },
            null,
            "name"
            );

        logger.LogInformation("Sending EmployerApprenticeshipResumedNotification Email for id {0} hashed as {1}", apprenticeshipId, encodedApprenticeshipId);
        await context.Send(sendEmailToEmployerCommand);
    }
}
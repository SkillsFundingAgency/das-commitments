using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;

public class LearnerWithdrawnEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICurrentDateTime currentDate,
    IOverlapCheckService overlapCheckService,
    IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService,
    ILogger<LearnerWithdrawnEventHandler> logger)
    : IHandleMessages<LearningWithdrawnEvent>
{
    public async Task Handle(LearningWithdrawnEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("LearningWithdrawnEvent for ApprenticeshipId {ApprenticeshipId} with WithdrawalDate {WithdrawalDate} and WithdrawalReasonCode {WithdrawalReasonCode}",
                message.ApprenticeshipId, message.WithdrawalDate, message.WithdrawalReasonCode);
            var db = dbContext.Value;
            var apprentice = await db.GetApprenticeshipAggregate(message.ApprenticeshipId, default);
            
            var withdrawalDate = new DateTime(message.WithdrawalDate.Year, message.WithdrawalDate.Month, 1);
            if (message.WithdrawalReasonCode < 0)
            {
                throw new DomainException(nameof(message.WithdrawalReasonCode), "Invalid WithdrawalReasonCode. The reason code can not be negative.");
            }
            ValidateStopDateForWithdrawal(withdrawalDate, apprentice);
            await ValidateEndDateOverlap(withdrawalDate, apprentice, default);

            apprentice.SetIlrWithdrawn(withdrawalDate, message.WithdrawalReasonCode);
            await resolveOverlappingTrainingDateRequestService.Resolve(apprentice.Id, null, OverlappingTrainingDateRequestResolutionType.StopDateUpdate);

            var historyCommand = new StoreLearningHistoryCommand
            {
                ApprenticeshipId = message.ApprenticeshipId,
                Source = LearningSourceType.ILRStatusChange,
                ChangeType = LearningChangeType.AutoApproved,
                LearningKey = message.LearningKey,
                AppliedDate = message.Created,
                Description = $"ILR Learner status changed from Live to Withdrawn due to {message.WithdrawalReasonCode}"
            };
            await context.Send(historyCommand);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing LearnerWithdrawnEventHandler for ApprenticeshipId {0}", message.ApprenticeshipId);
            throw;
        }
    }

    private void ValidateStopDateForWithdrawal(DateTime stopDate, Apprenticeship apprenticeship)
    {
        if (apprenticeship.PaymentStatus == PaymentStatus.Completed)
        {
            var ex = new DomainException(nameof(stopDate), "Apprenticeship cannot be Stopped if Payment Status is Completed. Unable to stop apprenticeship");
            throw ex;
        }

        if (apprenticeship.IsWaitingToStart(currentDate))
        {
            if (stopDate.Date != apprenticeship.StartDate.Value.Date)
            {
                throw new DomainException(nameof(stopDate), "Invalid stop date. Date should be value of start date if training has not started.");
            }
        }
        else
        {
            if (stopDate.Date > new DateTime(currentDate.UtcNow.Year, currentDate.UtcNow.Month, 1))
            {
                throw new DomainException(nameof(stopDate), "Invalid Stop Date. Stop date cannot be in the future and must be the 1st of the month.");
            }

            if (stopDate.Date < new DateTime(apprenticeship.StartDate.Value.Year, apprenticeship.StartDate.Value.Month, 1))
            {
                throw new DomainException(nameof(stopDate), "Invalid Stop Date. Stop date cannot be before the apprenticeship has started.");
            }
        }

        if(stopDate.Day != 1)
        {
            throw new DomainException(nameof(stopDate), "Invalid Stop Date. Stop date must be the 1st of the month.");
        }
    }

    private async Task ValidateEndDateOverlap(DateTime stopDate, Apprenticeship apprenticeship, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(apprenticeship.Uln) || !apprenticeship.StartDate.HasValue) return;

        var overlapResult = await overlapCheckService.CheckForOverlaps(apprenticeship.Uln, apprenticeship.StartDate.Value.To(stopDate), apprenticeship.Id, cancellationToken);

        if (!overlapResult.HasOverlaps) return;

        const string errorMessage = "The date overlaps with existing dates for the same apprentice";

        var errors = new List<DomainError> { new("stopDate", errorMessage) };

        throw new DomainException(errors);
    }
}
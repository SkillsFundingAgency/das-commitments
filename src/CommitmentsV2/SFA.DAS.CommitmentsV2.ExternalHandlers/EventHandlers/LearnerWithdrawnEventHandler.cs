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
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;


namespace SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;

public class LearnerWithdrawnEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICurrentDateTime currentDate,
    IOverlapCheckService overlapCheckService,
    IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService,
    ILogger<LearnerWithdrawnEventHandler> logger)
    : IHandleMessages<LearnerWithdrawnEvent>
{
    public async Task Handle(LearnerWithdrawnEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("LearnerWithdrawnEvent for ApprenticeshipId {ApprenticeshipId} with WithdrawnDate {WithdrawnDate} and WithdrawnReasonCode {WithdrawnReasonCode}",
                message.ApprenticeshipId, message.WithdrawnDate, message.WithdrawnReasonCode);
            var db = dbContext.Value;
            var apprentice = await db.GetApprenticeshipAggregate(message.ApprenticeshipId, default);
            ValidateStopDateForWithdrawal(message.WithdrawnDate, apprentice);
            await ValidateEndDateOverlap(message.WithdrawnDate, apprentice, default);

            apprentice.SetIlrWithdrawn(message.WithdrawnDate, message.WithdrawnReasonCode);
            await resolveOverlappingTrainingDateRequestService.Resolve(apprentice.Id, null, OverlappingTrainingDateRequestResolutionType.StopDateUpdate);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing LearnerWithdrawnEventHandler for ApprenticeshipId {0}", message.ApprenticeshipId);
            throw;
        }
    }

    private void ValidateStopDateForWithdrawal(DateTime stopDate, Apprenticeship apprenticeship)
    {
        if (apprenticeship == null)
        {
            throw new ArgumentException(null, nameof(apprenticeship));
        }

        if (apprenticeship.PaymentStatus == PaymentStatus.Completed)
        {
            throw new DomainException(nameof(PaymentStatus), "Apprenticeship cannot be Stopped if Payment Status is Completed. Unable to stop apprenticeship");
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
    }

    private async Task ValidateEndDateOverlap(DateTime stopDate, Apprenticeship apprenticeship, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(apprenticeship.Uln) || !apprenticeship.StartDate.HasValue) return;

        var overlapResult = await overlapCheckService.CheckForOverlaps(apprenticeship.Uln, apprenticeship.StartDate.Value.To(stopDate), apprenticeship.Id, cancellationToken);

        if (!overlapResult.HasOverlaps) return;

        const string errorMessage = "The date overlaps with existing dates for the same apprentice";

        var errors = new List<DomainError> { new("newStopDate", errorMessage) };

        throw new DomainException(errors);
    }
}

// Will be removed once Learning creates the message
public class LearnerWithdrawnEvent
{
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
    public DateTime Created { get; set; }
    public DateTime WithdrawnDate { get; set; }
    public int WithdrawnReasonCode { get; set; }
}
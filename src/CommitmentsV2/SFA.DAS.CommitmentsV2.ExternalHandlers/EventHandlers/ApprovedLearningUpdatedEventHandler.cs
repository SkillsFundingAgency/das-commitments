using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;

public class ApprovedLearningUpdatedEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<ApprovedLearningUpdatedEventHandler> logger) : IHandleMessages<ApprovedLearningUpdatedEvent>

{
    public async Task Handle(ApprovedLearningUpdatedEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("Started executing {Event}", nameof(ApprovedLearningUpdatedEvent));

            if (message is null)
            {
                logger.LogInformation(" {Event} received null message", nameof(ApprovedLearningUpdatedEvent));
                return;
            }

            logger.LogInformation("ApprovedLearningUpdatedEvent for ApprenticeshipId {ApprenticeshipId} with LearningKey {LearningKey}",
                message.ApprenticeshipId, message.LearningKey);
            var db = dbContext.Value;
            var apprentice = await db.Apprenticeships
                .Include(a => a.PriceHistory)
                .SingleOrDefaultAsync(t => t.Id == message.ApprenticeshipId);

            if (apprentice == null)
            {
                throw new DomainException(nameof(apprentice), $"Apprenticeship with Id {message.ApprenticeshipId} not found.");
            }

            foreach (var change in message.Changes)
            {
                ApplyChange(message, apprentice, change);
            }

            if(message.Prices != null && message.Prices.Any())
            {
                ApplyPriceChanges(message, apprentice);
            }

            logger.LogInformation(" Executing {Event} completed", nameof(ApprovedLearningUpdatedEvent));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing ApprovedLearningUpdatedEventHandler for ApprenticeshipId {0}", message?.ApprenticeshipId);
            throw;
        }
    }

    private void ApplyChange(ApprovedLearningUpdatedEvent message, Models.Apprenticeship apprentice, ApprenticeshipFieldChange change)
    {
        if (change?.Data == null) return;

        if (Enum.TryParse<ApprovedLearnerChangeType>(change.ChangeType, ignoreCase: true, out var changeType))
        {
            switch (changeType)
            {
                case ApprovedLearnerChangeType.Firstname:
                    apprentice.FirstName = change.Data.New;
                    break;

                case ApprovedLearnerChangeType.Surname:
                    apprentice.LastName = change.Data.New;
                    break;

                case ApprovedLearnerChangeType.DOB:
                    ApplyDOBChange(message, apprentice, change);
                    break;

                case ApprovedLearnerChangeType.PlannedStartDate:
                    var parsedStartDate = ParseDate(change.Data.New);
                    if (parsedStartDate == null)
                    {
                        logger.LogWarning("Invalid date for PlannedStartDate change for ApprenticeshipId {ApprenticeshipId}: {NewValue}", message.ApprenticeshipId, change.Data.New);
                        return;
                    }

                    apprentice.StartDate = ParseFirstDayOfMonth(parsedStartDate);
                    apprentice.ActualStartDate = parsedStartDate;
                    break;

                case ApprovedLearnerChangeType.PlannedEndDate:
                    var parsedEndDate = ParseDate(change.Data.New);
                    if (parsedEndDate == null)
                    {
                        logger.LogWarning("Invalid date for PlannedEndDate change for ApprenticeshipId {ApprenticeshipId}: {NewValue}", message.ApprenticeshipId, change.Data.New);
                        return;
                    }
                    apprentice.EndDate = ParseFirstDayOfMonth(parsedEndDate);
                    break;

                case ApprovedLearnerChangeType.Email:
                    apprentice.Email = change.Data.New;
                    break;
            }
        }
        else
        {
            logger.LogWarning("Unknown change type '{ChangeType}' for ApprenticeshipId {ApprenticeshipId}", change.ChangeType, message?.ApprenticeshipId);
        }

        return;
    }

    private void ApplyPriceChanges(ApprovedLearningUpdatedEvent message, Apprenticeship apprentice)
    {
        try
        {
            logger.LogInformation("Processing PriceEpisodes for ApprenticeshipId {ApprenticeshipId}", message.ApprenticeshipId);

            var currentCost = apprentice.Cost;
            var currentTrainingPrice = apprentice.TrainingPrice;
            var currentAssessmentPrice = apprentice.EndPointAssessmentPrice;

            var priceEpisodes = message.Prices.OrderBy(pe => pe.EffectiveFrom).ToList();
            logger.LogInformation("Clearing PriceEpisodes for ApprenticeshipId {ApprenticeshipId}", message.ApprenticeshipId);

            var db = dbContext.Value;
            foreach (var item in apprentice.PriceHistory)
            {
                db.PriceHistory.Remove(item);
            }

            for (var i = 0; i < priceEpisodes.Count; i++)
            {
                var newPrice = new PriceHistory
                {
                    ApprenticeshipId = apprentice.Id,
                    Cost = priceEpisodes[i].TrainingPrice + priceEpisodes[i].AssessmentPrice,
                    TrainingPrice = priceEpisodes[i].TrainingPrice,
                    AssessmentPrice = priceEpisodes[i].AssessmentPrice,
                    FromDate = priceEpisodes[i].EffectiveFrom,
                    ToDate = i < priceEpisodes.Count - 1 ? priceEpisodes[i + 1].EffectiveFrom.AddDays(-1) : null
                };

                if (newPrice.FromDate <= DateTime.Today && (newPrice.ToDate == null || newPrice.ToDate >= DateTime.Today))
                {
                    currentCost = newPrice.Cost;
                    currentTrainingPrice = (int)newPrice.TrainingPrice;
                    currentAssessmentPrice = (int)newPrice.AssessmentPrice;
                }
                logger.LogInformation("Adding new PriceEpisode for ApprenticeshipId {ApprenticeshipId}", message.ApprenticeshipId);
                apprentice.PriceHistory.Add(newPrice);
            }

            apprentice.Cost = currentCost;
            apprentice.TrainingPrice = currentTrainingPrice;
            apprentice.EndPointAssessmentPrice = currentAssessmentPrice;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing PriceEpisodes for ApprenticeshipId {ApprenticeshipId}", message.ApprenticeshipId);
            throw;
        }
    }

    private void ApplyDOBChange(ApprovedLearningUpdatedEvent message, Models.Apprenticeship apprentice, ApprenticeshipFieldChange change)
    {
        var parsedDOB = ParseDate(change.Data.New);
        if (parsedDOB == null)
        {
            logger.LogWarning("Invalid date for DOB change for ApprenticeshipId {ApprenticeshipId}: {NewValue}", message.ApprenticeshipId, change.Data.New);
            return;
        }
        apprentice.DateOfBirth = parsedDOB;
        return;
    }

    private static DateTime? ParseDate(string dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return null;
        }

        if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            return parsedDate;
        }
        return null;
    }

    private static DateTime? ParseFirstDayOfMonth(DateTime? date)
    {
        return date.HasValue ? new DateTime(date.Value.Year, date.Value.Month, 1) : null;
    }
}
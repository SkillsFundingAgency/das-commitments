﻿using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services;

public class OverlapCheckService(IUlnUtilisationService ulnUtilisationService, IEmailOverlapService emailOverlapService, Lazy<ProviderCommitmentsDbContext> dbContext)
    : IOverlapCheckService
{
    public async Task<OverlapCheckResult> CheckForOverlaps(string uln, DateRange range, long? existingApprenticeshipId, CancellationToken cancellationToken)
    {
        var overlapStartDate = false;
        var overlapEndDate = false;

        foreach (var utilisation in await GetCandidateUlnUtilisations(uln, existingApprenticeshipId, cancellationToken))
        {
            var overlapStatus = utilisation.DateRange.DetermineOverlap(range);

            switch (overlapStatus)
            {
                case OverlapStatus.OverlappingStartDate:
                    overlapStartDate = true;
                    break;
                case OverlapStatus.OverlappingEndDate:
                    overlapEndDate = true;
                    break;
                case OverlapStatus.DateWithin:
                case OverlapStatus.DateEmbrace:
                    overlapStartDate = true;
                    overlapEndDate = true;
                    break;
            }

            if (overlapStartDate && overlapEndDate)
            {
                break;
            }
        }

        return new OverlapCheckResult(overlapStartDate, overlapEndDate);
    }

    public async Task<OverlapCheckResultOnStartDate> CheckForOverlapsOnStartDate(string uln, DateRange range, long? existingApprenticeshipId, CancellationToken cancellationToken)
    {
        var overlapStartDate = false;
        long? apprenticeshipId = null;

        foreach (var utilisation in await GetCandidateUlnUtilisations(uln, existingApprenticeshipId, cancellationToken))
        {
            var overlapStatus = utilisation.DateRange.DetermineOverlap(range);

            switch (overlapStatus)
            {
                case OverlapStatus.DateWithin:
                case OverlapStatus.DateEmbrace:
                case OverlapStatus.OverlappingStartDate:
                    overlapStartDate = true;
                    apprenticeshipId = utilisation.ApprenticeshipId;

                    break;
            }

            if (overlapStartDate)
            {
                break;
            }
        }

        return new OverlapCheckResultOnStartDate(overlapStartDate, apprenticeshipId);
    }

    private async Task<IEnumerable<UlnUtilisation>> GetCandidateUlnUtilisations(string uln, long? existingApprenticeshipId, CancellationToken cancellationToken)
    {
        var utilisations = await ulnUtilisationService.GetUlnUtilisations(uln, cancellationToken);
        return existingApprenticeshipId.HasValue ? utilisations.Where(x => x.ApprenticeshipId != existingApprenticeshipId.Value) : utilisations;
    }

    public async Task<EmailOverlapCheckResult> CheckForEmailOverlaps(string email, DateRange range, long? existingApprenticeshipId, long? cohortId,
        CancellationToken cancellationToken)
    {
        var overlappingEmails = await emailOverlapService.GetOverlappingEmails(new EmailToValidate(email, range.From, range.To, existingApprenticeshipId), cohortId, cancellationToken);

        if (overlappingEmails.Count == 0)
        {
            return null;
        }

        var overlappingEmail = overlappingEmails.First();
        return new EmailOverlapCheckResult(overlappingEmail.RowId, overlappingEmail.OverlapStatus, overlappingEmail.IsApproved);
    }

    public async Task<List<EmailOverlapCheckResult>> CheckForEmailOverlaps(long cohortId, CancellationToken cancellationToken)
    {
        var overlappingEmails = await emailOverlapService.GetOverlappingEmails(cohortId, cancellationToken);

        var singleEmails = from overlap in overlappingEmails
            group overlap by overlap.RowId
            into groups
            select groups.OrderBy(e => e.RowId).First();

        var summary = singleEmails.Select(x => new EmailOverlapCheckResult(x.RowId, x.OverlapStatus, x.IsApproved));

        return summary.ToList();
    }

    public async Task<List<OverlapCheckResult>> CheckForOverlaps(long cohortId, CancellationToken cancellationToken)
    {
        var overlapCheckResult = new List<OverlapCheckResult>();

        var cohort = await dbContext.Value.Cohorts
            .Include(x => x.AccountLegalEntity)
            .Include(x => x.Apprenticeships)
            .Where(x => x.Id == cohortId).FirstOrDefaultAsync(cancellationToken);

        foreach (var apprentice in cohort.DraftApprenticeships)
        {
            if (apprentice.StartDate.HasValue && apprentice.EndDate.HasValue)
            {
                overlapCheckResult.Add(await CheckForOverlaps(apprentice.Uln,
                    new DateRange(apprentice.StartDate ?? apprentice.StartDate.Value, apprentice.EndDate ?? apprentice.EndDate.Value),
                    apprentice.Id,
                    CancellationToken.None));
            }
        }

        return overlapCheckResult.ToList();
    }
}
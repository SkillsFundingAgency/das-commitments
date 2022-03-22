using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class OverlapCheckService : IOverlapCheckService
    {
        private readonly IUlnUtilisationService _ulnUtilisationService;
        private readonly IEmailOverlapService _emailOverlapService;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public OverlapCheckService(IUlnUtilisationService ulnUtilisationService, IEmailOverlapService emailOverlapService, Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _ulnUtilisationService = ulnUtilisationService;
            _emailOverlapService = emailOverlapService;
            _dbContext = dbContext;
        }

        public async Task<OverlapCheckResult> CheckForOverlaps(string uln, DateRange range, long? existingApprenticeshipId, CancellationToken cancellationToken)
        {
            async Task<IEnumerable<UlnUtilisation>> GetCandidateUlnUtilisations()
            {
                var utilisations  = await _ulnUtilisationService.GetUlnUtilisations(uln, cancellationToken);
                return existingApprenticeshipId.HasValue ? utilisations.Where(x => x.ApprenticeshipId != existingApprenticeshipId.Value) : utilisations;
            }

            var overlapStartDate = false;
            var overlapEndDate = false;

            foreach (var utilisation in await GetCandidateUlnUtilisations())
            {
                var overlapStatus = utilisation.DateRange.DetermineOverlap(range);

                switch (overlapStatus)
                {
                    case OverlapStatus.OverlappingStartDate: overlapStartDate = true;
                        break;
                    case OverlapStatus.OverlappingEndDate: overlapEndDate = true;
                        break;
                    case OverlapStatus.DateWithin:
                    case OverlapStatus.DateEmbrace:
                        overlapStartDate = true;
                        overlapEndDate = true;
                        break;
                    default:
                        break;
                }

                if (overlapStartDate && overlapEndDate)
                {
                    break;
                }
            }

            return new OverlapCheckResult(overlapStartDate, overlapEndDate);
        }

        public async Task<EmailOverlapCheckResult> CheckForEmailOverlaps(string email, DateRange range, long? existingApprenticeshipId, long? cohortId,
            CancellationToken cancellationToken)
        {
            var overlappingEmails = await _emailOverlapService.GetOverlappingEmails(new EmailToValidate(email, range.From, range.To, existingApprenticeshipId), cohortId, cancellationToken);

            if (overlappingEmails.Count == 0)
            {
                return null;
            }

            var overlappingEmail = overlappingEmails.First();
            return new EmailOverlapCheckResult(overlappingEmail.RowId, overlappingEmail.OverlapStatus, overlappingEmail.IsApproved);
        }

        public async Task<List<EmailOverlapCheckResult>> CheckForEmailOverlaps(long cohortId, CancellationToken cancellationToken)
        {
            var overlappingEmails = await _emailOverlapService.GetOverlappingEmails(cohortId, cancellationToken);

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

            var cohort = _dbContext.Value.Cohorts
               .Include(x => x.AccountLegalEntity)
               .Include(x => x.Apprenticeships)
               .Where(x => x.Id == cohortId).FirstOrDefault();

            foreach (var apprentice in cohort.DraftApprenticeships)
            {
                if (apprentice.StartDate.HasValue && apprentice.EndDate.HasValue)
                {
                    overlapCheckResult.Add(await CheckForOverlaps(apprentice.Uln,
                          new DateRange(apprentice.StartDate ?? apprentice.StartDate.Value, apprentice.EndDate ?? apprentice.EndDate.Value),
                          null,
                          CancellationToken.None));
                }
            }

            return overlapCheckResult.ToList();
        }
    }
}

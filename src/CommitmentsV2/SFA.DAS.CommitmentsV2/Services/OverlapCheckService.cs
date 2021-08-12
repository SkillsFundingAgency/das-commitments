using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class OverlapCheckService : IOverlapCheckService
    {
        private readonly IUlnUtilisationService _ulnUtilisationService;
        private readonly IEmailOverlapService _emailOverlapService;

        public OverlapCheckService(IUlnUtilisationService ulnUtilisationService, IEmailOverlapService emailOverlapService)
        {
            _ulnUtilisationService = ulnUtilisationService;
            _emailOverlapService = emailOverlapService;
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
            return new EmailOverlapCheckResult(overlappingEmail.OverlapStatus, overlappingEmail.IsApproved);
        }
    }
}

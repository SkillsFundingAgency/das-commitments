using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class UlnUtilisationService : IUlnUtilisationService
    {
        private readonly IDbContextFactory _dbContextFactory;

        public UlnUtilisationService(IDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<UlnUtilisation[]> GetUlnUtilisations(string uln, CancellationToken cancellationToken)
        {
            using (var db = _dbContextFactory.CreateDbContext())
            {
                var liveApprenticeships = await db.Apprenticeships
                    .Where(ca => ca.Uln == uln)
                    .Select(x => new UlnUtilisation(x.Id,
                        x.Uln,
                        x.StartDate.Value,
                        CalculateOverlapApprenticeshipEndDate(x))).ToListAsync(cancellationToken);

                var draftApprenticeshipWithTransferSender = await db.DraftApprenticeships.Include(y => y.Cohort)
                    .Where(da => da.Uln == uln
                                 && da.StartDate.HasValue
                                 && da.EndDate.HasValue
                                 && da.Cohort.TransferSenderId.HasValue
                                 && da.Cohort.WithParty == Party.TransferSender)
                    .Select(x => new UlnUtilisation(x.Id,
                        x.Uln,
                        x.StartDate.Value,
                        x.EndDate.Value)).ToListAsync(cancellationToken);

                var result = liveApprenticeships.Union(draftApprenticeshipWithTransferSender).ToArray();

                return result;
            }
        }

        /// <summary>
        /// Calculates what date should be used as the overlap end date for an apprenticeship when validating start date / end date overlaps.
        /// </summary>
        private static DateTime CalculateOverlapApprenticeshipEndDate(Apprenticeship apprenticeship)
        {
            switch (apprenticeship.PaymentStatus)
            {
                case PaymentStatus.Withdrawn:
                    return apprenticeship.StopDate.Value;

                case PaymentStatus.Completed:
                    return new[] { apprenticeship.CompletionDate, apprenticeship.EndDate, apprenticeship.StopDate }
                          .Where(x => x.HasValue).Select(x => x.Value).Min();

                default:
                    return apprenticeship.EndDate.Value;
            }
        }
    }
}
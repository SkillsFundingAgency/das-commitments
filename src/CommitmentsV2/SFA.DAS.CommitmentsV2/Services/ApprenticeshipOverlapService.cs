using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Validators;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class ApprenticeshipOverlapService : IApprenticeshipOverlapService
    {
        private readonly IDbContextFactory _dbContextFactory;

        public ApprenticeshipOverlapService(IDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<OverlapStatus> CheckForOverlaps(
            DraftApprenticeshipDetails draftApprenticeshipDetails,
            CancellationToken cancellationToken)
        {
            using (var db = _dbContextFactory.CreateAccountsDbContext())
            {
                var apprenticeships = await db.DraftApprenticeships
                    .Where(da => da.Uln == draftApprenticeshipDetails.Uln)
                    .AsNoTracking()
                    .OfType<Apprenticeship>()
                    .ToListAsync(cancellationToken);

                apprenticeships.AddRange(await db.ConfirmedApprenticeships
                    .Where(ca => ca.Uln == draftApprenticeshipDetails.Uln)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken));

                return apprenticeships.DetermineOverlap(draftApprenticeshipDetails);
            }
        }

    }
}

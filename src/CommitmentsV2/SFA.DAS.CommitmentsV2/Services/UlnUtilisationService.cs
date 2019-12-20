using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

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
                var result = await db.Apprenticeships
                    .Where(ca => ca.Uln == uln)
                    .Select(x => new UlnUtilisation(x.Id,
                        x.Uln,
                        x.StartDate.Value,
                        x.PaymentStatus == PaymentStatus.Withdrawn
                            ? x.StopDate.Value
                            : x.EndDate.Value))
                    .ToArrayAsync(cancellationToken);

                return result;
            }
        }
    }
}

using System.Data;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.ApprovedApprenticeship;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Sql.Client;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class ApprovedApprenticeshipRepository: BaseRepository, IApprovedApprenticeshipRepository
    {
        public ApprovedApprenticeshipRepository(string connectionString, ICommitmentsLogger logger)
            : base(connectionString, logger.BaseLogger)
        {
        }

        public Task<ApprovedApprenticeship> Get(long id)
        {
            return WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", id, DbType.Int64);

                ApprovedApprenticeship result = null;

                using (var multi = await c.QueryMultipleAsync("GetApprovedApprenticeship", parameters, commandType: CommandType.StoredProcedure))
                {
                    multi.Read<ApprovedApprenticeship, PriceHistory, DataLockStatus, ApprovedApprenticeship>(
                        (apprenticeship, priceEpisode, dataLock) =>
                        {
                            if (result == null)
                            {
                                result = apprenticeship;
                            }

                            if (priceEpisode != null && !result.PriceEpisodes.Exists(x => x.Id == priceEpisode.Id))
                            {
                                result.PriceEpisodes.Add(priceEpisode);
                            }

                            if (dataLock != null && !result.DataLocks.Exists(x => x.Id == dataLock.Id))
                            {
                                result.DataLocks.Add(dataLock);
                            }

                            return result;
                        });
                }
                return result;
            });
        }
    }
}

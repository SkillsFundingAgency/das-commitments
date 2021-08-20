using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Sql.Client;
using SFA.DAS.Sql.Dapper;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class TrainingProgrammeRepository : BaseRepository, ITrainingProgrammeRepository
    {
        public TrainingProgrammeRepository(string connectionString, ILog logger) : base(connectionString, logger)
        {
        }
        
        public async Task<List<Standard>> GetAllStandards()
        {
            var lookup = new Dictionary<object, Standard>();
            var mapper = new ParentChildrenMapper<Standard, FundingPeriod>();
            
            return await WithConnection(async connection =>
            {
                var results = await connection.QueryAsync(
                    sql: $"[dbo].[GetStandards]",
                    commandType: CommandType.StoredProcedure,
                    map: mapper.Map(lookup, x => x.LarsCode, x => x.FundingPeriods),
                    splitOn: "LarsCode"
                    );

                return results.GroupBy(c => c.LarsCode).Select(item=>item.First()).ToList();
            });
        }
        
        public async Task<List<Framework>> GetAllFrameworks()
        {
            var lookup = new Dictionary<object, Framework>();
            var mapper = new ParentChildrenMapper<Framework, FundingPeriod>();
            return await WithConnection(async connection =>
            {
                var results = await connection.QueryAsync(
                    sql: $"[dbo].[GetFrameworks]",
                    commandType: CommandType.StoredProcedure,
                    map: mapper.Map(lookup, x => x.LarsCode, x => x.FundingPeriods),
                    splitOn: "Id"
                    );

                return results.GroupBy(c => c.LarsCode).Select(item=>item.First()).ToList();
            });
        }
    }
}
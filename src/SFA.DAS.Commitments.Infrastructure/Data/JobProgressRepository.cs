using System.Data;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class JobProgressRepository : BaseRepository, IJobProgressRepository
    {
        private readonly ICommitmentsLogger _logger;

        public JobProgressRepository(string connectionString, ICommitmentsLogger logger)
            : base(connectionString, logger.BaseLogger)
        {
            _logger = logger;
        }

        // column per progress tag, or row per tag?
        // column +ve : tags can be different data type
        // row +ve : unlimited tags, no schema change for extra tags unless require different data type
        //     -ve : tags all same data type, or muliple columns
        // https://stackoverflow.com/questions/3967372/sql-server-how-to-constrain-a-table-to-contain-a-single-row

        public async Task<long?> Get_AddEpaToApprenticeships_LastSubmissionEventId()
        {
            _logger.Debug("Getting last processed (by AddEpaToApprenticeships) SubmissionEventId");

            return await WithConnection(async connection => await connection.ExecuteScalarAsync<long?>(
                "SELECT [AddEpa_LastSubmissionEventId] FROM [dbo].[JobProgress]",
                commandType: CommandType.Text));
        }

        public async Task Set_AddEpaToApprenticeships_LastSubmissionEventId(long lastSubmissionEventId)
        {
            _logger.Debug($"Setting last processed (by AddEpaToApprenticeships) SubmissionEventId to {lastSubmissionEventId}");

            await WithConnection(async connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@lastSubmissionEventId", lastSubmissionEventId, DbType.Int64);

                return await connection.ExecuteAsync(
@"MERGE [dbo].[JobProgress] WITH(HOLDLOCK) as target
using (values(@lastSubmissionEventId)) as source (AddEpa_LastSubmissionEventId)
on target.Lock = 'X'
when matched then
  update set AddEpa_LastSubmissionEventId = source.AddEpa_LastSubmissionEventId
when not matched then
  insert (AddEpa_LastSubmissionEventId) values (source.AddEpa_LastSubmissionEventId);",
                    param: parameters,
                    commandType: CommandType.Text);
            });
        }
    }
}
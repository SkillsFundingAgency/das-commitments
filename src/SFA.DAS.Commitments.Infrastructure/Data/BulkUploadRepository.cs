using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Dapper;

using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class BulkUploadRepository : BaseRepository, IBulkUploadRepository
    {
        public BulkUploadRepository(string connectionString)
            : base(connectionString)
        {
        }

        public async Task<long> InsertBulkUploadFile(string file)
        {
            return await WithTransaction(
                async (connection, transaction) =>
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@fileContent", file, DbType.String);
                        parameters.Add("@createdOn", DateTime.UtcNow, DbType.DateTime);
                        
                        var hej = (await connection
                            .QueryAsync<long>(
                                sql:"INSERT INTO [dbo].[BulkUpload](FileContent,CreatedOn)"
                                    + "VALUES (@fileContent,@createdOn)" 
                                    + "SELECT CAST(SCOPE_IDENTITY() as BIGINT);", 
                                param:parameters,
                                commandType: CommandType.Text,
                                transaction: transaction)).Single();

                        return hej;
                    });
        }

        public async Task<string> GetBulkUploadFile(long bulkUploadId)
        {
            return await WithTransaction(
                async (connection, transaction) =>
                {
                    var parameters = new DynamicParameters();
                    parameters.Add($"@id", bulkUploadId, DbType.Int64);

                    var hej = (await connection
                        .QueryAsync<string>(
                            sql: "SELECT FileContent FROM [dbo].[BulkUpload]"
                                + "WHERE Id = @id;",
                            param: parameters,
                            commandType: CommandType.Text,
                            transaction: transaction)).SingleOrDefault();

                    return hej;
                });
        }
    }
}

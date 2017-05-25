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

        public async Task<long> InsertBulkUploadFile(string file, string fileName, long commitmentId)
        {
            return await WithTransaction(
                async (connection, transaction) =>
                    {
                        var truncatedFileName = fileName;
                        if(fileName.Length > 50)
                            truncatedFileName = fileName.Substring(0, 50);
                        var parameters = new DynamicParameters();
                        parameters.Add("@commitmentId", commitmentId, DbType.Int64);
                        parameters.Add("@fileName", truncatedFileName, DbType.String);
                        parameters.Add("@fileContent", file, DbType.String);
                        parameters.Add("@createdOn", DateTime.UtcNow, DbType.DateTime);
                        
                        var bulkUploadId = (await connection
                            .QueryAsync<long>(
                                sql:"INSERT INTO [dbo].[BulkUpload](CommitmentId,FileName,FileContent,CreatedOn)"
                                    + "VALUES (@commitmentId,@fileName,@fileContent,@createdOn)" 
                                    + "SELECT CAST(SCOPE_IDENTITY() as BIGINT);", 
                                param:parameters,
                                commandType: CommandType.Text,
                                transaction: transaction)).Single();

                        return bulkUploadId;
                    });
        }

        public async Task<string> GetBulkUploadFile(long bulkUploadId)
        {
            return await WithTransaction(
                async (connection, transaction) =>
                {
                    var parameters = new DynamicParameters();
                    parameters.Add($"@id", bulkUploadId, DbType.Int64);

                    var data = (await connection
                        .QueryAsync<string>(
                            sql: "SELECT FileContent FROM [dbo].[BulkUpload]"
                                + "WHERE Id = @id;",
                            param: parameters,
                            commandType: CommandType.Text,
                            transaction: transaction)).SingleOrDefault();

                    return data;
                });
        }
    }
}

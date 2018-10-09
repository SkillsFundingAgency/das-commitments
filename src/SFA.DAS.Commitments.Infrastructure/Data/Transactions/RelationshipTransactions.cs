using System.Data;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Data.Transactions
{
    public class RelationshipTransactions : IRelationshipTransactions
    {
        private readonly ICommitmentsLogger _logger;
        private readonly ICurrentDateTime _currentDateTime;

        public RelationshipTransactions(ICommitmentsLogger logger, ICurrentDateTime currentDateTime)
        {
            _logger = logger;
            _currentDateTime = currentDateTime;
        }

        public async Task<long> CreateRelationship(IDbConnection connection, IDbTransaction trans, Relationship relationship)
        {
            _logger.Debug(
                $"Creating relationship between Provider {relationship.ProviderId}, Employer {relationship.EmployerAccountId}, Legal Entity: {relationship.LegalEntityId}");

                var parameters = new DynamicParameters();
                parameters.Add("@ProviderId", relationship.ProviderId, DbType.Int64);
                parameters.Add("@ProviderName", relationship.ProviderName, DbType.String);
                parameters.Add("@LegalEntityId", relationship.LegalEntityId, DbType.String);
                parameters.Add("@LegalEntityName", relationship.LegalEntityName, DbType.String);
                parameters.Add("@LegalEntityAddress", relationship.LegalEntityAddress, DbType.String);
                parameters.Add("@LegalEntityOrganisationType", relationship.LegalEntityOrganisationType, DbType.Int16);
                parameters.Add("@EmployerAccountId", relationship.EmployerAccountId, DbType.String);
                parameters.Add("@Verified", relationship.Verified, DbType.Boolean);
                parameters.Add("@CreatedOn", _currentDateTime.Now, DbType.DateTime);

                return await connection.ExecuteAsync(
                    sql: "[dbo].[CreateRelationship]",
                    param: parameters,
                    transaction: trans,
                    commandType: CommandType.StoredProcedure);
        }
    }
}

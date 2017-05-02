using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Infrastructure.Data.Transactions
{
    public class ApprenticeshipUpdateTransactions : IApprenticeshipUpdateTransactions
    {
        public async Task<long> CreateApprenticeshipUpdate(IDbConnection connection, IDbTransaction trans, ApprenticeshipUpdate apprenticeshipUpdate)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@apprenticeshipId", apprenticeshipUpdate.ApprenticeshipId);
            parameters.Add("@Originator", apprenticeshipUpdate.Originator);
            parameters.Add("@FirstName", apprenticeshipUpdate.FirstName);
            parameters.Add("@LastName", apprenticeshipUpdate.LastName);
            parameters.Add("@TrainingType", apprenticeshipUpdate.TrainingType);
            parameters.Add("@TrainingCode", apprenticeshipUpdate.TrainingCode);
            parameters.Add("@TrainingName", apprenticeshipUpdate.TrainingName);
            parameters.Add("@Cost", apprenticeshipUpdate.Cost);
            parameters.Add("@StartDate", apprenticeshipUpdate.StartDate);
            parameters.Add("@EndDate", apprenticeshipUpdate.EndDate);
            parameters.Add("@DateOfBirth", apprenticeshipUpdate.DateOfBirth);
            parameters.Add("@CreatedOn", DateTime.UtcNow);
            parameters.Add("@UpdateOrigin", apprenticeshipUpdate.UpdateOrigin);
            parameters.Add("@EffectiveFromDate", apprenticeshipUpdate.EffectiveFromDate);
            parameters.Add("@EffectiveToDate", apprenticeshipUpdate.EffectiveToDate);
		
            return await connection.ExecuteAsync(
                    sql: $"[dbo].[CreateApprenticeshipUpdate]",
                    param: parameters,
                    transaction: trans,
                    commandType: CommandType.StoredProcedure);
        }

        public async Task<long> UpdateApprenticeshipReferenceAndUln(IDbConnection connection, IDbTransaction trans, Apprenticeship apprenticeship)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@id", apprenticeship.Id);
            parameters.Add("@uln", apprenticeship.ULN);
            parameters.Add("@employerRef", apprenticeship.EmployerRef);
            parameters.Add("@providerRef", apprenticeship.ProviderRef);

            var returnCode = await connection.ExecuteAsync(
                sql: "UPDATE [dbo].[Apprenticeship] " +
                    "SET ULN = COALESCE(@uln, uln), " +
                     "EmployerRef = COALESCE(@employerRef,EmployerRef), " +
                     "ProviderRef = COALESCE(@providerRef, ProviderRef)" +
                    "WHERE Id = @id;",
                param: parameters,
                transaction: trans,
                commandType: CommandType.Text);

            return returnCode;
        }
    }
}

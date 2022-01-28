using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Data.Transactions
{
    public class ApprenticeshipTransactions : IApprenticeshipTransactions
    {
        private readonly ICommitmentsLogger _logger;
        private readonly ICurrentDateTime _currentDateTime;

        public ApprenticeshipTransactions(ICommitmentsLogger logger, ICurrentDateTime currentDateTime)
        {
            if(logger == null)
                throw new ArgumentNullException(nameof(logger));
            if(currentDateTime == null)
                throw new ArgumentNullException(nameof(currentDateTime));

            _logger = logger;
            _currentDateTime = currentDateTime;
        }

        public DynamicParameters GetApprenticeshipUpdateCreateParameters(Apprenticeship apprenticeship)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@commitmentId", apprenticeship.CommitmentId, DbType.Int64);
            parameters.Add("@firstName", apprenticeship.FirstName, DbType.String);
            parameters.Add("@lastName", apprenticeship.LastName, DbType.String);
            parameters.Add("@dateOfBirth", apprenticeship.DateOfBirth, DbType.DateTime);
            parameters.Add("@niNumber", apprenticeship.NINumber, DbType.String);
            parameters.Add("@trainingType", apprenticeship.TrainingType, DbType.Int32);
            parameters.Add("@trainingCode", apprenticeship.TrainingCode, DbType.String);
            parameters.Add("@trainingName", apprenticeship.TrainingName, DbType.String);
            parameters.Add("@uln", apprenticeship.ULN, DbType.String);
            parameters.Add("@cost", apprenticeship.Cost, DbType.Decimal);
            parameters.Add("@startDate", apprenticeship.StartDate, DbType.DateTime);
            parameters.Add("@endDate", apprenticeship.EndDate, DbType.DateTime);
            parameters.Add("@paymentStatus", apprenticeship.PaymentStatus, DbType.Int16);
            parameters.Add("@employerRef", apprenticeship.EmployerRef, DbType.String);
            parameters.Add("@providerRef", apprenticeship.ProviderRef, DbType.String);
            parameters.Add("@createdOn", _currentDateTime.Now, DbType.DateTime);
            return parameters;
        }

        public async Task<int> UpdateApprenticeship(IDbConnection connection, IDbTransaction trans, Apprenticeship apprenticeship, Caller caller)
        {
            var parameters = GetApprenticeshipUpdateCreateParameters(apprenticeship);
            parameters.Add("@id", apprenticeship.Id, DbType.Int64);

            var refItem = caller.CallerType == CallerType.Employer ? "EmployerRef = @employerRef" : "ProviderRef = @providerRef";

            var returnCode = await connection.ExecuteAsync(
                sql: "UPDATE [dbo].[Apprenticeship] " +
                    "SET CommitmentId = @commitmentId, FirstName = @firstName, LastName = @lastName, DateOfBirth = @dateOfBirth, NINUmber = @niNumber, " +
                    "ULN = @uln, TrainingType = @trainingType, TrainingCode = @trainingCode, TrainingName = @trainingName, Cost = @cost, " +
                    "StartDate = @startDate, EndDate = @endDate, PaymentStatus = @paymentStatus, " +
                    $"{refItem} WHERE Id = @id;",
                param: parameters,
                transaction: trans,
                commandType: CommandType.Text);

            return returnCode;
        }

        public async Task UpdateCurrentPrice(IDbConnection connection, IDbTransaction trans, Apprenticeship apprenticeship)
        {
            var paras = new DynamicParameters();
            paras.Add("@apprenticeshipId", apprenticeship.Id, DbType.Int64);
            paras.Add("@cost", apprenticeship.Cost, DbType.Decimal);
            paras.Add("@fromDate", apprenticeship.StartDate, DbType.DateTime);
            paras.Add("@now", _currentDateTime.Now.ToString("o"), DbType.DateTime);

            await connection.ExecuteAsync(
                sql: " UPDATE[dbo].[PriceHistory] " 
                     + "SET Cost = @cost, FromDate = @fromDate "
                     + "WHERE ApprenticeshipId = @apprenticeshipId "
                     + "AND( (FromDate <= @now AND ToDate >= FORMAT(@now, 'yyyMMdd')) "
                     + "OR ToDate IS NULL);", 
                param: paras,
                transaction: trans,
                commandType: CommandType.Text);
        }
    }
}
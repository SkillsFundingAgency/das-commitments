using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastMember;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Infrastructure.Configuration;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup
{
    public class CommitmentsDatabase
    {
        private CommitmentsApiConfiguration _config;

        public CommitmentsDatabase()
        {
            //pass in?
            //load config from local app.config instead?

            _config = Infrastructure.Configuration.Configuration.Get();
        }

        public async Task InsertApprenticeships(List<DbSetupApprenticeship> apprenticeships)
        {
            await BulkInsertRows(apprenticeships, "[dbo].[Apprenticeship]", new []
            {
                "Id", //todo: incrementing
                "CommitmentId",
                //todo: public string Reference",
                "FirstName",
                "LastName",
                "ULN",
                "TrainingType",
                "TrainingCode",
                "TrainingName",
                "Cost",
                "StartDate",
                "EndDate",
                "AgreementStatus",
                "PaymentStatus",
                "DateOfBirth",
                "NINumber",
                "EmployerRef",
                "ProviderRef",
                "CreatedOn",
                "AgreedOn",
                "PaymentOrder"

                //"PauseDate",
                //"StopDate",
                //"HasHadDataLockSuccess"
            });
        }

        public async Task InsertCommitments(List<DbSetupCommitment> commitments)
        {
            await BulkInsertRows(commitments, "[dbo].[Commitment]", new[]
            {
                "Id", "Reference", "EmployerAccountId", "LegalEntityId", "LegalEntityName",
                "LegalEntityAddress", "LegalEntityOrganisationType", "ProviderId",
                "ProviderName", "CommitmentStatus", "EditStatus", "CreatedOn",
                "LastAction", "LastUpdatedByEmployerName", "LastUpdatedByEmployerEmail",
                "LastUpdatedByProviderName","LastUpdatedByProviderEmail"
            });
        }

        public async Task InsertDataLockStatuses(List<DbSetupDataLockStatus> dataLockStatuses)
        {
            await BulkInsertRows(dataLockStatuses, "[dbo].[DataLockStatus]", new[]
            {
                "Id", "DataLockEventId", "DataLockEventDatetime", "PriceEpisodeIdentifier",
                "ApprenticeshipId", "IlrTrainingCourseCode", "IlrTrainingType",
                "IlrActualStartDate", "IlrEffectiveFromDate", "IlrPriceEffectiveToDate",
                "IlrTotalCost", "ErrorCode", "Status", "TriageStatus",
                "ApprenticeshipUpdateId", "IsResolved", "EventStatus", "IsExpired",
                "Expired"
            });
        }

        public async Task BulkInsertRows<T>(List<T> rowData, string tableName, string[] columnNamesInTableOrder)
        {
            using (var connection = new SqlConnection(_config.DatabaseConnectionString))
            {
                await connection.OpenAsync();
                using (var bcp = new SqlBulkCopy(connection))
                using (var reader = ObjectReader.Create(rowData, columnNamesInTableOrder))
                {
                    bcp.DestinationTableName = tableName;
                    await bcp.WriteToServerAsync(reader);
                }
            }
        }
    }
}
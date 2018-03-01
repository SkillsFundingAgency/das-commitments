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
        public async Task InsertApprenticeships(List<DbSetupApprenticeship> apprenticeships)
        {
            //helpers/class for this bit
            var config = Infrastructure.Configuration.Configuration.Get();

            using (var connection = new SqlConnection(config.DatabaseConnectionString))
            {
                await connection.OpenAsync();
                using (var bcp = new SqlBulkCopy(connection))
                using (var reader = ObjectReader.Create(apprenticeships,
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
                ))
                {
                    bcp.DestinationTableName = "[dbo].[Apprenticeship]";
                    await bcp.WriteToServerAsync(reader);
                }
            }
        }

        public async Task InsertCommitments(List<DbSetupCommitment> commitments)
        {
            //load config from local app.config instead?
            var config = Infrastructure.Configuration.Configuration.Get();

            using (var connection = new SqlConnection(config.DatabaseConnectionString))
            {
                await connection.OpenAsync();
                using (var bcp = new SqlBulkCopy(connection))
                using (var reader = ObjectReader.Create(commitments,
                    "Id", "Reference", "EmployerAccountId", "LegalEntityId", "LegalEntityName",
                    "LegalEntityAddress", "LegalEntityOrganisationType", "ProviderId",
                    "ProviderName", "CommitmentStatus", "EditStatus", "CreatedOn",
                    "LastAction", "LastUpdatedByEmployerName", "LastUpdatedByEmployerEmail",
                    "LastUpdatedByProviderName","LastUpdatedByProviderEmail"
                ))
                {
                    bcp.DestinationTableName = "[dbo].[Commitment]";
                    await bcp.WriteToServerAsync(reader);
                }
            }
        }
    }
}
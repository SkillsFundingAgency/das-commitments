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
                    "CommitmentId",
                    //todo: public string Reference",
                    "FirstName",
                    "LastName",
                    "DateOfBirth",
                    "NINumber",
                    "ULN",
                    "TrainingType",
                    "TrainingCode",
                    "TrainingName",
                    "Cost",
                    "StartDate",
                    "EndDate",
                    "PauseDate",
                    "StopDate",
                    "PaymentStatus",
                    "AgreementStatus",
                    "EmployerRef",
                    "ProviderRef",
                    "HasHadDataLockSuccess"
                    ))
                {
                    bcp.DestinationTableName = "[dbo].[Apprenticeship]";
                    await bcp.WriteToServerAsync(reader);
                }
            }
        }
    }
}

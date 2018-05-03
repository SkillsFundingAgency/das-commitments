using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.ApiHost;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using OrganisationType = SFA.DAS.Common.Domain.Types.OrganisationType;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Tests.API
{
    [TestFixture]
    public sealed class GetApprenticeship
    {
        private const string GetEmployerApprenticeshipEmployerId = "GetApprenticeship_GetEmployerApprenticeship_EmployerId";
        private const string GetEmployerApprenticeshipApprenticeshipId = "GetApprenticeship_GetEmployerApprenticeship_ApprenticeshipId";

        public static void InjectTestSpecificData(TestDataInjector injector)
        {
            //todo: clone, to avoid mutations
            var commitmentId = injector.AddCommitment(new DbSetupCommitment
            {
                //todo: this is just the non-nullable for now
                Reference = "Reference",
                LegalEntityId = "LegalId",
                LegalEntityName = "LegalName",
                LegalEntityAddress = "LegalAddress",
                LegalEntityOrganisationType = OrganisationType.CompaniesHouse,
                EditStatus = EditStatus.EmployerOnly,
                LastUpdatedByEmployerName = "LastUpdatedByEmployerName", // why is this non-null?
                LastUpdatedByEmployerEmail = "LastUpdatedByEmployerEmail@example.com"
            },
            GetEmployerApprenticeshipEmployerId);

            var apprenticeshipId = injector.AddApprenticeship(new DbSetupApprenticeship
            {
                CommitmentId = commitmentId,
                FirstName = "Connie",
                LastName = "Lingus",
                //todo: other nullable fields
                AgreementStatus = AgreementStatus.EmployerAgreed,
                // must not be PendingApproval or Deleted to be returned
                PaymentStatus = PaymentStatus.Active,
                HasHadDataLockSuccess = false
            },
            GetEmployerApprenticeshipApprenticeshipId);

            injector.AddPriceHistory(new DbSetupPriceHistory
            {
                ApprenticeshipId = apprenticeshipId,
                Cost = 1000L,
                FromDate = new DateTime(2018, 1, 1),
                ToDate = null
            });
        }

        [Test]
        public async Task GetEmployerApprenticeship()
        {
            long employerId = TestSetup.TestIds[GetEmployerApprenticeshipEmployerId];
            long apprenticeshipId = TestSetup.TestIds[GetEmployerApprenticeshipApprenticeshipId];
            var url = $"api/employer/{employerId}/apprenticeships/{apprenticeshipId}";

            var stopwatch = Stopwatch.StartNew();
            // block on result, rather than awaiting as it gives a more realistic timing
            var result = IntegrationTestServer.Client.GetAsync(url).Result;
            await TestLog.Progress($"Call to GetEmployerApprenticeship took {stopwatch.Elapsed}");

            Assert.IsTrue(result.IsSuccessStatusCode);

            var resultsAsString = await result.Content.ReadAsStringAsync();
            var apprenticeship = JsonConvert.DeserializeObject<Apprenticeship>(resultsAsString);

            //todo:
            Assert.AreEqual("Connie", apprenticeship.FirstName);
        }
    }
}
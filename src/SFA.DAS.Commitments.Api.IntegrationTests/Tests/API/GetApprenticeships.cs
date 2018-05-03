using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.ApiHost;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using OrganisationType = SFA.DAS.Common.Domain.Types.OrganisationType;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Tests.API
{
    [TestFixture]
    public sealed class GetApprenticeships
    {
        //public static IEnumerable<TestDbSetupEntity> GetTestSpecificData()
        //{
        //    //todo: ask for next id of a type, or *supply to this method* ids that can be used, otherwise int tests will interfere with each other
        //    // or clash with the randomly generated data
        //    // or let generator generate ids, but how do we handle foreign keys?
        //    // we could have multiple calls to ienumerable<testdbsetupentity> returning methods, and later methods
        //    // could ask for the ids earlier calls requested
        //    // could ask for IEnumerable<Func<IEnumerable<TestDbSetupEntity>>>
        //    var testSpecificData = new List<TestDbSetupEntity>
        //    {
        //        {new TestDbSetupEntity { DbSetupEntity = new DbSetupCommitment {}}},

        //    };
        //}

        // by using the convention TestClassName_TestName_IdType, we don't have to centrally store TestId names in TestId as const strings
        private const string GetEmployerApprenticeshipsEmployerId = "GetApprenticeships_GetEmployerApprenticeships_EmployerId";

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
            GetEmployerApprenticeshipsEmployerId);

            injector.AddApprenticeship(new DbSetupApprenticeship
            {
                CommitmentId = commitmentId,
                FirstName = "Anita",
                LastName = "Bush",
                //todo: other nullable fields
                AgreementStatus = AgreementStatus.EmployerAgreed,
                // must not be PendingApproval or Deleted to be returned
                PaymentStatus = PaymentStatus.Active,
                HasHadDataLockSuccess = false
            });
            injector.AddApprenticeship(new DbSetupApprenticeship
            {
                CommitmentId = commitmentId,
                FirstName = "Anna-Leigh",
                LastName = "Probin",
                //todo: other nullable fields
                AgreementStatus = AgreementStatus.EmployerAgreed,
                // must not be PendingApproval or Deleted to be returned
                PaymentStatus = PaymentStatus.Active,
                HasHadDataLockSuccess = false
            });
        }

        //todo: if we make optimisation to GetApprenticeships we need to cover the refactoring with integrationtest(s)
        //      which means we have to set up a specific data set
        //todo: check everything returned (s.*) is actually mapped (and therefore used)
        [Test]
        public async Task GetEmployerApprenticeships()
        {
            long employerId = TestSetup.TestIds[GetEmployerApprenticeshipsEmployerId];
            var url = $"api/employer/{employerId}/apprenticeships";

            var stopwatch = Stopwatch.StartNew();
            // block on result, rather than awaiting as it gives a more realistic timing
            var result = IntegrationTestServer.Client.GetAsync(url).Result;
            //stopwatch.Elapsed;

            Assert.IsTrue(result.IsSuccessStatusCode);

            var resultsAsString = await result.Content.ReadAsStringAsync();
            var apprenticeships = JsonConvert.DeserializeObject<IEnumerable<Apprenticeship>>(resultsAsString);

            //todo:
            //CollectionAssert.AreEquivalent();
            Assert.AreEqual("Anita", apprenticeships.First().FirstName);
            Assert.AreEqual("Anna-Leigh", apprenticeships.Skip(1).First().FirstName);
        }
    }
}

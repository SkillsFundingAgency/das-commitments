using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.ApiHost;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.Validation;
using SFA.DAS.Commitments.Api.Types.Validation.Types;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Tests.API.ValidateOverlappingApprenticeships
{
    [TestFixture]
    public class WhenValidatingOverlappingApprenticeships
    {
        private const string FirstName = "ValidateOverlappingApprenticeships";
        private const string LastName = "ThenApprenticeshipWaitingForSenderApprovalIsIncludedInCheck";

        private const string Uln = "1773673121";

        public static void InjectTestSpecificData(TestDataInjector injector)
        {
            var apprenticeship = TestDbSetupEntities.GetDbSetupApprenticeship(
                injector.AddCommitment(TestDbSetupEntities.GetDbSetupCommitment()), FirstName, LastName);

            apprenticeship.ULN = Uln;
            apprenticeship.AgreementStatus = AgreementStatus.BothAgreed;
            apprenticeship.PaymentStatus = PaymentStatus.PendingApproval;
            apprenticeship.StartDate = new DateTime(2010, 1, 1);
            apprenticeship.EndDate = new DateTime(2011, 1, 1);

            injector.AddApprenticeship(apprenticeship);
        }

        [Test]
        public async Task ThenApprenticeshipWaitingForSenderApprovalIsIncludedInCheck()
        {
            const string url = "api/validation/apprenticeships/overlapping";

            var request = new List<ApprenticeshipOverlapValidationRequest>
            {
                new ApprenticeshipOverlapValidationRequest
                {
                    ApprenticeshipId = 54321L,
                    Uln = Uln,
                    StartDate = new DateTime(2010, 6, 1),
                    EndDate = new DateTime(2011, 6, 1)
                }
            };

            var stopwatch = Stopwatch.StartNew();
            // block on result, rather than awaiting as it gives a more realistic timing
            var result = IntegrationTestServer.Client.PostAsJsonAsync(url, request).Result;
            await TestLog.Progress($"Call to ValidateOverlappingApprenticeships took {stopwatch.Elapsed}");

            Assert.IsTrue(result.IsSuccessStatusCode);

            var resultsAsString = await result.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<IEnumerable<ApprenticeshipOverlapValidationResult>>(resultsAsString);

            Assert.AreEqual(1, results.Count());

            var firstValidationResult = results.First();
            Assert.IsNotNull(firstValidationResult.OverlappingApprenticeships);
            Assert.AreEqual(1, firstValidationResult.OverlappingApprenticeships.Count());

            var firstOverlap = firstValidationResult.OverlappingApprenticeships.First();
            // first check we have the correct apprenticeship
            Assert.AreEqual($"{FirstName} {LastName}", firstOverlap.Apprenticeship.ApprenticeshipName);
            Assert.AreEqual(ValidationFailReason.OverlappingStartDate, firstOverlap.ValidationFailReason);
        }
    }
}
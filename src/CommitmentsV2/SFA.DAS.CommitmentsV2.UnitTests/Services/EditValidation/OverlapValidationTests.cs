using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
using SFA.DAS.CommitmentsV2.Types;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class OverlapValidationTests
    {
        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public async Task When_StartDate_Overlaps(Party party)
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            var tupleResult = await SetupAuthenticationContext(party, fixture, true, false);

            var result = tupleResult.Result;
            var errorText = tupleResult.ErrorText;

            Assert.NotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(errorText, result.Errors[0].ErrorMessage);
            Assert.AreEqual("StartDate", result.Errors[0].PropertyName);
        }

        private static async Task<(EditApprenticeshipValidationResult Result, string ErrorText)> 
            SetupAuthenticationContext(Party party, EditApprenticeshipValidationServiceTestsFixture fixture, bool startDateOverlap, bool endDateOverlap)
        {
            var eaFixture = fixture.SetupMockContextApprenticeship()
                .SetupOverlapService(startDateOverlap, endDateOverlap);
            
            var partyText = party == Party.Employer ? "training provider" : "employer";
            var errorText = $"The date overlaps with existing training dates for the same apprentice. Please check the date - contact your {partyText} for help";

            EditApprenticeshipValidationRequest request;

            if (party == Party.Employer)
            {
                eaFixture.SetupAuthenticationContextAsEmployer();
                request = eaFixture.CreateValidationRequest(employerRef: "abc");
            }
            else
            {
                eaFixture.SetupAuthenticationContextAsProvider();
                request = eaFixture.CreateValidationRequest(providerRef: "abc");
            }

            var result = await fixture.Validate(request);

            return (result, errorText);
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public async Task When_EndDate_Overlaps(Party party)
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            var tupleResult = await SetupAuthenticationContext(party, fixture, false, true);

            var result = tupleResult.Result;
            var errorText = tupleResult.ErrorText;

            Assert.NotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(errorText, result.Errors[0].ErrorMessage);
            Assert.AreEqual("EndDate", result.Errors[0].PropertyName);
        }


        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public async Task When_StarDate_And_EndDate_Overlaps(Party party)
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            var tupleResult = await SetupAuthenticationContext(party, fixture, true, true);

            var result = tupleResult.Result;
            var errorText = tupleResult.ErrorText;

            Assert.NotNull(result.Errors);
            Assert.AreEqual(2, result.Errors.Count);
            var endDateError = result.Errors.First(x => x.PropertyName == "EndDate");
            Assert.AreEqual(errorText, endDateError.ErrorMessage);
            Assert.AreEqual("EndDate", endDateError.PropertyName);

            var startDateError = result.Errors.First(x => x.PropertyName == "StartDate");
            Assert.AreEqual(errorText, startDateError.ErrorMessage);
            Assert.AreEqual("StartDate", startDateError.PropertyName);
        }
    }
}

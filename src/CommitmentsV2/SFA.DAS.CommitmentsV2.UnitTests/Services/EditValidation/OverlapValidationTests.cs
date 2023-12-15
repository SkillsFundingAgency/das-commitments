using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
using SFA.DAS.CommitmentsV2.Types;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class OverlapValidationTests
    {
        const string providerErrorText = "The date overlaps with existing training dates for the same apprentice. Please check the date - contact the employer for help";
        const string employerErrorText = "The date overlaps with existing training dates for the same apprentice. Please check the date - contact the training provider for help";

        [TestCase(Party.Employer, employerErrorText)]
        [TestCase(Party.Provider, providerErrorText)]
        public async Task When_StartDate_Overlaps(Party party, string errorText)
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            var result = await SetupAuthenticationContext(party, fixture, true, false);

            Assert.That(result.Errors, Is.Not.Null);
            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo(errorText));
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo("StartDate"));
        }

    

        [TestCase(Party.Employer, employerErrorText)]
        [TestCase(Party.Provider, providerErrorText)]
        public async Task When_EndDate_Overlaps(Party party, string errorText)
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            var result = await SetupAuthenticationContext(party, fixture, false, true);

            Assert.That(result.Errors, Is.Not.Null);
            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo(errorText));
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo("EndDate"));
        }


        [TestCase(Party.Employer, employerErrorText)]
        [TestCase(Party.Provider, providerErrorText)]
        public async Task When_StarDate_And_EndDate_Overlaps(Party party, string errorText)
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            var result = await SetupAuthenticationContext(party, fixture, true, true);

            Assert.That(result.Errors, Is.Not.Null);
            Assert.That(result.Errors.Count, Is.EqualTo(2));
            var endDateError = result.Errors.First(x => x.PropertyName == "EndDate");
            Assert.That(endDateError.ErrorMessage, Is.EqualTo(errorText));
            Assert.That(endDateError.PropertyName, Is.EqualTo("EndDate"));

            var startDateError = result.Errors.First(x => x.PropertyName == "StartDate");
            Assert.That(startDateError.ErrorMessage, Is.EqualTo(errorText));
            Assert.That(startDateError.PropertyName, Is.EqualTo("StartDate"));
        }

        private static async Task<EditApprenticeshipValidationResult>
        SetupAuthenticationContext(Party party, EditApprenticeshipValidationServiceTestsFixture fixture, bool startDateOverlap, bool endDateOverlap)
        {
            var eaFixture = fixture.SetupMockContextApprenticeship()
                .SetupOverlapService(startDateOverlap, endDateOverlap);

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

            return result;
        }
    }
}

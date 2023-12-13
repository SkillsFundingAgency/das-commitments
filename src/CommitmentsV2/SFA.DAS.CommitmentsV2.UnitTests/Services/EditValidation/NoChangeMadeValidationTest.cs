using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class NoChangeMadeValidationTest
    {
        [Test]
        public async Task When_No_Change_IsMade()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest();

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("No change made: you need to amend details or cancel"));
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo("ApprenticeshipId"));
        }
    }
}

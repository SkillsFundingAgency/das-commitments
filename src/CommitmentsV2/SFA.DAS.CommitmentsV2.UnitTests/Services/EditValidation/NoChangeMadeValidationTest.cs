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
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("No change made", result.Errors[0].ErrorMessage);
            Assert.AreEqual("ApprenticeshipId", result.Errors[0].PropertyName);
        }
    }
}

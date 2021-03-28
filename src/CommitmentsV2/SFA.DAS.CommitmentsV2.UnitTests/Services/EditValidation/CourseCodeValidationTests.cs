using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class CourseCodeValidationTests
    {
        [Test]
        public async Task CourseCode_Is_Mandatory()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest();
            request.CourseCode = string.Empty;

            var result = await fixture.Validate(request);

            Assert.NotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Invalid training code", result.Errors[0].ErrorMessage);
            Assert.AreEqual("CourseCode", result.Errors[0].PropertyName);
        }
    }
}

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
            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("Invalid training code"));
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo("CourseCode"));
        }
    }
}

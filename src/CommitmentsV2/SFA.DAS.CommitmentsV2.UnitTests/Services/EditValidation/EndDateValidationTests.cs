using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class EndDateValidationTests
    {
        [Test]
        public async Task EndDate_Should_Not_Be_Empty()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest();
            request.EndDate = null;

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The end date is not valid"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("EndDate"));
            });
        }

        [Test]
        public async Task EndDate_Should_Not_Be_Before_DasStartDate()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest(endYear: 2017, endMonth: 4);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Is.Not.Null);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The end date must not be earlier than May 2017"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("EndDate"));
            });
        }

        [Test]
        public async Task EndDate_Should_Not_Be_Before_Start()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest(endYear: fixture.StartDate.Value.Year, endMonth: fixture.StartDate.Value.Month -1);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Is.Not.Null);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The end date must not be on or before the start date"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("EndDate"));
            });
        }

        [Test]
        public async Task EndDate_Should_Allow_Same_Month_For_Ilr_ApprenticeshipUnit()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            fixture.Apprenticeship.LearnerDataId = 123;
            fixture.SetupCourseLearningType(fixture.Apprenticeship.CourseCode, LearningType.ApprenticeshipUnit);

            var request = fixture.CreateValidationRequest(
                firstName: "Updated",
                startYear: fixture.StartDate.Value.Year,
                startMonth: fixture.StartDate.Value.Month,
                endYear: fixture.StartDate.Value.Year,
                endMonth: fixture.StartDate.Value.Month);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors.Any(x =>
                    x.PropertyName == "EndDate" &&
                    x.ErrorMessage == "The end date must not be on or before the start date"),
                Is.False);
        }
    }
}

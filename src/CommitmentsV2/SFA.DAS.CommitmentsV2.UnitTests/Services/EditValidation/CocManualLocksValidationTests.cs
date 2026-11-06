namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class CocManualLocksValidationTests
    {
        [Test]
        public async Task When_FirstName_Updated_Should_Error_Depending_On_ProviderId()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship(providerId: 200).SetupCocManualService(200, "FirstName", true);

            var request = fixture.CreateValidationRequest(firstName: "Test");

            var result = await fixture.Validate(request);
            result.Errors.Should().ContainSingle(x => x.PropertyName == "FirstName" && x.ErrorMessage == "First name can only be updated via the ILR");
        }

        [Test]
        public async Task When_LastName_Updated_Should_Error_Depending_On_ProviderId()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship(providerId: 200).SetupCocManualService(200, "LastName", true);

            var request = fixture.CreateValidationRequest(lastName: "Test");

            var result = await fixture.Validate(request);
            result.Errors.Should().ContainSingle(x => x.PropertyName == "LastName" && x.ErrorMessage == "Last name can only be updated via the ILR");
        }

        [Test]
        public async Task When_DateOfBirth_Updated_Should_Error_Depending_On_ProviderId()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship(providerId: 200).SetupCocManualService(200, "DateOfBirth", true);

            var request = fixture.CreateValidationRequest(dobYear: 2000, dobMonth: 2,  dobDay: 1);

            var result = await fixture.Validate(request);
            result.Errors.Should().ContainSingle(x => x.PropertyName == "DateOfBirth" && x.ErrorMessage == "Date of birth can only be updated via the ILR");
        }

        [Test]
        public async Task When_StartDate_Updated_Should_Error_Depending_On_ProviderId()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture().CourseIsEffectiveFromDate(new DateTime(2021, 1, 1));
            fixture.SetupMockContextApprenticeship(providerId: 200).SetupCocManualService(200, "StartDate", true);

            var request = fixture.CreateValidationRequest(startYear: 2021, startMonth: 2, endYear: 2022, endMonth: 3);

            var result = await fixture.Validate(request);
            result.Errors.Should().ContainSingle(x => x.PropertyName == "StartDate" && x.ErrorMessage == "Start date can only be updated via the ILR");
        }

        [Test]
        public async Task When_EndDate_Updated_Should_Error_Depending_On_ProviderId()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship(providerId: 200).SetupCocManualService(200, "EndDate", true);

            var request = fixture.CreateValidationRequest(endYear: 2026, endMonth: 2);

            var result = await fixture.Validate(request);
            result.Errors.Should().ContainSingle(x => x.PropertyName == "EndDate" && x.ErrorMessage == "End date can only be updated via the ILR");
        }
        [Test]
        public async Task When_Cost_Updated_Should_Error_Depending_On_ProviderId()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship(providerId: 200).SetupCocManualService(200, "Cost", true);

            var request = fixture.CreateValidationRequest(cost: 1000);

            var result = await fixture.Validate(request);
            result.Errors.Should().ContainSingle(x => x.PropertyName == "Cost" && x.ErrorMessage == "Total price can only be updated via the ILR");
        }
    }
}

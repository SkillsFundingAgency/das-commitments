namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class EmailValidationTests
    {
        [Test]
        public async Task When_Email_Does_Not_Exist_On_Apprenticeship_Then_No_Change_Can_Be_Requested()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest(email: "a@a.com");

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("Email update cannot be requested"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("Email"));
            });
        }

        [Test]
        public async Task When_Email_Does_Not_Exist_On_Apprenticeship_Then_A_Change_Can_Be_Requested_When_Cohort_Approved_After_2021_09_09()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship(employerProviderApprovedOn:new DateTime(2021,09,10));
            var request = fixture.CreateValidationRequest(email: "a@a.com");

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Is.Empty);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("     ")]
        public async Task When_Email_Does_Exist_On_Apprenticeship_Then_Email_Cannot_Be_Null_Or_Empty(string email)
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship(email: "a@a.com");
            var request = fixture.CreateValidationRequest(email: email);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("Email address cannot be blank"));
        }

        [TestCase("@")]
        [TestCase("paul@@a.com")]
        [TestCase("p@aul@a.com")]
        [TestCase("asa@a")]
        [TestCase("\\asa@a.com")]
        public async Task When_Email_Does_Exist_On_Apprenticeship_Then_New_Email_Cannot_Be_Invalid(string email)
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship(email: "a@a.com");
            var request = fixture.CreateValidationRequest(email: email);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("Please enter a valid email address"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("Email"));
            });
        }

        [TestCase("paul@a.com")]
        [TestCase("asa@a.ie.uk")]
        [TestCase("asa@a.tv")]
        public async Task When_Email_Does_Exist_On_Apprenticeship_Then_New_Email_Must_Be_Valid(string email)
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship(email: "a@a.com");
            var request = fixture.CreateValidationRequest(email: "b@b.com");

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Is.Empty);
        }

        [TestCase("emailalready@exists.com")]
        public async Task When_Valid_Email_Exists_On_Apprenticeship_And_Changes_Email_Then_New_Email_Must_Still_Be_Unique(string email)
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship(email: "a@a.com").SetupOverlapCheckServiceToReturnEmailOverlap(email);
            var request = fixture.CreateValidationRequest(email: email);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("This email address is in use on another apprentice record. You need to enter a different email address."));
        }

        [TestCase(null)]
        public async Task When_Valid_Email_Is_Empty_On_Apprenticeship_And_Request_Email_Overlap_check_Should_Not_Be_Called(string email)
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            var request = fixture.SetupGetTrainingProgrammeQueryResult().SetupMockContextApprenticeship(email: email).CreateValidationRequest(email: email, startMonth:2, startYear:2023, endMonth:1, endYear:2027);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Is.Empty);
            fixture.VerifyCheckForEmailOverlapsIsNotCalled();
        }
    }
}
namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class DateOfBirthValidationTests
    {
        [Test]
        public async Task DateOfBirth_Should_Be_Greater_Than_Min_DateOfBirth()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var request = fixture.CreateValidationRequest(dobYear: 1899, dobMonth : 12, dobDay: 31);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The Date of birth is not valid"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("DateOfBirth"));
            });
        }

        [Test]
        public async Task DateOfBirth_Should_Be_At_least_15_At_start_of_training()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
           
            var request = fixture.CreateValidationRequest(dobYear: 2006, dobMonth: 12, dobDay: 31, minimumAgeAtApprenticeshipStart: 15);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The apprentice must be at least 15 years old at the start of their training"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("DateOfBirth"));
            });
        }

        [Test]
        public async Task DateOfBirth_Must_Be_Younger_Than_115_At_start_of_training()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
           
            var request = fixture.CreateValidationRequest(dobYear: 1904, dobMonth: 12, dobDay: 31, maximumAgeAtApprenticeshipStart: 115);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The apprentice must be younger than 115 years old at the start of their training"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("DateOfBirth"));
            });
        }

        [Test]
        public async Task DateOfBirth_Is_Required()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
          
            var request = fixture.CreateValidationRequest();
            request.DateOfBirth = null;

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The Date of birth is not valid"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("DateOfBirth"));
            });
        }

        [Test]
        public async Task DateOfBirth_Should_Be_At_least_16_At_start_of_training_When_Minimum_Age_Is_16()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
           
            var request = fixture.CreateValidationRequest(dobYear: 2007, dobMonth: 12, dobDay: 31, minimumAgeAtApprenticeshipStart: 16);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The apprentice must be at least 16 years old at the start of their training"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("DateOfBirth"));
            });
        }

        [Test]
        public async Task DateOfBirth_Should_Be_At_least_18_At_start_of_training_When_Minimum_Age_Is_18()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
           
            var request = fixture.CreateValidationRequest(dobYear: 2005, dobMonth: 12, dobDay: 31, minimumAgeAtApprenticeshipStart: 18);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The apprentice must be at least 18 years old at the start of their training"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("DateOfBirth"));
            });
        }

        [Test]
        public async Task DateOfBirth_Must_Be_Younger_Than_25_At_start_of_training_When_Maximum_Age_Is_25()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
           
            // Create request with different DOB than the apprenticeship (1995-01-01)
            // Use a much older DOB to ensure they are above 25 at start date
            var request = fixture.CreateValidationRequest(dobYear: 1990, dobMonth: 1, dobDay: 1, maximumAgeAtApprenticeshipStart: 25);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The apprentice must be younger than 25 years old at the start of their training"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("DateOfBirth"));
            });
        }

        [Test]
        public async Task DateOfBirth_Must_Be_Younger_Than_30_At_start_of_training_When_Maximum_Age_Is_30()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
           
            // Create request with different DOB than the apprenticeship (1995-01-01)
            // Use a much older DOB to ensure they are above 30 at start date
            var request = fixture.CreateValidationRequest(dobYear: 1985, dobMonth: 1, dobDay: 1, maximumAgeAtApprenticeshipStart: 30);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The apprentice must be younger than 30 years old at the start of their training"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("DateOfBirth"));
            });
        }

        [Test]
        public async Task DateOfBirth_Should_Pass_Validation_When_Age_Is_Within_Dynamic_Range()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
           
            // Apprentice will be 20 years old at start date, which is within 18-25 range
            // Only change DOB, not start date to avoid other validations
            var request = fixture.CreateValidationRequest(
                dobYear: 2000, 
                dobMonth: 1, 
                dobDay: 1,
                minimumAgeAtApprenticeshipStart: 18,
                maximumAgeAtApprenticeshipStart: 25);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(0));
        }

        [Test]
        public async Task DateOfBirth_Should_Fail_Validation_When_Age_Is_Below_Dynamic_Minimum()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
           
            // Apprentice will be 17 years old at start date (2024), which is below minimum 18
            // Only change DOB, not start date to avoid other validations
            var request = fixture.CreateValidationRequest(
                dobYear: 2007, 
                dobMonth: 6, 
                dobDay: 15,
                minimumAgeAtApprenticeshipStart: 18,
                maximumAgeAtApprenticeshipStart: 25);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The apprentice must be at least 18 years old at the start of their training"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("DateOfBirth"));
            });
        }

        [Test]
        public async Task DateOfBirth_Should_Fail_Validation_When_Age_Is_Above_Dynamic_Maximum()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
           
            // Apprentice will be much older than 25 at start date, which is above maximum 25
            // Only change DOB, not start date to avoid other validations
            var request = fixture.CreateValidationRequest(
                dobYear: 1990, 
                dobMonth: 1, 
                dobDay: 1,
                minimumAgeAtApprenticeshipStart: 18,
                maximumAgeAtApprenticeshipStart: 25);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The apprentice must be younger than 25 years old at the start of their training"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("DateOfBirth"));
            });
        }
    }
}

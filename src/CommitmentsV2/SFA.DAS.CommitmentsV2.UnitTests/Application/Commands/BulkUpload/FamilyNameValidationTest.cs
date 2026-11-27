namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    [TestFixture]
    [Parallelizable]
    public class FamilyNameValidationTest
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task Validate_IsNotEmpty(string lastName)
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetFamilyName(lastName);
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "FamilyName", "<b>Last name</b> must be entered");
        }

        [Test]
        public async Task Validate_Is_Less_Than_100_Characters()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetFamilyName("12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890");
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "FamilyName", "Enter a <b>last name</b> that is not longer than 100 characters");
        }

        [TestCase("blzm6<script>alert(1)</script>pghgc")]
        [TestCase("<b>Smith</b>")]
        public async Task Validate_Rejects_Markup(string lastName)
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetFamilyName(lastName);

            var errors = await fixture.Handle();

            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "FamilyName",
                "Enter a <b>last name</b> that only contains valid characters");
        }

        [TestCase("Anne-Marie")]
        [TestCase("O'Neill")]
        [TestCase("O’Connor")]
        [TestCase("José")]
        [TestCase("Mary Jane")]
        [TestCase("李")]
        public async Task Validate_Accepts_Names_With_Allowed_Characters(string lastName)
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetFamilyName(lastName);

            var errors = await fixture.Handle();

            errors.BulkUploadValidationErrors.SelectMany(error => error.Errors)
                .Should().NotContain(error => error.Property == "FamilyName");
        }

        [Test]
        public async Task Validate_Accepts_100_Characters()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetFamilyName(new string('a', 100));

            var errors = await fixture.Handle();

            errors.BulkUploadValidationErrors.SelectMany(error => error.Errors)
                .Should().NotContain(error => error.Property == "FamilyName");
        }
    }
}

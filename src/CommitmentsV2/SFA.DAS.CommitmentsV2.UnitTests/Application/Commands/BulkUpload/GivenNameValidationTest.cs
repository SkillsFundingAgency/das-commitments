namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    [TestFixture]
    [Parallelizable]
    public class GivenNameValidationTest
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task Validate_IsNotEmpty(string firstName)
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetGivenNames(firstName);
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "GivenName", "<b>First name</b> must be entered");
        }

        [Test]
        public async Task Validate_Is_Less_Than_100_Characters()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetGivenNames("12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890");
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "GivenName", "Enter a <b>first name</b> that is not longer than 100 characters");
        }

        [TestCase("wuqar<script>alert(1)</script>l853a")]
        [TestCase("<b>Mark</b>")]
        public async Task Validate_Rejects_Markup(string firstName)
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetGivenNames(firstName);

            var errors = await fixture.Handle();

            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "GivenName",
                "Enter a <b>first name</b> that only contains valid characters");
        }

        [TestCase("Anne-Marie")]
        [TestCase("O'Neill")]
        [TestCase("O’Connor")]
        [TestCase("José")]
        [TestCase("Mary Jane")]
        [TestCase("李")]
        public async Task Validate_Accepts_Names_With_Allowed_Characters(string firstName)
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetGivenNames(firstName);

            var errors = await fixture.Handle();

            errors.BulkUploadValidationErrors.SelectMany(error => error.Errors)
                .Should().NotContain(error => error.Property == "GivenName");
        }

        [Test]
        public async Task Validate_Accepts_100_Characters()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetGivenNames(new string('a', 100));

            var errors = await fixture.Handle();

            errors.BulkUploadValidationErrors.SelectMany(error => error.Errors)
                .Should().NotContain(error => error.Property == "GivenName");
        }
    }
}

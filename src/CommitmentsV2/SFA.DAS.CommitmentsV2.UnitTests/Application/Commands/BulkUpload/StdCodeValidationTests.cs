using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    [TestFixture]
    [Parallelizable]
    public class StdCodeValidationTests
    {
        [Test]
        public async Task Validate_IsNotEmpty()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStdCode("");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "CourseCode", "<b>Standard code</b> must be entered");
        }

        [Test]
        public async Task Validate_Is_Number_Only()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStdCode("59ab");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "CourseCode", "Enter a valid <b>standard code</b>");
        }

        [Test]
        public async Task Validate_Is_Less_Tan_5_Characters()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStdCode("595961");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "CourseCode", "Enter a valid <b>standard code</b>");
        }

        [Test]
        public async Task Validate_Is_Not_A_Valid_StdCode()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStdCode("5959");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "CourseCode", "Enter a valid <b>standard code</b>");
        }

        [Test]
        public async Task Validate_Is_Not_A_Valid_Provider_StdCode()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStdCode("59");
            fixture.SetMainProvider(true);
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "CourseCode", "Enter a valid <b>standard code.</b> You have not told us that you deliver this training course. You must assign the course to your account in the <a href= class='govuk - link'>Your standards and training venues</a> section.");
        }

        [Test]
        public async Task Validate_No_Standards_Declared()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStdCode("59");
            fixture.SetMainProvider(true);
            fixture.SetStandardsEmpty();
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "DeclaredStandards", "No Standards Declared");
        }

        [Test]
        public async Task Validate__MainProvider_False_No_Standards_Declared()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStdCode("59");
            fixture.SetMainProvider(false);
            fixture.SetStandardsEmpty();
            var errors = await fixture.Handle();
            Assert.AreEqual(0, errors.BulkUploadValidationErrors.Count);
        }
    }
}

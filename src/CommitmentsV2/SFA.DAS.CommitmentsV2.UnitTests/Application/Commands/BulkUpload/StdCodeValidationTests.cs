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
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStdCode("");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "StdCode", "<b>Standard code</b> must be entered");
        }

        [Test]
        public async Task Validate_Is_Number_Only()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStdCode("59ab");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "StdCode", "Enter a valid <b>standard code</b>");
        }

        [Test]
        public async Task Validate_Is_Less_Tan_5_Characters()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStdCode("595961");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "StdCode", "Enter a valid <b>standard code</b>");
        }

        [Test]
        public async Task Validate_Is_Not_A_Valid_StdCode()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStdCode("5959");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "StdCode", "Enter a valid <b>standard code</b>");
        }
    }
}

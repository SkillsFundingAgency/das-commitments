using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    [TestFixture]
    [Parallelizable]
    public class FamilyNameValidationTest
    {
        [Test]
        public async Task Validate_IsNotEmpty()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetFamilyName("");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "FamilyName", "<b>Last name</b> must be entered");
        }

        [Test]
        public async Task Validate_Is_Less_Than_100_Characters()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetFamilyName("12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "FamilyName", "Enter a <b>last name</b> that is not longer than 100 characters");
        }
    }
}

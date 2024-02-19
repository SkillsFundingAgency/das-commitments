using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    [TestFixture]
    [Parallelizable]
    public class DateOfBirthValidationTests
    {
        [Test]
        public async Task Validate_IsNotEmpty()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetDateOfBirth("");
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "DateOfBirth", "Enter the apprentice's <b>date of birth</b> using the format yyyy-mm-dd, for example 2001-04-23");
        }

        [TestCase("01-01-2000")]
        [TestCase("20000-01-02")]
        public async Task Validate_Is_Of_Valid_Pattern(string dateOfBirthPattern)
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetDateOfBirth(dateOfBirthPattern);
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "DateOfBirth", "Enter the apprentice's <b>date of birth</b> using the format yyyy-mm-dd, for example 2001-04-23");
        }

        [TestCase("1999-11-31")]
        [TestCase("2000-02-30")]
        public async Task Validate_Is_Valid_Date(string dateOfBirthPattern)
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetDateOfBirth(dateOfBirthPattern);
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "DateOfBirth", "Enter the apprentice's <b>date of birth</b> using the format yyyy-mm-dd, for example 2001-04-23");
        }

        [Test]
        public async Task Validate_Apprentice_AtLeast_15_On_Start_Of_Course()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetDateOfBirth("2009-05-01");
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "DateOfBirth", "The apprentice's <b>date of birth</b> must show that they are at least 15 years old at the start of their training");
        }

        [Test]
        public async Task Validate_Apprentice_Age_Less_Than_115_On_Start_Of_Course()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetDateOfBirth("1904-05-01");
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "DateOfBirth", "The apprentice's <b>date of birth</b> must show that they are not older than 115 years old at the start of their training");
        }
    }
}

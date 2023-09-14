using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    [TestFixture]
    [Parallelizable]
    public class EndDateValidationTests
    {
        [Test]
        public async Task Validate_IsNotEmpty()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetEndDate("");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "EndDate", "Enter the <b>end date</b> using the format yyyy-mm, for example 2019-02");
        }

        [TestCase("01-2000")]
        [TestCase("01-19999")]
        public async Task Validate_Is_Valid_Format(string startDate)
        {
            using  var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetEndDate(startDate);
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "EndDate", "Enter the <b>end date</b> using the format yyyy-mm, for example 2019-02");
        }

        [TestCase("1999-13")]
        [TestCase("2000-14")]
        public async Task Validate_Is_Valid_Date(string startDate)
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetEndDate(startDate);
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "EndDate", "Enter the <b>end date</b> using the format yyyy-mm, for example 2019-02");
        }

        [Test]
        public async Task Validate_Is_After_StartDate()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetEndDate("2018-01");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "EndDate", "Enter an <b>end date</b> that is after the start date");
        }
    }
}

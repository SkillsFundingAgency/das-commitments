using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    [TestFixture]
    [Parallelizable]
    public class CostValidationTests
    {
        [Test]
        public async Task Validate_IsNotEmpty()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetTotalPrice("");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "TotalPrice", "Enter the <b>total cost</b> of training in whole pounds using numbers only");
        }

        [Test]
        public async Task Validate_Is_Whole_Pounds()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetTotalPrice("19.23");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "TotalPrice", "Enter the <b>total cost</b> of training in whole pounds using numbers only");
        }

        [Test]
        public async Task Validate_Is_Price_Is_Not_Zero()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetTotalPrice("0");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "TotalPrice", "The <b>total cost</b> must be more than £0");
        }

        [Test]
        public async Task Validate_Is_Price_Is_Not_More_Tan_100000()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetTotalPrice("100001");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "TotalPrice", "The <b>total cost</b> must be £100,000 or less");
        }
    }
}

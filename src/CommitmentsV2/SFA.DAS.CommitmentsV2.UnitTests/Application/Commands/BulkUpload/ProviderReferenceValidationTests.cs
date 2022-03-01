using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    [TestFixture]
    [Parallelizable]
    public class ProviderReferenceValidationTests
    {
        [Test]
        public async Task Validate_Is_Not_More_Than_20_Characters()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetProviderRef("012345678901234567890");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "ProviderRef", "The <b>Provider Ref</b> must not be longer than 20 characters");
        }
    }
}

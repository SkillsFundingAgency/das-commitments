﻿namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    public class EPAOrgIdTests
    {
        [Test]
        public async Task Validate_Is_Not_Greater_Than_7_Characters()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetEpaOrgId("12345678");
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "EPAOrgId", "The <b>EPAO ID</b> must not be longer than 7 characters");
        }
    }
}

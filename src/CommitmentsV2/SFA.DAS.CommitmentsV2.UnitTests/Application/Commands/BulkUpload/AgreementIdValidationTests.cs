﻿using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    [TestFixture]
    [Parallelizable]
    public class AgreementIdValidationTests
    {
        [Test]
        public async Task Validate_IsNotEmpty()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetAgreementId("");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "AgreementId", "<b>Agreement ID</b> must be entered");
        }

        [Test]
        public async Task Validate_IsAllLetterOrDigit()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetAgreementId("ABC*12");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "AgreementId", "Enter a valid <b>Agreement ID</b>");
        }

        [Test]
        public async Task Validate_IsLessThan_6_Characters()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetAgreementId("ABC1234");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "AgreementId", "Enter a valid <b>Agreement ID</b>");
        }

        [Test]
        public async Task Validate_IsAValidEmployer()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetAgreementId("ABC123");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "AgreementId", "Enter a valid <b>Agreement ID</b>");
        }
    }
}
﻿using NUnit.Framework;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    [TestFixture]
    [Parallelizable]
    public class UlnValidationTests
    {
        [Test]
        public async Task Validate_IsNotEmpty()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetUln("");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "ULN", "Enter a 10-digit <b>unique learner number</b>");
        }

        [Test]
        public async Task Validate_IsNot_9999999999()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetUln("9999999999");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "ULN", "The <b>unique learner number</b> of 9999999999 isn't valid");
        }

        [Test]
        public async Task Validate_Is_LessThan_10()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetUln("12345678901");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "ULN", "Enter a 10-digit <b>unique learner number</b>");
        }

        [Test]
        public async Task Validate_Is_A_Valid_ULN_Pattern()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetUln("0112233669");
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "ULN", "Enter a 10-digit <b>unique learner number</b>");
        }

        [Test]
        public async Task Validate_Is_When_Overlapping_StartDate()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetOverlappingDate(true, false);
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "ULN", "The <b>start date</b> overlaps with existing training dates for the same apprentice");
        }

        [Test]
        public async Task Validate_Is_When_Overlapping_EndDate()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetOverlappingDate(false, true);
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "ULN", "The <b>end date</b> overlaps with existing training dates for the same apprentice");
        }

        [Test]
        public async Task Validate_When_Duplicate_ULN()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetUpDuplicateUln();
            var errors = await fixture.Handle();
            fixture.ValidateError(errors, 1, "ULN", "The <b>unique learner number</b> has already been used for an apprentice in this file");
        }

        [Test]
        public async Task Validate_Is_When_Overlapping_Start_And_EndDate()
        {
            var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetOverlappingDate(true, true);
            var errors = await fixture.Handle();

            Assert.AreEqual(1, errors.BulkUploadValidationErrors.Count);
            Assert.AreEqual(2, errors.BulkUploadValidationErrors[0].Errors.Count);
            Assert.AreEqual("The <b>start date</b> overlaps with existing training dates for the same apprentice", errors.BulkUploadValidationErrors[0].Errors[0].ErrorText);
            Assert.AreEqual("The <b>end date</b> overlaps with existing training dates for the same apprentice", errors.BulkUploadValidationErrors[0].Errors[1].ErrorText);
            Assert.AreEqual("ULN", errors.BulkUploadValidationErrors[0].Errors[0].Property);
            Assert.AreEqual("ULN", errors.BulkUploadValidationErrors[0].Errors[1].Property);
        }
    }
}
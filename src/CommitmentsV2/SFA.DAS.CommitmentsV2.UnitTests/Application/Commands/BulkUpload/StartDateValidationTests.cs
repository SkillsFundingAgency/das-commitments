﻿namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    [TestFixture]
    [Parallelizable]
    public class StartDateValidationTests
    {
        [Test]
        public async Task Validate_IsNotEmpty()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("");
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "StartDate", "Enter the <b>start date</b> using the format yyyy-mm-dd, for example 2017-09-01");
        }

        [TestCase("01-01-2000")]
        [TestCase("20000-01-02")]
        public async Task Validate_Is_Valid_Format(string startDate)
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate(startDate);
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "StartDate", "Enter the <b>start date</b> using the format yyyy-mm-dd, for example 2017-09-01");
        }

        [TestCase("1999-11-31")]
        [TestCase("2000-02-30")]
        public async Task Validate_Is_Valid_Date(string startDate)
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate(startDate);
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "StartDate", "You must enter the <b>start date</b>, for example 2017-09-01");
        }

        [Test]
        public async Task Validate_Is_After_May_2017()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2017-04-01");
            fixture.SetPriorLearning(null);
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "StartDate", "The <b>start date</b> must not be earlier than May 2017");
        }

        [Test]
        public async Task Validate_Is_After_May_2018_Transfer_Funded()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2018-04-01").SetIsTransferFunded();
            fixture.SetPriorLearning(null);
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "StartDate", "The <b>start date</b> for apprenticeships funded through a transfer must not be earlier than May 2018");
        }

        [Test]
        public async Task Validate_Is_No_Later_Than_One_Year_After_AcademicYearEndDate()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetAfterAcademicYearEndDate();
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "StartDate", "The <b>start date</b> must be no later than one year after the end of the current teaching year");
        }

        [Test]
        public async Task Validate_Apprenticeship_Starts_After_Course_EffectiveFrom()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetCourseEffectiveFromAfterCourseStartDate();
            var errors = await fixture.Handle();
            var standard = await fixture.GetStandard();
            var prevMonth = standard.EffectiveFrom.Value.AddMonths(-1);
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "StartDate", $"This training course is only available to apprentices with a <b>start date</b> after {prevMonth.Month}  {prevMonth.Year}");
        }

        [Test]
        public async Task Validate_Apprenticeship_Ends_Before_Course_EffectiveTo()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetCourseEffectiveToBeforeCourseStartDate();
            var errors = await fixture.Handle();
            var standard = await fixture.GetStandard();
            var nextMonth = standard.EffectiveTo.Value.AddMonths(1);
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "StartDate", $"This training course is only available to apprentices with a <b>start date</b> before {nextMonth.Month}  {nextMonth.Year}");
        }
    }
}

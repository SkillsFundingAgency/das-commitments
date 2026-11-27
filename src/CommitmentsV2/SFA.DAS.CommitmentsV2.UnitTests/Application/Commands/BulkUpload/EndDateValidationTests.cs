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
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "EndDate", "Enter the <b>end date</b> using the format yyyy-mm, for example 2019-02");
        }

        [TestCase("01-2000")]
        [TestCase("01-19999")]
        public async Task Validate_Is_Valid_Format(string startDate)
        {
            using  var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetEndDate(startDate);
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "EndDate", "Enter the <b>end date</b> using the format yyyy-mm, for example 2019-02");
        }

        [TestCase("1999-13")]
        [TestCase("2000-14")]
        public async Task Validate_Is_Valid_Date(string startDate)
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetEndDate(startDate);
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "EndDate", "Enter the <b>end date</b> using the format yyyy-mm, for example 2019-02");
        }

        [Test]
        public async Task Validate_Is_After_StartDate()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetEndDate("2018-01");
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "EndDate", "Enter an <b>end date</b> that is after the start date");
        }

        [Test]
        public async Task Validate_Is_Not_The_Same_As_StartDate()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2023-01-01");
            fixture.SetEndDate("2023-01");
            var errors = await fixture.Handle();
            BulkUploadValidateCommandHandlerTestsFixture.ValidateError(errors, 1, "EndDate", "Enter an <b>end date</b> that is after the start date");
        }

        [TestCase("2026-09", false)]
        [TestCase("2026-10", true)]
        [TestCase("2026-11", true)]
        [TestCase("2027-01", true)]
        [TestCase("2025-12", false)]
        public void Validate_AppUnit_EndDateByMonth(string endDate, bool isValid)
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2026-10-26");
            fixture.SetEndDate(endDate);

            var errors = fixture.ValidationService.ValidateEndDate(
                fixture.CsvRecords[0],
                SFA.DAS.CommitmentsV2.Types.LearningType.ApprenticeshipUnit);

            if (isValid)
            {
                errors.Should().BeEmpty();
            }
            else
            {
                errors.Should().ContainSingle()
                    .Which.ErrorText.Should().Be("Enter an <b>end date</b> that is after the start date");
            }
        }

        [Test]
        public void Validate_Standard_EndDateInSameMonth()
        {
            using var fixture = new BulkUploadValidateCommandHandlerTestsFixture();
            fixture.SetStartDate("2026-10-26");
            fixture.SetEndDate("2026-10");

            var errors = fixture.ValidationService.ValidateEndDate(fixture.CsvRecords[0]);

            errors.Should().ContainSingle()
                .Which.ErrorText.Should().Be("Enter an <b>end date</b> that is after the start date");
        }
    }
}

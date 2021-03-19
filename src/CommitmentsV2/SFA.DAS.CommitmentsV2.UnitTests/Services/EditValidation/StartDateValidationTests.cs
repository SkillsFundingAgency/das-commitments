using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Shared.Extensions;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class StartDateValidationTests
    {
        [Test]
        public async Task StartDate_Is_Required()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship();
            // The default course start date for these test is 1st Jan 2020 
            var request = fixture.CreateValidationRequest(startYear: null);

            var result = await fixture.Validate(request);

            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "The start date is not valid");
            Assert.AreEqual(result.Errors[0].PropertyName, "StartDate");
        }

        [Test]
        public async Task StartDate_Must_be_no_later_than_one_year_after_the_end_of_the_current_teaching_year()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship().SetupMockAcademicYearDateProvider(new DateTime(2019, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            // The default course start date for these test is 1st Jan 2020 
            var request = fixture.CreateValidationRequest(startYear: 2021, startMonth: 9, endYear: 2022);

            var result = await fixture.Validate(request);

            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "The start date must be no later than one year after the end of the current teaching year");
            Assert.AreEqual(result.Errors[0].PropertyName, "StartDate");
        }

        [Test]
        public async Task StartDate_is_before_current_academic_year_startdate_And_DateTimeNow_()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            var datTime = new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc);

            fixture.SetupMockContextApprenitceship(startYear : 2022, endYear: 2029)
                .SetupMockAcademicYearDateProvider(datTime)
                .SetupCurrentDateTime(new DateTime(2021, 3,18));

            var request = fixture.CreateValidationRequest(startYear: 2017, startMonth: 6);

            var result = await fixture.Validate(request);

            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, $"The earliest start date you can use is {datTime.ToGdsFormatShortMonthWithoutDay()}");
            Assert.AreEqual(result.Errors[0].PropertyName, "StartDate");
        }

        //[Test]
        //public async Task StartDate_is_before_May_2018_For_TransferSender()
        //{
        //    var fixture = new EditApprenitceshipValidationServiceTestsFixture();
        //    var datTime = new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc);

        //    fixture.SetupMockContextApprenitceship(startYear: 2022, endYear: 2029)
        //        .SetupMockAcademicYearDateProvider(datTime)
        //        .SetupCurrentDateTime(new DateTime(2021, 3, 18));

        //    var request = fixture.CreateValidationRequest(startYear: 2018, startMonth: 5);

        //    var result = await fixture.Validate(request);

        //    Assert.AreEqual(result.Errors.Count, 1);
        //    Assert.AreEqual(result.Errors[0].ErrorMessage, $"Apprentices funded through a transfer can't start earlier than May 2018");
        //    Assert.AreEqual(result.Errors[0].PropertyName, "StartDate");
        //}

        [Test]
        public async Task StartDate_is_before_DasStartDate_and_Course_Started_before_Das()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            var datTime = new DateTime(2021, 8, 1, 0, 0, 0, DateTimeKind.Utc);

            fixture.SetupMockContextApprenitceship()
                .SetupMockAcademicYearDateProvider(datTime)
                .SetupCurrentDateTime(new DateTime(2021, 3, 18))
                .SetUpMediatorForTrainingCourse(new DateTime(2016,1,1));

            var request = fixture.CreateValidationRequest(startYear: 2017, startMonth: 4);

            var result = await fixture.Validate(request);

            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, $"The start date must not be earlier than May 2017");
            Assert.AreEqual(result.Errors[0].PropertyName, "StartDate");
        }

        [Test]
        public async Task Course_Is_Pending_On_StartDate()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            var datTime = new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc);

            fixture.SetupMockContextApprenitceship()
                .SetupMockAcademicYearDateProvider(datTime)
                .SetupCurrentDateTime(new DateTime(2020, 10, 1))
                .SetUpMediatorForTrainingCourse(new DateTime(2021, 1, 1), 1);

            var request = fixture.CreateValidationRequest(startYear: 2020, startMonth: 7);

            var result = await fixture.Validate(request);

            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, $"This training course is only available to apprentices with a start date after {new DateTime(2021, 1, 1).AddMonths(-1):MM yyyy}");
            Assert.AreEqual(result.Errors[0].PropertyName, "StartDate");
        }

        [Test]
        public async Task Course_Is_Expired_On_StartDate()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            var datTime = new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc);

            fixture.SetupMockContextApprenitceship()
                .SetupMockAcademicYearDateProvider(datTime)
                .SetupCurrentDateTime(new DateTime(2020, 10, 1))
                .SetUpMediatorForTrainingCourse(new DateTime(2018, 1, 1), 1);

            var request = fixture.CreateValidationRequest(startYear: 2020, startMonth: 7);

            var result = await fixture.Validate(request);

            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, $"This training course is only available to apprentices with a start date before {new DateTime(2019, 1, 1).AddMonths(1):MM yyyy}");
            Assert.AreEqual(result.Errors[0].PropertyName, "StartDate");
        }
    }
}

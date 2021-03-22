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
          
            var request = fixture.CreateValidationRequest();
            request.StartDate = null;

            var result = await fixture.Validate(request);

            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "The start date is not valid");
            Assert.AreEqual(result.Errors[0].PropertyName, "StartDate");
        }

        [Test]
        public async Task StartDate_Must_be_no_later_than_one_year_after_the_end_of_the_current_teaching_year()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenitceship();
            var endOfCurrentTeachingYear = fixture.GetEndOfCurrentTeachingYear();

            var request = fixture.CreateValidationRequest(startYear: endOfCurrentTeachingYear.AddYears(1).Year, startMonth: endOfCurrentTeachingYear.Month +1 , endYear: endOfCurrentTeachingYear.Year + 2, endMonth: endOfCurrentTeachingYear.Month);

            var result = await fixture.Validate(request);

            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, "The start date must be no later than one year after the end of the current teaching year");
            Assert.AreEqual(result.Errors[0].PropertyName, "StartDate");
        }

        [Test]
        public async Task StartDate_Is_Not_With_In_FundingPeriod()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();

            fixture.SetupMockContextApprenitceship().SetUpLastAcademicYearFundingPeriodToBeBeforeDateTimeNow();
            DateTime currentAcademiceYearStartDate = fixture.GetCurrentAcademicYearStartDate();
                

            var request = fixture.CreateValidationRequest(startYear: currentAcademiceYearStartDate.Year, startMonth: currentAcademiceYearStartDate.Month -1);

            var result = await fixture.Validate(request);

            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, $"The earliest start date you can use is {fixture.GetCurrentAcademicYearStartDate().ToGdsFormatShortMonthWithoutDay()}");
            Assert.AreEqual(result.Errors[0].PropertyName, "StartDate");
        }

        [Test]
        public async Task StartDate_is_before_DasStartDate_and_Course_Started_before_Das()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();

            fixture.SetupMockContextApprenitceship().CourseIsEffectiveFromDate(new DateTime(2016,1,1));
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

            fixture.SetupMockContextApprenitceship().CourseIsEffectiveFromDate(new DateTime(2020, 7, 1));

            var request = fixture.CreateValidationRequest(startYear: 2020, startMonth: 6);

            var result = await fixture.Validate(request);

            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, $"This training course is only available to apprentices with a start date after {new DateTime(2020, 7, 1).AddMonths(-1):MM yyyy}");
            Assert.AreEqual(result.Errors[0].PropertyName, "StartDate");
        }

        [Test]
        public async Task Course_Is_Expired_On_StartDate()
        {
            var fixture = new EditApprenitceshipValidationServiceTestsFixture();

            fixture.SetupMockContextApprenitceship()
               .CourseIsEffectiveFromDate(new DateTime(2017, 7, 1), 1);

           DateTime  startDate = fixture.GetStartDate();

            var request = fixture.CreateValidationRequest(startYear: startDate.Year, startMonth: startDate.Month + 1);

            var result = await fixture.Validate(request);

            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Errors[0].ErrorMessage, $"This training course is only available to apprentices with a start date before {new DateTime(2018, 7, 1).AddMonths(1):MM yyyy}");
            Assert.AreEqual(result.Errors[0].PropertyName, "StartDate");
        }
    }
}

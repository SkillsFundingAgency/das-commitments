﻿using SFA.DAS.CommitmentsV2.Shared.Extensions;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.EditValidation
{
    public class StartDateValidationTests
    {
        [Test]
        public async Task StartDate_Is_Required()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();

            var request = fixture.CreateValidationRequest();
            request.StartDate = null;

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The start date is not valid"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("StartDate"));
            });
        }

        [Test]
        public async Task StartDate_Must_be_no_later_than_one_year_after_the_end_of_the_current_teaching_year()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();
            fixture.SetupMockContextApprenticeship();
            var endOfCurrentTeachingYear = fixture.GetEndOfCurrentTeachingYear();

            var request = fixture.CreateValidationRequest(startYear: endOfCurrentTeachingYear.AddYears(1).Year, startMonth: endOfCurrentTeachingYear.Month + 1, endYear: endOfCurrentTeachingYear.Year + 2, endMonth: endOfCurrentTeachingYear.Month);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("The start date must be no later than one year after the end of the current teaching year"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("StartDate"));
            });
        }

        [Test]
        public async Task StartDate_Is_Not_With_In_FundingPeriod()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();

            fixture.SetupMockContextApprenticeship().SetUpLastAcademicYearFundingPeriodToBeBeforeDateTimeNow();
            DateTime currentAcademiceYearStartDate = fixture.GetCurrentAcademicYearStartDate();


            var request = fixture.CreateValidationRequest(startYear: currentAcademiceYearStartDate.Year, startMonth: currentAcademiceYearStartDate.Month - 1);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo($"The earliest start date you can use is {fixture.GetCurrentAcademicYearStartDate().ToGdsFormatShortMonthWithoutDay()}"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("StartDate"));
            });
        }

        [Test]
        public async Task StartDate_is_before_DasStartDate_and_Course_Started_before_Das()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();

            fixture.SetupMockContextApprenticeship().CourseIsEffectiveFromDate(new DateTime(2016, 1, 1));
            var request = fixture.CreateValidationRequest(startYear: 2017, startMonth: 4);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo($"The start date must not be earlier than May 2017"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("StartDate"));
            });
        }

        [Test]
        public async Task ActualStartDate_is_before_SimplifiedPaymentsStartDate()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();

            fixture
                .SetupMockContextApprenticeship(isOnFlexiPaymentsPilot: true)
                .CourseIsEffectiveFromDate(new DateTime(2020, 1, 1));
            var request = fixture.CreateValidationRequest(actualStartDate: new DateTime(2022, 1, 1));

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo($"The start date must not be earlier than 1 November 2024."));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("ActualStartDate"));
            });
        }

        [Test]
        public async Task Course_Is_Pending_On_StartDate()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();

            fixture.SetupMockContextApprenticeship().CourseIsEffectiveFromDate(new DateTime(2020, 7, 1));

            var request = fixture.CreateValidationRequest(startYear: 2020, startMonth: 6);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo($"This training course is only available to apprentices with a start date after {new DateTime(2020, 7, 1).AddMonths(-1):MM yyyy}"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("StartDate"));
            });
        }

        [Test]
        public async Task Course_Is_Expired_On_StartDate()
        {
            var fixture = new EditApprenticeshipValidationServiceTestsFixture();

            fixture.SetupMockContextApprenticeship()
               .CourseIsEffectiveFromDate(new DateTime(2017, 7, 1), 1);

            DateTime startDate = fixture.GetStartDate();

            var request = fixture.CreateValidationRequest(startYear: startDate.Year, startMonth: startDate.Month + 1);

            var result = await fixture.Validate(request);

            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo($"This training course is only available to apprentices with a start date before {new DateTime(2018, 7, 1).AddMonths(1):MM yyyy}"));
                Assert.That(result.Errors[0].PropertyName, Is.EqualTo("StartDate"));
            });
        }
    }
}

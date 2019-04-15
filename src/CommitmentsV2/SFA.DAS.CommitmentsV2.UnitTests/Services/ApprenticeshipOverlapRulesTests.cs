using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Types;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class ApprenticeshipOverlapRulesTests
    {
        [TestCase("May-19 to Jul-19", "Jan-19 to Feb-19", OverlapStatus.None)]
        [TestCase("May-19 to Jul-19", "Jan-19 to May-19", OverlapStatus.None)]
        [TestCase("May-19 to Jul-19", "Jan-19 to Jun-19", OverlapStatus.OverlappingEndDate)]
        [TestCase("May-19 to Jul-19", "May-19 to Jul-19", OverlapStatus.DateEmbrace)]
        [TestCase("May-19 to Jul-19", "Jun-19 to Aug-19", OverlapStatus.OverlappingStartDate)]
        [TestCase("May-19 to Jul-19", "Jul-19 to Aug-19", OverlapStatus.None)]
        [TestCase("May-19 to Aug-19", "Jun-19 to Jul-19", OverlapStatus.DateWithin)]

        public void SingleApprenticeship_WithExplicitStartAndEndDates_ShouldDetermineOverlapCorrectly(
            string apprenticeshipStartEnd, 
            string testStartEnd, 
            OverlapStatus expectedOverlapStatus)
        {
            var apprenticeshipDates = apprenticeshipStartEnd.StartEndPeriod();

            var fixtures = new ApprenticeshipOverlapRulesTestFixtures()
                                .WithConfirmedApprenticeship("123", apprenticeshipDates.StartDate, apprenticeshipDates.EndDate);

            var testDates = testStartEnd.StartEndPeriod();
            fixtures.AssertApprenticeOverlapStatus(testDates.StartDate, testDates.EndDate, expectedOverlapStatus);
        }

        [TestCase("120, 120, 120", "May-19 to Jul-19, Jan-19 to Mar-19", "Feb-19 to Mar-19", OverlapStatus.OverlappingStartDate)]
        [TestCase("121, 121, 121", "Jan-19 to Mar-19, May-19 to Jul-19", "Apr-19 to Jun-19", OverlapStatus.OverlappingEndDate)]
        [TestCase("122, 122, 122", "Jan-19 to Mar-19, May-19 to Jul-19", "Feb-19 to Jun-19", OverlapStatus.OverlappingDates)]
        [TestCase("123, 123, 123", "Feb-19 to Mar-19, May-19 to Jul-19", "Jan-19 to Jun-19", OverlapStatus.DateEmbrace | OverlapStatus.OverlappingEndDate)]
        [TestCase("124, 124, 124", "Feb-19 to Mar-19, May-19 to Jul-19", "Jan-19 to Apr-19", OverlapStatus.DateEmbrace)]
        [TestCase("124", "", "Jan-19 to Apr-19", OverlapStatus.None)]
        public void MultipleApprenticeship_WithExplicitStartAndEndDates_ShouldDetermineOverlapCorrectly(
            // first uln is for the test, the rest are for the existing apprenticeships
            string ulns,
            string apprenticeshipsStartEnd,
            string testStartEnd,
            OverlapStatus expectedOverlapStatus)
        {
            // arrange
            var fixtures = new ApprenticeshipOverlapRulesTestFixtures();
            var ulnStack = new Stack<string>(ulns.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()));

            foreach (var apprenticeshipDates in apprenticeshipsStartEnd.StartEndPeriods())
            {
                fixtures.WithConfirmedApprenticeship(ulnStack.Pop(), apprenticeshipDates.StartDate, apprenticeshipDates.EndDate);
            }

            var testDates = testStartEnd.StartEndPeriod();

            // act
            var actualOverlapStatus = fixtures.DetermineOverlapStatus(ulnStack.Pop(), testDates.StartDate, testDates.EndDate);

            // assert
            Assert.AreEqual(expectedOverlapStatus, actualOverlapStatus);
        }

        [TestCase("May-19 to Jul-19, Jan-19 to Mar-19", "Feb-19 to Mar-19")]
        [TestCase("Jan-19 to Mar-19, May-19 to Jul-19", "Apr-19 to Jun-19")]
        [TestCase("Jan-19 to Mar-19, May-19 to Jul-19", "Feb-19 to Jun-19")]
        [TestCase("Feb-19 to Mar-19, May-19 to Jul-19", "Jan-19 to Jun-19")]
        [TestCase("Feb-19 to Mar-19, May-19 to Jul-19", "Jan-19 to Apr-19")]
       public void MultipleApprenticeship_WithDifferenceUlns_ShouldNotDetectAsOverlaps(
            string apprenticeshipsStartEnd,
            string testStartEnd)
        {
            // arrange
            var fixtures = new ApprenticeshipOverlapRulesTestFixtures();

            foreach (var apprenticeshipDates in apprenticeshipsStartEnd.StartEndPeriods())
            {
                fixtures.WithConfirmedApprenticeship("999", apprenticeshipDates.StartDate, apprenticeshipDates.EndDate);
            }

            var testDates = testStartEnd.StartEndPeriod();

            // act
            var actualOverlapStatus = fixtures.DetermineOverlapStatus("123", testDates.StartDate, testDates.EndDate);

            // assert
            Assert.AreEqual(OverlapStatus.None, actualOverlapStatus);
       }

       [Test]
       public void MultipleApprenticeship_WithCancelled_ShouldNotIncludeCancelledApprenticeshipsAfterStoppedDate()
       {
           const string uln = "123";

           // arrange
           var fixtures = new ApprenticeshipOverlapRulesTestFixtures()
               .WithConfirmedApprenticeship(uln, new DateTime(2019, 01, 01), new DateTime(2019, 12, 31) )
               .ThatHasCancelled(new DateTime(2019, 07, 01));

           // act
           var actualOverlapStatus = fixtures.DetermineOverlapStatus(uln, new DateTime(2019, 08, 01), new DateTime(2017, 09, 01) );

           // assert
           Assert.AreEqual(OverlapStatus.None, actualOverlapStatus);
       }

       [Test]
       public void MultipleApprenticeship_WithCancelled_ShouldIncludeCancelledApprenticeshipsBeforeStoppedDate()
       {
           const string uln = "123";

            // arrange
            var fixtures = new ApprenticeshipOverlapRulesTestFixtures()
               .WithConfirmedApprenticeship(uln, new DateTime(2019, 01, 01), new DateTime(2019, 12, 31))
               .ThatHasCancelled(new DateTime(2019, 07, 01));

           // act
           var actualOverlapStatus = fixtures.DetermineOverlapStatus(uln, new DateTime(2019, 05, 01), new DateTime(2017, 06, 01));

           // assert
           Assert.AreEqual(OverlapStatus.OverlappingStartDate, actualOverlapStatus);
       }

        [TestCase("120, 120, 120", "May-19 to Jul-19, Jan-19 to Mar-19", "Feb-19 to Mar-19", true)]
        [TestCase("121, 121, 121", "Jan-19 to Mar-19, May-19 to Jul-19", "Apr-19 to Jun-19", false)]
        [TestCase("122, 122, 122", "Jan-19 to Mar-19, May-19 to Jul-19", "Feb-19 to Jun-19", true)]
        [TestCase("123, 123, 123", "Feb-19 to Mar-19, May-19 to Jul-19", "Jan-19 to Jun-19", true)]
        [TestCase("124, 124, 124", "Feb-19 to Mar-19, May-19 to Jul-19", "Jan-19 to Apr-19", true)]
        public void MultipleApprenticeship_WithExplicitStartAndEndDates_ShouldIndicateProblemWithStartDateCorrectly(
            // first uln is for the test, the rest are for the existing apprenticeships
            string ulns,
            string apprenticeshipsStartEnd,
            string testStartEnd,
            bool  expectProblemWithStartDate)
        {
            // arrange
            var fixtures = new ApprenticeshipOverlapRulesTestFixtures();
            var ulnStack = new Stack<string>(ulns.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()));

            foreach (var apprenticeshipDates in apprenticeshipsStartEnd.StartEndPeriods())
            {
                fixtures.WithConfirmedApprenticeship(ulnStack.Pop(), apprenticeshipDates.StartDate, apprenticeshipDates.EndDate);
            }

            var testDates = testStartEnd.StartEndPeriod();

            // act
            var actualOverlapStatus = fixtures.DetermineOverlapStatus(ulnStack.Pop(), testDates.StartDate, testDates.EndDate);

            // assert
            fixtures.AssertStartDateIssueIsDetected(actualOverlapStatus, expectProblemWithStartDate);
        }

        [TestCase("120, 120, 120", "May-19 to Jul-19, Jan-19 to Mar-19", "Feb-19 to Mar-19", false)]
        [TestCase("121, 121, 121", "Jan-19 to Mar-19, May-19 to Jul-19", "Apr-19 to Jun-19", true)]
        [TestCase("122, 122, 122", "Jan-19 to Mar-19, May-19 to Jul-19", "Feb-19 to Jun-19", true)]
        [TestCase("123, 123, 123", "Feb-19 to Mar-19, May-19 to Jul-19", "Jan-19 to Jun-19", true)]
        [TestCase("124, 124, 124", "Feb-19 to Mar-19, May-19 to Jul-19", "Jan-19 to Apr-19", true)]
        public void MultipleApprenticeship_WithExplicitStartAndEndDates_ShouldIndicateProblemWithEndDateCorrectly(
            // first uln is for the test, the rest are for the existing apprenticeships
            string ulns,
            string apprenticeshipsStartEnd,
            string testStartEnd,
            bool expectProblemWithEndDate)
        {

            // arrange
            var fixtures = new ApprenticeshipOverlapRulesTestFixtures();
            var ulnStack = new Stack<string>(ulns.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()));

            foreach (var apprenticeshipDates in apprenticeshipsStartEnd.StartEndPeriods())
            {
                fixtures.WithConfirmedApprenticeship(ulnStack.Pop(), apprenticeshipDates.StartDate, apprenticeshipDates.EndDate);
            }

            var testDates = testStartEnd.StartEndPeriod();

            // act
            var actualOverlapStatus = fixtures.DetermineOverlapStatus(ulnStack.Pop(), testDates.StartDate, testDates.EndDate);

            // assert
            fixtures.AssertEndDateIssueIsDetected(actualOverlapStatus, expectProblemWithEndDate);
        }
    }

    public class ApprenticeshipOverlapRulesTestFixtures
    {
        private int _apprenticeshipId = 0;

        public ApprenticeshipOverlapRulesTestFixtures()
        {
            Apprenticeships = new List<Apprenticeship>();
        }

        public List<Apprenticeship> Apprenticeships { get; }

        public ApprenticeshipOverlapRulesTestFixtures WithDraftApprenticeship(string uln, DateTime? startDate, DateTime? endDate)
        {
            return AddApprenticeship(uln, startDate, endDate, PaymentStatus.PendingApproval);
        }

        public ApprenticeshipOverlapRulesTestFixtures WithConfirmedApprenticeship(string uln, DateTime startDate, DateTime endDate)
        {
            return AddApprenticeship(uln, startDate, endDate, PaymentStatus.Active);
        }

        public ApprenticeshipOverlapRulesTestFixtures ThatHasCancelled(DateTime stopDate)
        {
            var apprenticeship = Apprenticeships.Last();

            apprenticeship.StopDate = stopDate;
            apprenticeship.PaymentStatus = PaymentStatus.Withdrawn;

            return this;
        }

        /// <summary>
        ///     Verify that the added apprentice (only one apprentice should have been added) overlaps with the specified
        ///     date period in the expected way. 
        /// </summary>
        public void AssertApprenticeOverlapStatus(DateTime startDate, DateTime endDate, OverlapStatus expectedOverlapStatus)
        {
            var apprenticeship = Apprenticeships.Single();

            var actualOverlapStatus = apprenticeship.DetermineOverlap(startDate, endDate);

            Assert.AreEqual(expectedOverlapStatus, actualOverlapStatus);
        }

        /// <summary>
        ///     Verify that the start date has been correctly determined to have a problem. 
        /// </summary>
        public void AssertStartDateIssueIsDetected(OverlapStatus actualOverlapStatus, bool expectStartDateIssue)
        {
            AssertSpecifiedDateIssueIsDetected(actualOverlapStatus, OverlapStatus.ProblemWithStartDate, expectStartDateIssue);
        }

        /// <summary>
        ///     Verify that the end date has been correctly determined to have a problem. 
        /// </summary>
        public void AssertEndDateIssueIsDetected(OverlapStatus actualOverlapStatus, bool expectEndDateIssue)
        {
            AssertSpecifiedDateIssueIsDetected(actualOverlapStatus, OverlapStatus.ProblemWithEndDate, expectEndDateIssue);
        }

        public OverlapStatus DetermineOverlapStatus(string uln, DateTime startDate, DateTime endDate)
        {
            var apprenticeshipWeWantToAdd = CreateApprenticeship(uln, startDate, endDate, PaymentStatus.Active);

            var actualOverlapStatus = Apprenticeships.DetermineOverlap(apprenticeshipWeWantToAdd);

            return actualOverlapStatus;
        }

        private void AssertSpecifiedDateIssueIsDetected(OverlapStatus actualOverlapStatus, OverlapStatus dateIssueBeingTested, bool expectedDateIssue)
        {
            var hasDateIssueBeingTested = (actualOverlapStatus & dateIssueBeingTested) != OverlapStatus.None;

            if (expectedDateIssue)
            {
                Assert.IsTrue(hasDateIssueBeingTested, $"Actual detected overlap:{actualOverlapStatus}");
            }
            else
            {
                Assert.IsFalse(hasDateIssueBeingTested, $"Actual detected overlap:{actualOverlapStatus}");
            }
        }

        private ApprenticeshipOverlapRulesTestFixtures AddApprenticeship(string uln, DateTime? startDate, DateTime? endDate, PaymentStatus paymentStatus)
        {
            var apprenticeship = CreateApprenticeship(uln, startDate, endDate, paymentStatus);
            Apprenticeships.Add(apprenticeship);
            return this;
        }

        private Apprenticeship CreateApprenticeship(string uln, DateTime? startDate, DateTime? endDate, PaymentStatus paymentStatus)
        {
            return new DraftApprenticeship
            {
                Id = Interlocked.Increment(ref _apprenticeshipId),
                Uln = uln,
                StartDate = startDate,
                EndDate = endDate,
                PaymentStatus = paymentStatus
            };
        }
    }
}

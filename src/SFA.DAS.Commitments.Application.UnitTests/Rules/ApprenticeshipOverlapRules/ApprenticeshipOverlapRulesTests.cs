using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.Validation;

namespace SFA.DAS.Commitments.Application.UnitTests.Rules.ApprenticeshipOverlapRules
{
    internal static class StringExtensions
    {
        private static readonly List<string> Months = new List<string>
            {"JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"};

        /// <summary>
        ///     Convert a string in the format Apr-19 or Apr-2019, Jun-20 or Jun-2020 etc
        ///     in to a date of the 1st of the month.
        /// </summary>
        public static DateTime MonthYear(this string s)
        {
            var parts = s.Split('-').ToArray();
            if (parts.Length != 2)
            {
                throw new InvalidOperationException($"The string {s} should have two parts separated by a dash: mmm-yy or mmm-yyyy");
            }

            if (parts[0].Length < 3)
            {
                throw new InvalidOperationException($"The string {s} should have a month part with at least three letters: mmm-yy or mmm-yyyy");
            }

            if (parts[1].Length != 2 && parts[1].Length != 4)
            {
                throw new InvalidOperationException($"The string {s} should have a year that is either 2 or 4 digits: mmm-yy or mmm-yyyy");
            }

            var month = parts[0].Substring(0,3).ToUpperInvariant();

            var monthIdx = Months.IndexOf(month);

            if (monthIdx < 0)
            {
                throw new InvalidOperationException($"The month part in {s} ({month} is not a recognised month");
            }

            if (!int.TryParse(parts[1], out int year))
            {
                throw new InvalidOperationException($"The year part in {s} ({parts[1]} is not a valid year - should be yy or yyyy");
            }

            return new DateTime(year, monthIdx+1, 1);
        }
    }

    [TestFixture]
    public class ApprenticeshipOverlapRulesTests
    {
        [TestCase("May-19", "Jul-19", "Jan-19", "Feb-19", ValidationFailReason.None)]
        [TestCase("May-19", "Jul-19", "Jan-19", "May-19", ValidationFailReason.None)]
        [TestCase("May-19", "Jul-19", "Jan-19", "Jun-19", ValidationFailReason.OverlappingEndDate)]
        [TestCase("May-19", "Jul-19", "May-19", "Jul-19", ValidationFailReason.DateEmbrace)]
        [TestCase("May-19", "Jul-19", "Jun-19", "Aug-19", ValidationFailReason.OverlappingStartDate)]
        [TestCase("May-19", "Jul-19", "Jul-19", "Aug-19", ValidationFailReason.None)]
        [TestCase("May-19", "Aug-19", "Jun-19", "Jul-19", ValidationFailReason.DateWithin)]
        public void SingleApprenticeship_WithExplicitStartAndEndDates_ShouldDetermineOverlapCorrectly(string apprenticeshipStart, string apprenticeshipEnd, string testStart, string testEnd, ValidationFailReason expectedOverlapStatus)
        {
            var fixtures = new ApprenticeshipOverlapRulesTestFixtures()
                                .WithConfirmedApprenticeship("123", apprenticeshipStart.MonthYear(), apprenticeshipEnd.MonthYear());

            fixtures.AssertApprenticeOverlapStatus(testStart.MonthYear(), testEnd.MonthYear(), expectedOverlapStatus);
        }
    }

    public class ApprenticeshipOverlapRulesTestFixtures
    {
        private int _apprenticeshipId = 0;

        public ApprenticeshipOverlapRulesTestFixtures()
        {
            Apprenticeships = new List<ApprenticeshipResult>();    
        }

        public List<ApprenticeshipResult> Apprenticeships { get; }

        public ApprenticeshipOverlapRulesTestFixtures WithDraftApprenticeship(string uln, DateTime startDate, DateTime endDate)
        {
            return AddApprenticeship(uln, startDate, endDate, PaymentStatus.PendingApproval);
        }

        public ApprenticeshipOverlapRulesTestFixtures WithConfirmedApprenticeship(string uln, DateTime startDate, DateTime endDate)
        {
            return AddApprenticeship(uln, startDate, endDate, PaymentStatus.Active);
        }

        public ApprenticeshipOverlapRulesTestFixtures AndCancelApprenticeship(string uln, DateTime stopDate,
            PaymentStatus paymentStatus)
        {
            var apprenticeship = Apprenticeships.Last();

            apprenticeship.StopDate = stopDate;
            apprenticeship.PaymentStatus = PaymentStatus.Withdrawn;

            return this;
        }

        public void AssertApprenticeOverlapStatus(DateTime startDate, DateTime endDate, ValidationFailReason expectedOverlapStatus)
        {
            var rules = new Application.Rules.ApprenticeshipOverlapRules();
            var apprenticeship = Apprenticeships.First();

            var request = new ApprenticeshipOverlapValidationRequest
            {
                Uln = apprenticeship.Uln,
                StartDate = startDate,
                EndDate = endDate
            };

            var actualOverlapStatus = rules.DetermineOverlap(request, apprenticeship);

            Assert.AreEqual(expectedOverlapStatus, actualOverlapStatus);
        }

        private ApprenticeshipOverlapRulesTestFixtures AddApprenticeship(string uln, DateTime startDate, DateTime endDate, PaymentStatus paymentStatus)
        {
            var apprenticeship = CreateApprenticeship(uln, startDate, endDate, paymentStatus);
            Apprenticeships.Add(apprenticeship);
            return this;
        }

        private ApprenticeshipResult CreateApprenticeship(string uln, DateTime startDate, DateTime endDate, PaymentStatus paymentStatus)
        {
            return new ApprenticeshipResult
            {
                Id = Interlocked.Increment(ref _apprenticeshipId),
                Uln = uln,
                StartDate = startDate,
                EndDate =  endDate,
                PaymentStatus = paymentStatus
            };
        }
    }
}

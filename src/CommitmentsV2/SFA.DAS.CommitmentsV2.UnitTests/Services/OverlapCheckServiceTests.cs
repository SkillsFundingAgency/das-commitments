using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class OverlapCheckServiceTests
    {
        private OverlapCheckServiceTestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new OverlapCheckServiceTestFixture();
        }

        [TestCase("2018-04-01", "2018-06-30", Description = "Start and end date both disregarded")]
        [TestCase("2018-04-01", "2018-05-15", Description = "Start date disregarded")]
        [TestCase("2018-05-15", "2018-06-01", Description = "End date disregarded")]
        public async Task ThenTheOverlapCheckDisregardsDatesWithinTheSameMonth(DateTime startDate, DateTime endDate)
        {
            var result = await _fixture
                .WithDateRange(startDate, endDate)
                .CheckForOverlaps();

            Assert.IsFalse(result.HasOverlaps);
        }

        [TestCase("2017-01-01", "2017-12-31", Description = "Before any apprenticeships")]
        [TestCase("2021-01-01", "2021-12-31", Description = "After any apprenticeships")]
        [TestCase("2018-03-01", "2018-03-01", Description = "Zero-length candidate")]
        [TestCase("2018-02-01", "2018-02-14", Description = "Same month but not overlapping (before)")]
        [TestCase("2018-04-16", "2018-04-30", Description = "Same month but not overlapping (after)")]
        public async Task ThenIfDatesDoNotFallWithinRangeOfExistingApprenticeshipThenNotOverlapping(DateTime startDate,
            DateTime endDate)
        {
            var result = await _fixture.WithDateRange(startDate, endDate)
                .CheckForOverlaps();

            Assert.IsFalse(result.HasOverlaps);
        }

        [Test]
        public async Task ThenIfNoUlnsMatchInputThenNotOverlapping()
        {
            var result = await _fixture
                .WithNoMatchingUlnUtilisations()
                .WithDateRange(new DateTime(2018, 01, 1), new DateTime(2018, 12, 31))
                .CheckForOverlaps();

            Assert.IsFalse(result.HasOverlaps);
        }

        [Test]
        public async Task ThenIfUtilisationIsEffectivelyDeletedThenNotOverlapping()
        {
            var result = await _fixture
                .WithUlnUtilisationEffectivelyDeleted()
                .WithDateRange(new DateTime(2018, 01, 1), new DateTime(2019, 12, 31))
                .CheckForOverlaps();
            Assert.IsFalse(result.HasOverlaps);
        }


        [Test]
        public async Task ThenIfStartDateFallsWithinRangeOfExistingApprenticeshipThenIsOverlapping()
        {
            var result = await _fixture
                .WithDateRange(new DateTime(2018, 03, 15), new DateTime(2018, 05, 15))
                .CheckForOverlaps();

            Assert.IsTrue(result.HasOverlappingStartDate);
        }

        [Test]
        public async Task ThenIfEndDateFallsWithinRangeOfExistingApprenticeshipThenIsOverlapping()
        {
            var result = await _fixture
                .WithDateRange(new DateTime(2018, 05, 15), new DateTime(2018, 07, 15))
                .CheckForOverlaps();

            Assert.IsTrue(result.HasOverlappingEndDate);
        }

        [TestCase("2018-03-01", "2018-03-31", Description = "Dates contained within existing range - single month")]
        [TestCase("2018-03-15", "2020-09-15", Description =
            "Start date contained within existing range but later end date")]
        [TestCase("2018-01-15", "2020-03-15", Description =
            "End date contained within existing range but earlier start date")]
        [TestCase("2018-02-15", "2018-04-15", Description = "Same dates as existing range")]
        [TestCase("2018-02-15", "2018-05-15", Description = "Same start date but later end date")]
        [TestCase("2018-01-15", "2018-04-15", Description = "Same end date but earlier start date")]
        [TestCase("2018-03-01", "2018-03-15", Description = "Start/end within same month within existing range")]
        public async Task ThenIfBothDatesFallWithinRangeOfSingleExistingApprenticeshipThenIsOverlapping(
            DateTime startDate, DateTime endDate)
        {
            var result = await _fixture
                .WithDateRange(startDate, endDate)
                .CheckForOverlaps();

            Assert.IsTrue(result.HasOverlappingStartDate);
            Assert.IsTrue(result.HasOverlappingEndDate);
        }

        [Test]
        public async Task ThenIfDatesFallWithinRangeOfDifferentExistingApprenticeshipThenOverlapsBoth()
        {
            var result = await _fixture
                .WithDateRange(new DateTime(2018, 03, 15), new DateTime(2018, 07, 15))
                .CheckForOverlaps();

            Assert.IsTrue(result.HasOverlappingStartDate);
            Assert.IsTrue(result.HasOverlappingEndDate);
        }

        [Test]
        public async Task ThenIfDatesStraddleExistingApprenticeshipThenIsOverlapping()
        {
            var result = await _fixture
                .WithDateRange(new DateTime(2018, 01, 15), new DateTime(2018, 05, 15))
                .CheckForOverlaps();

            Assert.IsTrue(result.HasOverlaps);
        }

        [Test]
        public async Task ThenAnExistingApprenticeshipShouldNotBeConsideredAsOverlappingWithItself()
        {
            var result = await _fixture
                .WithDateRange(new DateTime(2018, 02, 15), new DateTime(2018, 04, 15))
                .WithExistingApprenticeship()
                .CheckForOverlaps();

            Assert.IsFalse(result.HasOverlaps);
        }

        private class OverlapCheckServiceTestFixture
        {
            private readonly OverlapCheckService _overlapCheckService;
            private readonly Mock<IUlnUtilisationService> _ulnUtilisationService;
            private DateTime _startDate;
            private DateTime _endDate;
            private long? _apprenticeshipId;

            public OverlapCheckServiceTestFixture()
            {
                _ulnUtilisationService = new Mock<IUlnUtilisationService>();
                _ulnUtilisationService
                    .Setup(x => x.GetUlnUtilisations(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(CreateTestData());

                _overlapCheckService = new OverlapCheckService(_ulnUtilisationService.Object);
            }

            public OverlapCheckServiceTestFixture WithDateRange(DateTime startDate, DateTime endDate)
            {
                _startDate = startDate;
                _endDate = endDate;
                return this;
            }

            public OverlapCheckServiceTestFixture WithExistingApprenticeship()
            {
                _apprenticeshipId = 1;
                return this;
            }

            public OverlapCheckServiceTestFixture WithNoMatchingUlnUtilisations()
            {
                _ulnUtilisationService
                    .Setup(x => x.GetUlnUtilisations(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new UlnUtilisation[0]);

                return this;
            }

            public OverlapCheckServiceTestFixture WithUlnUtilisationEffectivelyDeleted()
            {
                var mockData = new List<UlnUtilisation>
                {
                    new UlnUtilisation(1, "", new DateTime(2018, 03, 01), new DateTime(2018, 03, 01))
                };

                _ulnUtilisationService
                    .Setup(x => x.GetUlnUtilisations(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(mockData.ToArray);

                return this;
            }

            public async Task<OverlapCheckResult> CheckForOverlaps()
            {
                return await _overlapCheckService.CheckForOverlaps("", _startDate.To(_endDate), _apprenticeshipId,
                    new CancellationToken());
            }

            private static UlnUtilisation[] CreateTestData()
            {
                var mockData = new List<UlnUtilisation>
                {
                    new UlnUtilisation(1, "", new DateTime(2018, 02, 15), new DateTime(2018, 04, 15)),
                    new UlnUtilisation(2, "", new DateTime(2018, 06, 15), new DateTime(2018, 08, 15)),
                    new UlnUtilisation(3, "", new DateTime(2020, 01, 15), new DateTime(2020, 12, 15))
                };

                return mockData.ToArray();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class OverlapCheckServiceTests
    {
        private OverlapCheckService _overlapCheckService;
        private Mock<IUlnUtilisationService> _ulnUtilisationService;

        [SetUp]
        public void Arrange()
        {
            _ulnUtilisationService = new Mock<IUlnUtilisationService>();
            _ulnUtilisationService.Setup(x => x.GetUlnUtilisations(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateTestData());

            _overlapCheckService = new OverlapCheckService(_ulnUtilisationService.Object);
        }

        [TestCase("2018-04-01", "2018-06-30", Description = "Start and end date both disregarded")]
        [TestCase("2018-04-01", "2018-05-15", Description = "Start date disregarded")]
        [TestCase("2018-05-15", "2018-06-01", Description = "End date disregarded")]

        public async Task ThenTheOverlapCheckDisregardsDatesWithinTheSameMonth(DateTime startDate, DateTime endDate)
        {
            //Act
            var result = await _overlapCheckService.CheckForOverlaps("1234567890", startDate, endDate, null, new CancellationToken());

            //Assert
            Assert.IsFalse(result.HasOverlaps);
        }

        [TestCase("2017-01-01", "2017-12-31", Description = "Before any apprenticeships")]
        [TestCase("2021-01-01", "2021-12-31", Description = "After any apprenticeships")]
        public async Task ThenIfDatesDoNotFallWithinRangeOfExistingApprenticeshipThenNotOverlapping(DateTime startDate, DateTime endDate)
        {
            //Act
            var result = await _overlapCheckService.CheckForOverlaps("1234567890", startDate, endDate, null, new CancellationToken());

            //Assert
            Assert.IsFalse(result.HasOverlaps);
        }

        [Test]
        public async Task ThenIfNoUlnsMatchInputThenNotOverlapping()
        {
            //Arrange
            _ulnUtilisationService.Setup(x => x.GetUlnUtilisations(It.Is<string>(u => u == "9999999999"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UlnUtilisation[0]);

            //Act
            var result = await _overlapCheckService.CheckForOverlaps("9999999999", new DateTime(2018, 01, 1), new DateTime(2018, 12, 31), null, new CancellationToken());

            //Assert
            Assert.IsFalse(result.HasOverlaps);
        }

        [Test]
        public async Task ThenIfStartDateFallsWithinRangeOfExistingApprenticeshipThenIsOverlapping()
        {
            //Act
            var result = await _overlapCheckService.CheckForOverlaps("1234567890", new DateTime(2018, 03, 15), new DateTime(2018, 05, 15), null, new CancellationToken());

            //Assert
            Assert.IsTrue(result.OverlappingStartDate);
        }

        [Test]
        public async Task ThenIfEndDateFallsWithinRangeOfExistingApprenticeshipThenIsOverlapping()
        {
            //Act
            var result = await _overlapCheckService.CheckForOverlaps("1234567890", new DateTime(2018, 05, 15), new DateTime(2018, 07, 15), null, new CancellationToken());

            //Assert
            Assert.IsTrue(result.OverlappingEndDate);
        }

        [TestCase("2018-03-01", "2018-03-31", Description = "Dates contained within existing range - single month")]
        [TestCase("2020-03-15", "2020-09-15", Description = "Dates contained within existing range - longer duration")]
        [TestCase("2018-02-15", "2018-04-15", Description = "Same dates as existing range")]
        public async Task ThenIfBothDatesFallWithinRangeOfSingleExistingApprenticeshipThenIsOverlapping(DateTime startDate, DateTime endDate)
        {
            //Act
            var result = await _overlapCheckService.CheckForOverlaps("1234567890", startDate, endDate, null, new CancellationToken());

            //Assert
            Assert.IsTrue(result.OverlappingStartDate);
            Assert.IsTrue(result.OverlappingEndDate);
        }

        [Test]
        public async Task ThenIfDatesFallWithinRangeOfDifferentExistingApprenticeshipThenOverlapsBoth()
        {
            //Act
            var result = await _overlapCheckService.CheckForOverlaps("1234567890", new DateTime(2018, 03, 15), new DateTime(2018, 07, 15), null, new CancellationToken());

            //Assert
            Assert.IsTrue(result.OverlappingStartDate);
            Assert.IsTrue(result.OverlappingEndDate);
        }

        [Test]
        public async Task ThenIfDatesStraddleExistingApprenticeshipThenIsOverlapping()
        {
            //Act
            var result = await _overlapCheckService.CheckForOverlaps("1234567890", new DateTime(2018, 01, 15), new DateTime(2018, 05, 15), null, new CancellationToken());

            //Assert
            Assert.IsTrue(result.HasOverlaps);
        }

        [Test]
        public async Task ThenAnExistingApprenticeshipShouldNotBeConsideredAsOverlappingWithItself()
        {
            //Act
            var result = await _overlapCheckService.CheckForOverlaps("1234567890", new DateTime(2018, 02, 15), new DateTime(2018, 04, 15), 1, new CancellationToken());

            //Assert
            Assert.IsFalse(result.HasOverlaps);
        }


        private static UlnUtilisation[] CreateTestData()
        {
            var mockData = new List<UlnUtilisation>
            {
                new UlnUtilisation(1, "1234567890", new DateTime(2018,02,15), new DateTime(2018,04,15)),
                new UlnUtilisation(2, "1234567890", new DateTime(2018,06,15), new DateTime(2018,08,15)),
                new UlnUtilisation(3, "1234567890", new DateTime(2020,01,15), new DateTime(2020,12,15))
            };

            return mockData.ToArray();
        }


    }
}
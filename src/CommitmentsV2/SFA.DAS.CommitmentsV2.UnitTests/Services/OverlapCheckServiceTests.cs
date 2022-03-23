using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
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
        public async Task ThenIfDatesDoNotFallWithinRangeOfExistingApprenticeshipThenNotOverlapping(DateTime startDate, DateTime endDate)
        {
            var result = await _fixture.
                WithDateRange(startDate, endDate)
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
        [TestCase("2018-03-15", "2020-09-15", Description = "Start date contained within existing range but later end date")]
        [TestCase("2018-01-15", "2020-03-15", Description = "End date contained within existing range but earlier start date")]
        [TestCase("2018-02-15", "2018-04-15", Description = "Same dates as existing range")]
        [TestCase("2018-02-15", "2018-05-15", Description = "Same start date but later end date")]
        [TestCase("2018-01-15", "2018-04-15", Description = "Same end date but earlier start date")]
        [TestCase("2018-03-01", "2018-03-15", Description = "Start/end within same month within existing range")]
        public async Task ThenIfBothDatesFallWithinRangeOfSingleExistingApprenticeshipThenIsOverlapping(DateTime startDate, DateTime endDate)
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
                .WithDateRange(new DateTime(2018,02,15), new DateTime(2018,04,15))
                .WithExistingApprenticeship()
                .CheckForOverlaps();

            Assert.IsFalse(result.HasOverlaps);
        }

        [Test]
        public async Task ThenIfNoOverlappingEmailRecordsFoundShouldReturnNull()
        {
            var result = await _fixture.CheckForEmailOverlaps();

            Assert.IsNull(result);
        }

        [Test]
        public async Task ThenIfOverlappingEmailRecordsFoundShouldReturnFirstApproved()
        {
            var result = await _fixture.WithApprovedApprenticeOverlappingEmail().CheckForEmailOverlaps();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.FoundOnFullyApprovedApprenticeship);
            Assert.AreEqual(OverlapStatus.OverlappingEndDate, result.OverlapStatus);
        }

        [Test]
        public async Task ThenIfOverlappingEmailRecordsFoundShouldReturnFirstInCohort()
        {
            var result = await _fixture.WithUnapprovedApprenticeOverlappingEmail().CheckForEmailOverlaps();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.FoundOnFullyApprovedApprenticeship);
            Assert.AreEqual(OverlapStatus.DateEmbrace, result.OverlapStatus);
        }

        [Test]
        public async Task ThenIfOverlappingEmailRecordsFoundShouldCallServiceCorrectly()
        {
            await _fixture.WithCohortAndApprenticeId().CheckForEmailOverlaps();
            _fixture.VerifyEmailOverlapServiceIsCalledCorrectly();
        }

        [Test]
        public async Task ThenIfNoOverlappingEmailRecordsFoundInCohortShouldReturnNull()
        {
            var result = await _fixture.WithCohort().CheckForEmailOverlapsInCohort();

            Assert.AreEqual(0, result.Count);
            _fixture.VerifyEmailOverlapServiceIsCalledCorrectlyForCohortCheck();
        }

        [Test]
        public async Task ThenIfOneOverlappingEmailRecordFoundInCohortShouldReturnOneResult()
        {
            var list = new List<OverlappingEmail>
            {
                new OverlappingEmail
                {
                    RowId = 1,
                    IsApproved = true,
                    OverlapStatus = OverlapStatus.OverlappingEndDate
                }
            };

            var result = await _fixture.WithCohort().WithApprovedApprenticeOverlappingEmailForCohort(list).CheckForEmailOverlapsInCohort();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].RowId);
            Assert.AreEqual(OverlapStatus.OverlappingEndDate, result[0].OverlapStatus);
            Assert.IsTrue(result[0].FoundOnFullyApprovedApprenticeship);
        }

        [Test]
        public async Task ThenIfTwoOverlappingEmailRecordsFoundInCohortShouldReturnFirstIfItWasForTheSameRowId()
        {
            var list = new List<OverlappingEmail>
            {
                new OverlappingEmail
                {
                    RowId = 1,
                    IsApproved = true,
                    OverlapStatus = OverlapStatus.OverlappingEndDate
                },
                new OverlappingEmail
                {
                    RowId = 1,
                    IsApproved = false,
                    OverlapStatus = OverlapStatus.DateEmbrace
                }
            };

            var result = await _fixture.WithCohort().WithApprovedApprenticeOverlappingEmailForCohort(list).CheckForEmailOverlapsInCohort();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].RowId);
            Assert.AreEqual(OverlapStatus.OverlappingEndDate, result[0].OverlapStatus);
            Assert.IsTrue(result[0].FoundOnFullyApprovedApprenticeship);
        }

        [Test]
        public async Task ThenIfTwoOverlappingEmailRecordsFoundInCohortShouldReturnBothIfItWasForADifferentRowId()
        {
            var list = new List<OverlappingEmail>
            {
                new OverlappingEmail
                {
                    RowId = 1,
                    IsApproved = true,
                    OverlapStatus = OverlapStatus.OverlappingEndDate
                },
                new OverlappingEmail
                {
                    RowId = 2,
                    IsApproved = false,
                    OverlapStatus = OverlapStatus.DateEmbrace
                }
            };

            var result = await _fixture.WithCohort().WithApprovedApprenticeOverlappingEmailForCohort(list).CheckForEmailOverlapsInCohort();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0].RowId);
            Assert.AreEqual(OverlapStatus.OverlappingEndDate, result[0].OverlapStatus);
            Assert.IsTrue(result[0].FoundOnFullyApprovedApprenticeship);
            Assert.AreEqual(2, result[1].RowId);
            Assert.AreEqual(OverlapStatus.DateEmbrace, result[1].OverlapStatus);
            Assert.IsFalse(result[1].FoundOnFullyApprovedApprenticeship);
        }

        private class OverlapCheckServiceTestFixture
        {
            private readonly OverlapCheckService _overlapCheckService;
            private readonly Mock<IUlnUtilisationService> _ulnUtilisationService;
            private readonly Mock<IEmailOverlapService> _emailOverlapService;
            public ProviderCommitmentsDbContext Db { get; set; }
            private DateTime _startDate;
            private DateTime _endDate;
            private long? _apprenticeshipId;
            private long? _cohortId = null;
            private string _email;

            public OverlapCheckServiceTestFixture()
            {
                _ulnUtilisationService = new Mock<IUlnUtilisationService>();
                _ulnUtilisationService.Setup(x => x.GetUlnUtilisations(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(CreateTestData());

                _emailOverlapService = new Mock<IEmailOverlapService>();
                _emailOverlapService.Setup(x => x.GetOverlappingEmails(It.IsAny<EmailToValidate>(), It.IsAny<long?>(),
                        It.IsAny<CancellationToken>())).ReturnsAsync(new List<OverlappingEmail>());
                _emailOverlapService.Setup(x => x.GetOverlappingEmails(It.IsAny<long>(),
                    It.IsAny<CancellationToken>())).ReturnsAsync(new List<OverlappingEmail>());

                Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                               .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                .Options);

                _email = "any@email.com";

                _overlapCheckService = new OverlapCheckService(_ulnUtilisationService.Object, _emailOverlapService.Object, new Lazy<ProviderCommitmentsDbContext>(() => Db));
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
                _ulnUtilisationService.Setup(x => x.GetUlnUtilisations(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new UlnUtilisation[0]);

                return this;
            }

            public OverlapCheckServiceTestFixture WithUlnUtilisationEffectivelyDeleted()
            {
                var mockData = new List<UlnUtilisation>
                {
                    new UlnUtilisation(1, "", new DateTime(2018, 03, 01), new DateTime(2018, 03, 01))
                };

                _ulnUtilisationService.Setup(x => x.GetUlnUtilisations(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(mockData.ToArray);

                return this;
            }

            public OverlapCheckServiceTestFixture WithApprovedApprenticeOverlappingEmail()
            {
                _emailOverlapService
                    .Setup(x => x.GetOverlappingEmails(It.IsAny<EmailToValidate>(), It.IsAny<long?>(),
                        It.IsAny<CancellationToken>())).ReturnsAsync(new List<OverlappingEmail>
                    {
                        new OverlappingEmail
                        {
                            IsApproved = true,
                            OverlapStatus = OverlapStatus.OverlappingEndDate
                        }
                    });

                return this;
            }

            public OverlapCheckServiceTestFixture WithUnapprovedApprenticeOverlappingEmail()
            {
                _emailOverlapService
                    .Setup(x => x.GetOverlappingEmails(It.IsAny<EmailToValidate>(), It.IsAny<long?>(),
                        It.IsAny<CancellationToken>())).ReturnsAsync(new List<OverlappingEmail>
                    {
                        new OverlappingEmail
                        {
                            IsApproved = false,
                            OverlapStatus = OverlapStatus.DateEmbrace
                        }
                    });

                return this;
            }

            public OverlapCheckServiceTestFixture WithCohortAndApprenticeId()
            {
                _cohortId = 111;
                _apprenticeshipId = 222;
                return this;
            }

            public OverlapCheckServiceTestFixture WithCohort()
            {
                _cohortId = 111;
                return this;
            }

            public OverlapCheckServiceTestFixture WithApprovedApprenticeOverlappingEmailForCohort(List<OverlappingEmail> list)
            {
                _emailOverlapService
                    .Setup(x => x.GetOverlappingEmails(It.IsAny<long>(),
                        It.IsAny<CancellationToken>())).ReturnsAsync(list);
                return this;
            }


            public async Task<OverlapCheckResult> CheckForOverlaps()
            {
                return await _overlapCheckService.CheckForOverlaps("", _startDate.To(_endDate), _apprenticeshipId, new CancellationToken());
            }

            public async Task<EmailOverlapCheckResult> CheckForEmailOverlaps()
            {
                return await _overlapCheckService.CheckForEmailOverlaps(_email, _startDate.To(_endDate), _apprenticeshipId, _cohortId, new CancellationToken());
            }
            public async Task<List<EmailOverlapCheckResult>> CheckForEmailOverlapsInCohort()
            {
                return await _overlapCheckService.CheckForEmailOverlaps(_cohortId.Value, new CancellationToken());
            }

            public void VerifyEmailOverlapServiceIsCalledCorrectly()
            {
                _emailOverlapService.Verify(x=>x.GetOverlappingEmails(It.Is<EmailToValidate>(p=>p.Email == _email && p.StartDate == _startDate && p.EndDate == _endDate && p.ApprenticeshipId == _apprenticeshipId), _cohortId, It.IsAny<CancellationToken>()));
            }

            public void VerifyEmailOverlapServiceIsCalledCorrectlyForCohortCheck()
            {
                _emailOverlapService.Verify(x => x.GetOverlappingEmails(_cohortId.Value, It.IsAny<CancellationToken>()));
            }

            private static UlnUtilisation[] CreateTestData()
            {
                var mockData = new List<UlnUtilisation>
                {
                    new UlnUtilisation(1, "", new DateTime(2018,02,15), new DateTime(2018,04,15)),
                    new UlnUtilisation(2, "", new DateTime(2018,06,15), new DateTime(2018,08,15)),
                    new UlnUtilisation(3, "", new DateTime(2020,01,15), new DateTime(2020,12,15))
                };

                return mockData.ToArray();
            }

        }
    }
}
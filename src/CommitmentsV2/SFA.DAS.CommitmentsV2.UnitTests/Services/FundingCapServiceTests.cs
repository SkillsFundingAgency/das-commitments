using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class FundingCapServiceTests
    {
        private FundingCapServiceTestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new FundingCapServiceTestFixture();
        }

        [Test]
        public async Task ShouldReturnCorrectNumberOfCourses()
        {
            var result = await _fixture.SetApprenticesList().CallFundingCapCourseSummary();

            Assert.That(result, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task TheApprenticeshipCountForEachCourseIsCorrect()
        {
            var result = (await _fixture.SetApprenticesList().CallFundingCapCourseSummary()).ToArray();
            _fixture.AssertApprenticeshipCountsAreCorrect(result);
        }

        [Test]
        public async Task TheApprenticeshipCapForEachCourseIsCorrect()
        {
            var result = (await _fixture.SetApprenticesList().CallFundingCapCourseSummary()).ToArray();
            _fixture.AssertCourseCapsAreCorrect(result);
        }

        [Test]
        public async Task TheApprenticeshipCostExcludesCourseExcess()
        {
            var result = (await _fixture.SetApprenticesList().CallFundingCapCourseSummary()).ToArray();
            _fixture.AssertCourseCostsExcludeTheExcessAmountsWhereCostExceedsCap(result);
        }
        
        private class FundingCapServiceTestFixture
        {
            public FundingCapService FundingCapService;
            public IList<DraftApprenticeship> Apprentices;
            public readonly Mock<ITrainingProgrammeLookup> TrainingProgrammeLookup;
            public readonly SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme TrainingProgramme;
            private static DateTime _startDate = new DateTime(2000, 01, 01);
            private static DateTime _breakDate = new DateTime(2010, 11, 01);
            private static DateTime _endDate = new DateTime(2011, 10, 01);
            private static int _firstCap = 1000;
            private static int _secondCap = 1200;
            private StandardFundingPeriod _fundingPeriod1 = new StandardFundingPeriod { EffectiveFrom = _startDate, EffectiveTo = _breakDate, FundingCap = _firstCap };
            private StandardFundingPeriod _fundingPeriod2 = new StandardFundingPeriod { EffectiveFrom = _breakDate.AddMonths(1), EffectiveTo = _endDate, FundingCap = _secondCap };
            private Fixture _autoFixture;

            public FundingCapServiceTestFixture()
            {
                _autoFixture = new Fixture();
                TrainingProgrammeLookup = new Mock<ITrainingProgrammeLookup>();
                var fundingList = new List<StandardFundingPeriod>
                {
                    _fundingPeriod1, _fundingPeriod2
                };
                TrainingProgramme = new SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme("1","Title", ProgrammeType.Standard,_startDate, _endDate, new List<IFundingPeriod>(fundingList));
                
                TrainingProgrammeLookup.Setup(x => x.GetTrainingProgramme(It.IsAny<string>())).ReturnsAsync(TrainingProgramme);

                FundingCapService = new FundingCapService(TrainingProgrammeLookup.Object);
            }

            public FundingCapServiceTestFixture SetApprenticesList()
            {
                var apprenticeA = new DraftApprenticeship  
                {
                    Id = 1,
                    StartDate = _breakDate.AddMonths(-1),
                    Cost = _firstCap,
                    CourseName = "C1Name",
                    CourseCode = "C1"
                };
                var apprenticeB = new DraftApprenticeship
                {
                    Id = 2,
                    StartDate = _breakDate.AddMonths(2),
                    Cost = _firstCap,
                    CourseName = "C1Name",
                    CourseCode = "C1"
                };
                var apprenticeC = new DraftApprenticeship
                {
                    Id = 3,
                    StartDate = _breakDate,
                    Cost = _firstCap,
                    CourseName = "C2Name",
                    CourseCode = "C2"
                };
                var apprenticeD = new DraftApprenticeship
                {
                    Id = 4,
                    StartDate = _breakDate,
                    Cost = _secondCap,
                    CourseName = "C2Name",
                    CourseCode = "C2"
                };
                var apprenticeE = new DraftApprenticeship
                {
                    StartDate = _breakDate.AddMonths(2),
                    Cost = _secondCap,
                    CourseName = "C2Name",
                    CourseCode = "C2"
                };

                Apprentices = new List<DraftApprenticeship> {apprenticeA, apprenticeB, apprenticeC, apprenticeD, apprenticeE};

                return this;
            }

            public Task<IReadOnlyCollection<FundingCapCourseSummary>> CallFundingCapCourseSummary()
            {
                return FundingCapService.FundingCourseSummary(Apprentices);
            }

            public void AssertApprenticeshipCountsAreCorrect(FundingCapCourseSummary[] result)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(result[0].CourseTitle, Is.EqualTo("C1Name"));
                    Assert.That(result[0].ApprenticeshipCount, Is.EqualTo(2));

                    Assert.That(result[1].CourseTitle, Is.EqualTo("C2Name"));
                    Assert.That(result[1].ApprenticeshipCount, Is.EqualTo(3));
                });
            }

            public void AssertCourseCapsAreCorrect(FundingCapCourseSummary[] result)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(result[0].ActualCap, Is.EqualTo(2200), "Incorrect ActualCap for C1");
                    Assert.That(result[1].ActualCap, Is.EqualTo(3200), "Incorrect ActualCap for C2");
                });
            }

            public void AssertCourseCostsExcludeTheExcessAmountsWhereCostExceedsCap(FundingCapCourseSummary[] result)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(result[0].CappedCost, Is.EqualTo(2000), "Incorrect CappedCost for C1");
                    Assert.That(result[1].CappedCost, Is.EqualTo(3200), "Incorrect CappedCost for C2");
                });
            }
        }
    }
}
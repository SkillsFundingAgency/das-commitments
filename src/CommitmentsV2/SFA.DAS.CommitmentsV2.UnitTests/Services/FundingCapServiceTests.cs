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

            Assert.AreEqual(2, result.Count);
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
                    StartDate = _breakDate.AddMonths(-1),
                    Cost = _firstCap,
                    CourseName = "C1Name",
                    CourseCode = "C1"
                };
                var apprenticeB = new DraftApprenticeship
                {
                    StartDate = _breakDate.AddMonths(2),
                    Cost = _firstCap,
                    CourseName = "C1Name",
                    CourseCode = "C1"
                };
                var apprenticeC = new DraftApprenticeship
                {
                    StartDate = _breakDate,
                    Cost = _firstCap,
                    CourseName = "C2Name",
                    CourseCode = "C2"
                };
                var apprenticeD = new DraftApprenticeship
                {
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
                Assert.AreEqual("C1Name", result[0].CourseTitle);
                Assert.AreEqual(2, result[0].ApprenticeshipCount);

                Assert.AreEqual("C2Name", result[1].CourseTitle);
                Assert.AreEqual(3, result[1].ApprenticeshipCount);
            }

            public void AssertCourseCapsAreCorrect(FundingCapCourseSummary[] result)
            {
                Assert.AreEqual(2200, result[0].ActualCap, "Incorrect ActualCap for C1");
                Assert.AreEqual(3200, result[1].ActualCap, "Incorrect ActualCap for C2");
            }

            public void AssertCourseCostsExcludeTheExcessAmountsWhereCostExceedsCap(FundingCapCourseSummary[] result)
            {
                Assert.AreEqual(2000, result[0].CappedCost, "Incorrect CappedCost for C1");
                Assert.AreEqual(3200, result[1].CappedCost, "Incorrect CappedCost for C2");
            }
        }
    }
}
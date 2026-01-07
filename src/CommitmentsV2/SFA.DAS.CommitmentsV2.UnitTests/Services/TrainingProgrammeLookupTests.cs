using AutoFixture.NUnit3;
using FluentAssertions.Equivalency;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    public class TrainingProgrammeLookupTests
    {
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Test, MoqAutoData]
        public async Task Then_If_There_Is_No_Code_Null_Is_Returned(
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            //Act
            var actual = await service.GetTrainingProgramme("");

            //Assert
            actual.Should().BeNull();
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_If_The_Course_Code_Is_Numeric_Then_Standards_Are_Searched(
            Standard standard,
            List<Standard> standards,
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service
            )
        {
            //Arrange
            standards.Add(standard);
            dbContext.Setup(x => x.Standards).ReturnsDbSet(standards);

            //Act
            var actual = await service.GetTrainingProgramme(standard.LarsCode.ToString());

            //Assert
            actual.CourseCode.Should().Be(standard.LarsCode.ToString());
            actual.Name.Should().Be($"{standard.Title}, Level: {standard.Level}");
            actual.EffectiveFrom.Should().Be(standard.EffectiveFrom);
            actual.EffectiveTo.Should().Be(standard.EffectiveTo);
            actual.ProgrammeType.Should().Be(ProgrammeType.Standard);
            actual.StandardUId.Should().Be(standard.StandardUId);
            actual.Version.Should().Be(standard.Version);
            actual.StandardPageUrl.Should().BeNullOrEmpty();
            actual.Options.Should().BeNullOrEmpty();
            dbContext.Verify(x => x.Frameworks.FindAsync(It.IsAny<int>()), Times.Never);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_If_The_Course_Code_Is_Numeric_Then_Standards_With_No_Options_Then_Null_IsMapped(
            Standard standard,
            List<Standard> standards,
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service
        )
        {
            //Arrange
            standards.Add(standard);
            standard.Options = null;
            dbContext.Setup(x => x.Standards).ReturnsDbSet(standards);

            //Act
            var actual = await service.GetTrainingProgramme(standard.LarsCode.ToString());

            //Assert
            actual.CourseCode.Should().Be(standard.LarsCode.ToString());
            actual.Name.Should().Be($"{standard.Title}, Level: {standard.Level}");
            actual.EffectiveFrom.Should().Be(standard.EffectiveFrom);
            actual.EffectiveTo.Should().Be(standard.EffectiveTo);
            actual.ProgrammeType.Should().Be(ProgrammeType.Standard);
            actual.StandardUId.Should().Be(standard.StandardUId);
            actual.Version.Should().Be(standard.Version);
            actual.StandardPageUrl.Should().BeNullOrEmpty();
            actual.Options.Should().BeNullOrEmpty();
            dbContext.Verify(x => x.Frameworks.FindAsync(It.IsAny<int>()), Times.Never);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_If_It_Is_Not_Numeric_Then_Frameworks_Are_Searched(
            Framework framework,
            List<Framework> frameworks,
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service
            )
        {
            //Arrange
            frameworks.Add(framework);
            dbContext.Setup(x => x.Frameworks).ReturnsDbSet(frameworks);

            //Act
            var actual = await service.GetTrainingProgramme(framework.Id);

            //Assert
            actual.CourseCode.Should().Be(framework.Id);
            actual.Name.Should().Be($"{framework.Title}, Level: {framework.Level} (Framework)");
            actual.EffectiveFrom.Should().Be(framework.EffectiveFrom);
            actual.EffectiveTo.Should().Be(framework.EffectiveTo);
            actual.ProgrammeType.Should().Be(ProgrammeType.Framework);
            dbContext.Verify(x => x.Standards.FindAsync(It.IsAny<int>()), Times.Never);
        }

        [Test, RecursiveMoqAutoData]
        public void Then_If_Find_Standard_Returns_Null_An_Exception_Is_Thrown(
            int standardCode,
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            //Arrange
            dbContext.Setup(x => x.Standards).ReturnsDbSet(new List<Standard>());

            //Act Assert
            Assert.ThrowsAsync<Exception>(() => service.GetTrainingProgramme(standardCode.ToString()), $"The course code {standardCode} was not found");
        }

        [Test, RecursiveMoqAutoData]
        public void Then_If_Find_Framework_Returns_Null_An_Exception_Is_Thrown(
            string frameworkId,
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            //Arrange
            dbContext.Setup(x => x.Frameworks).ReturnsDbSet(new List<Framework>());

            //Act Assert
            Assert.ThrowsAsync<Exception>(() => service.GetTrainingProgramme(frameworkId), $"The course code {frameworkId} was not found");
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_List_Of_Standards_And_Frameworks_When_Getting_All(
            List<Framework> frameworks,
            List<Standard> standards,
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            //Arrange
            standards.ForEach(s => s.IsLatestVersion = true);
            dbContext.Setup(x => x.Frameworks).ReturnsDbSet(frameworks);
            dbContext.Setup(x => x.Standards).ReturnsDbSet(standards);

            //Act
            var actual = (await service.GetAll()).ToList();

            //Assert
            actual.Count.Should().Be(frameworks.Count + standards.Count);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_List_Of_Standards(
            List<Standard> standards,
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            //Arrange
            standards.ForEach(s => s.IsLatestVersion = true);
            dbContext.Setup(x => x.Standards).ReturnsDbSet(standards);

            //Act
            var actual = (await service.GetAllStandards()).ToList();

            //Assert
            actual.Count.Should().Be(standards.Count);
        }

        [Test, RecursiveMoqAutoData]
        public async Task When_GettingTrainingProgrammeVersionByStandardUId_Then_ReturnStandardVersion(
            List<Standard> standards,
            Standard standard,
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            standards.Add(standard);

            dbContext.Setup(x => x.Standards).ReturnsDbSet(standards);

            var actual = await service.GetTrainingProgrammeVersionByStandardUId(standard.StandardUId);

            actual.CourseCode.Should().Be(standard.LarsCode.ToString());
            actual.Name.Should().Be($"{standard.Title}, Level: {standard.Level}");
            actual.StandardPageUrl.Should().Be(standard.StandardPageUrl);
            actual.EffectiveFrom.Should().Be(standard.EffectiveFrom);
            actual.EffectiveTo.Should().Be(standard.EffectiveTo);
            actual.ProgrammeType.Should().Be(ProgrammeType.Standard);
            actual.Options.Should().BeEquivalentTo(standard.Options.Select(o => o.Option));
        }

        [Test, RecursiveMoqAutoData]
        public async Task When_GettingTrainingProgrammeVersion_Then_ReturnStandardVersion(
            List<Standard> standards,
            Standard standard,
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            standards.Add(standard);

            dbContext.Setup(x => x.Standards).ReturnsDbSet(standards);

            var actual = await service.GetTrainingProgrammeVersionByCourseCodeAndVersion(standard.LarsCode.ToString(), standard.Version);

            actual.CourseCode.Should().Be(standard.LarsCode.ToString());
            actual.Name.Should().Be($"{standard.Title}, Level: {standard.Level}");
            actual.StandardPageUrl.Should().Be(standard.StandardPageUrl);
            actual.EffectiveFrom.Should().Be(standard.EffectiveFrom);
            actual.EffectiveTo.Should().Be(standard.EffectiveTo);
            actual.ProgrammeType.Should().Be(ProgrammeType.Standard);
            actual.Options.Should().BeEquivalentTo(standard.Options.Select(o => o.Option));
        }

        [Test, MoqAutoData]
        public async Task When_GettingStandardVersion_And_StartDateAndCourseCodeAreProvided_Then_ReturnCorrectTrainingProgramme(
            DateTime baseDate,
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            var standards = GetStandards(baseDate);

            dbContext.Setup(x => x.Standards).ReturnsDbSet(standards);

            var result = await service.GetCalculatedTrainingProgrammeVersion("1", baseDate.AddDays(1));

            result.Should().BeEquivalentTo(standards[1], TrainingProgrammeEquivalencyAssertionOptions);
        }

        [Test, MoqAutoData]
        public async Task When_GettingStandardVersion_And_StartDateIsOutsideOfTheRangeOfVersionsEffectiveRange_Then_ReturnTheLatestVersion(
            DateTime baseDate,
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            var standards = GetStandards(baseDate);

            dbContext.Setup(x => x.Standards).ReturnsDbSet(standards);

            var result = await service.GetCalculatedTrainingProgrammeVersion("1", baseDate.AddYears(1).AddDays(1));

            result.Should().BeEquivalentTo(standards.Last(), TrainingProgrammeEquivalencyAssertionOptions);
        }

        [Test, MoqAutoData]
        public async Task When_GettingStandardVersion_And_StartDateAndLatestStandardEffectiveToIsNull_Then_ReturnCorrectTrainingProgramme(
            DateTime baseDate,
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            var standards = GetStandards(baseDate);
            standards[standards.Count - 1].VersionLatestStartDate = null;

            dbContext.Setup(x => x.Standards).ReturnsDbSet(standards);

            var result = await service.GetCalculatedTrainingProgrammeVersion("1", baseDate.AddDays(1));

            result.Should().BeEquivalentTo(standards[1], TrainingProgrammeEquivalencyAssertionOptions);
        }

        [Test, MoqAutoData]
        public async Task When_GettingStandardVersion_And_MoreThanOneVersionStartsInSameMonth_Then_ReturnCorrectTrainingProgramme(
            DateTime baseDate,
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            var standards = GetStandards(baseDate);
            standards.Add(
                _fixture.Build<Standard>()
                    .With(s => s.LarsCode, 1)
                    .With(s => s.StandardUId, "ST0001_1.2")
                    .With(s => s.Version, "1.2")
                    .With(s => s.VersionMajor, 1)
                    .With(s => s.VersionMinor, 2)
                    .With(s => s.VersionEarliestStartDate, baseDate)
                    .With(s => s.VersionLatestStartDate, baseDate.AddYears(1).AddDays(1))
                    .With(s => s.FundingPeriods, new List<StandardFundingPeriod>())
                    .With(s => s.ApprenticeshipType, "Apprenticeship")
                .Create());

            dbContext.Setup(x => x.Standards).ReturnsDbSet(standards);

            var result = await service.GetCalculatedTrainingProgrammeVersion("1", baseDate.AddDays(1));

            result.Should().BeEquivalentTo(standards[2], TrainingProgrammeEquivalencyAssertionOptions);
        }

        [Test, MoqAutoData]
        public async Task When_GettingStandardVersion_And_NoStandardsAreFound_Then_ReturnNull(
            DateTime baseDate,
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            dbContext.Setup(x => x.Standards).ReturnsDbSet(new List<Standard>());

            var result = await service.GetCalculatedTrainingProgrammeVersion("5", baseDate.AddDays(1));

            result.Should().BeNull();
        }

        [Test, MoqAutoData]
        public async Task When_GettingStandardVersion_And_ItsAFramework_Then_ReturnNull(
            DateTime baseDate,
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            dbContext.Setup(x => x.Standards).ReturnsDbSet(new List<Standard>());

            var result = await service.GetCalculatedTrainingProgrammeVersion("5-132-1", baseDate.AddDays(1));

            result.Should().BeNull();
        }

        [Test, MoqAutoData]
        public async Task When_GettingStandardVersions_Then_ReturnListOfTrainingProgrammeVersions(
            DateTime baseDate,
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            var standards = GetStandards(baseDate);

            dbContext.Setup(x => x.Standards).ReturnsDbSet(standards);

            var results = await service.GetTrainingProgrammeVersions("1");

            results.Should().BeEquivalentTo(standards, TrainingProgrammeEquivalencyAssertionOptions);
        }

        [Test, MoqAutoData]
        public async Task When_GettingStandardVersions_And_CourseCodeIsEmpty_Then_ReturnNull(TrainingProgrammeLookup service)
        {
            var results = await service.GetTrainingProgrammeVersions(string.Empty);

            results.Should().BeNull();
        }

        [Test, MoqAutoData]
        public async Task When_GettingStandardVersions_And_CourseCodeIsNotInteger_Then_ReturnNull(TrainingProgrammeLookup service)
        {
            var results = await service.GetTrainingProgrammeVersions("1a");

            results.Should().BeNull();
        }

        [Test, MoqAutoData]
        public async Task When_GettingNewerStandardVersions_Then_ReturnListOfNewerVersions(
             DateTime baseDate,
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            var standards = GetStandards(baseDate);
            standards.Add(
                _fixture.Build<Standard>()
                    .With(s => s.LarsCode, 1)
                    .With(s => s.StandardUId, "ST0001_2.0")
                    .With(s => s.IFateReferenceNumber, "ST0001")
                    .With(s => s.Version, "2.0")
                    .With(s => s.VersionMajor, 2)
                    .With(s => s.VersionMinor, 0)
                    .With(s => s.VersionEarliestStartDate, baseDate)
                    .With(s => s.VersionLatestStartDate, baseDate.AddYears(1).AddDays(1))
                    .With(s => s.FundingPeriods, new List<StandardFundingPeriod>())
                .Create());

            dbContext.Setup(x => x.Standards).ReturnsDbSet(standards);

            var result = await service.GetNewerTrainingProgrammeVersions("ST0001_1.0");

            result.Count().Should().Be(2);
            result.Should().Contain(v => v.Version == "1.1");
            result.Should().Contain(v => v.Version == "2.0");
        }

        [Test, MoqAutoData]
        public void When_GettingNewerStandardVersions_And_StandardNotFound_Then_ThrowException(
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service,
            string standardUId)
        {
            dbContext.Setup(x => x.Standards).ReturnsDbSet(new List<Standard>());

            Assert.ThrowsAsync<Exception>(() => service.GetNewerTrainingProgrammeVersions(standardUId), $"Standard {standardUId} was not found");

        }

        [Test, MoqAutoData]
        public async Task When_GettingNewerStandardVersions_And_NoNewerVersions_Then_ReturnNull(
            DateTime baseDate,
            [Frozen] Mock<IProviderCommitmentsDbContext> dbContext,
            TrainingProgrammeLookup service)
        {
            var standards = GetStandards(baseDate);

            dbContext.Setup(x => x.Standards).ReturnsDbSet(standards);

            var result = await service.GetNewerTrainingProgrammeVersions("ST0001_1.1");

            result.Should().BeNull();
        }

        private List<Standard> GetStandards(DateTime baseDate)
        {
            var standards = new List<Standard>
            {
                _fixture.Build<Standard>()
                        .With(s => s.LarsCode, 1)
                        .With(s => s.StandardUId, "ST0001_1.0")
                        .With(s => s.IFateReferenceNumber, "ST0001")
                        .With(s => s.Version, "1.0")
                        .With(s => s.VersionMajor, 1)
                        .With(s => s.VersionMinor, 0)
                        .With(s => s.EffectiveFrom, baseDate.AddYears(-1))
                        .With(s => s.EffectiveTo, baseDate)
                        .With(s => s.VersionEarliestStartDate, baseDate.AddYears(-1))
                        .With(s => s.VersionLatestStartDate, baseDate)
                        .With(s => s.FundingPeriods, new List<StandardFundingPeriod>())
                        .With(s => s.ApprenticeshipType, "Apprenticeship")
                    .Create(),
                _fixture.Build<Standard>()
                        .With(s => s.LarsCode, 1)
                        .With(s => s.StandardUId, "ST0001_1.1")
                        .With(s => s.IFateReferenceNumber, "ST0001")
                        .With(s => s.Version, "1.1")
                        .With(s => s.VersionMajor, 1)
                        .With(s => s.VersionMinor, 1)
                        .With(s => s.EffectiveFrom, baseDate)
                        .With(s => s.EffectiveTo, baseDate.AddYears(1))
                        .With(s => s.VersionEarliestStartDate, baseDate)
                        .With(s => s.VersionLatestStartDate, baseDate.AddYears(1))
                        .With(s => s.FundingPeriods, new List<StandardFundingPeriod>())
                        .With(s => s.ApprenticeshipType, "FoundationApprenticeship")
                    .Create()
            };

            return standards;
        }

        private EquivalencyAssertionOptions<Standard> TrainingProgrammeEquivalencyAssertionOptions(EquivalencyAssertionOptions<Standard> options)
        {
            return options.Excluding(x => x.LarsCode)
                .Excluding(x => x.Title)
                .Excluding(x => x.Level)
                .Excluding(x => x.Duration)
                .Excluding(x => x.MaxFunding)
                .Excluding(x => x.IFateReferenceNumber)
                .Excluding(x => x.IsLatestVersion)
                .Excluding(x => x.StandardPageUrl)
                .Excluding(x => x.VersionMajor)
                .Excluding(x => x.VersionMinor)
                .Excluding(x => x.Options)
                .Excluding(x => x.VersionEarliestStartDate)
                .Excluding(x => x.VersionLatestStartDate)
                .Excluding(x => x.Route)
                .Excluding(x => x.ApprenticeshipType);

        }
    }
}
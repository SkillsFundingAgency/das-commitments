using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Mapping.BulkUpload;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.UnitOfWork.Context;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using TrainingProgramme = SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping;

[TestFixture]
public class ApprenticeshipUnitVersionTests
{
    [Test]
    public async Task AddPersistsVersionResolvedByOuterInsteadOfUnversionedCatalogue()
    {
        var lookup = Lookup();
        var mapper = new AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper(lookup.Object);
        var result = await mapper.Map(new AddDraftApprenticeshipCommand
        {
            CourseCode = "ZSC00001", ActualStartDate = new DateTime(2026, 1, 15),
            TrainingCourseVersion = "1.1", StandardUId = "AU0001_1.1"
        });

        result.TrainingCourseVersion.Should().Be("1.1");
        result.StandardUId.Should().Be("AU0001_1.1");
        result.TrainingCourseVersionConfirmed.Should().BeTrue();
        lookup.Verify(x => x.GetCalculatedTrainingProgrammeVersion(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task EditPassesNewVersionResolvedByOuter()
    {
        var mapper = new UpdateDraftApprenticeshipToDraftApprenticeshipDetailsMapper(Lookup().Object);
        var result = await mapper.Map(new UpdateDraftApprenticeshipCommand
        {
            CourseCode = "ZSC00001", ActualStartDate = new DateTime(2026, 1, 15),
            TrainingCourseVersion = "1.1", StandardUId = "AU0001_1.1"
        });

        result.TrainingCourseVersion.Should().Be("1.1");
        result.StandardUId.Should().Be("AU0001_1.1");
    }

    [TestCase(null, "1.0")]
    [TestCase("1.1", "1.1")]
    public void UnrelatedEditPreservesVersionAndExplicitStampReplacesIt(string incomingVersion, string expectedVersion)
    {
        var draft = new DraftApprenticeship
        {
            CourseCode = "ZSC00001", TrainingCourseVersion = "1.0", StandardUId = "AU0001_1.0",
            TrainingCourseVersionConfirmed = true
        };
        draft.Merge(new DraftApprenticeshipDetails
        {
            TrainingProgramme = Programme(), LearningType = LearningType.ApprenticeshipUnit,
            TrainingCourseVersion = incomingVersion,
            StandardUId = incomingVersion == null ? null : $"AU0001_{incomingVersion}",
            TrainingCourseVersionConfirmed = incomingVersion != null,
            Reference = "Updated reference"
        }, Party.Provider);

        draft.TrainingCourseVersion.Should().Be(expectedVersion);
        draft.StandardUId.Should().Be($"AU0001_{expectedVersion}");
        draft.TrainingCourseVersionConfirmed.Should().BeTrue();
        draft.ProviderRef.Should().Be("Updated reference");
    }

    [Test]
    public async Task FirstCohortPersistsVersionForLearningAimCode()
    {
        var command = new AddCohortCommand(Party.Provider, 1, 2, 3, "ZSC00001", DeliveryModel.Regular,
            1000, null, new DateTime(2026, 1, 15), new DateTime(2026, 4, 15), null, null,
            "Test", "Learner", null, new DateTime(2000, 1, 1), "1234567890", null, null,
            null, null, new UserInfo(), false, null, null, 123, 16, 100)
        {
            TrainingCourseVersion = "1.1", StandardUId = "AU0001_1.1"
        };
        var mapper = new AddCohortCommandToDraftApprenticeshipDetailsMapper(Lookup().Object);

        var result = await mapper.Map(command);

        result.TrainingCourseVersion.Should().Be("1.1");
        result.StandardUId.Should().Be("AU0001_1.1");
        result.TrainingCourseVersionConfirmed.Should().BeTrue();
    }

    [Test]
    public async Task MultiSelectPersistsEachLearnersVersionForSameLearningAimCode()
    {
        var mapper = new BulkUploadAddDraftApprenticeshipRequestToDraftApprenticeshipDetailsMapper(Lookup().Object);
        var result = await mapper.Map(new BulkUploadAddDraftApprenticeshipsCommand
        {
            BulkUploadDraftApprenticeships = new List<BulkUploadAddDraftApprenticeshipRequest>
            {
                new() { CourseCode = "ZSC00001", ActualStartDate = new DateTime(2026, 1, 14), TrainingCourseVersion = "1.0", StandardUId = "AU0001_1.0" },
                new() { CourseCode = "ZSC00001", ActualStartDate = new DateTime(2026, 1, 15), TrainingCourseVersion = "1.1", StandardUId = "AU0001_1.1" }
            }
        });

        result[0].TrainingCourseVersion.Should().Be("1.0");
        result[0].StandardUId.Should().Be("AU0001_1.0");
        result[1].TrainingCourseVersion.Should().Be("1.1");
        result[1].StandardUId.Should().Be("AU0001_1.1");
        result.Should().OnlyContain(x => x.TrainingCourseVersionConfirmed);
    }

    [TestCase(null, "1.0")]
    [TestCase("1.1", "1.1")]
    public void ApprovedUnitUpdatePreservesVersionUnlessNewStampIsProvided(string incomingVersion, string expectedVersion)
    {
        var context = new UnitOfWorkContext();
        var clock = Mock.Of<ICurrentDateTime>(x => x.UtcNow == new DateTime(2026, 2, 1));
        var apprenticeship = new Apprenticeship
        {
            Cohort = new Cohort(), CourseCode = "ZSC00001", ProgrammeType = ProgrammeType.Standard,
            StartDate = new DateTime(2026, 1, 15), EndDate = new DateTime(2026, 4, 15),
            TrainingCourseVersion = "1.0", StandardUId = "AU0001_1.0", TrainingCourseVersionConfirmed = true
        };
        apprenticeship.ApprenticeshipUpdate.Add(new ApprenticeshipUpdate
        {
            Status = ApprenticeshipUpdateStatus.Pending, TrainingType = ProgrammeType.Standard,
            FirstName = "Updated name", TrainingCourseVersion = incomingVersion,
            StandardUId = incomingVersion == null ? null : $"AU0001_{incomingVersion}"
        });

        apprenticeship.ApplyApprenticeshipUpdate(Party.Employer, new UserInfo(), clock,
            SFA.DAS.Common.Domain.Types.LearningType.ApprenticeshipUnit);

        apprenticeship.FirstName.Should().Be("Updated name");
        apprenticeship.TrainingCourseVersion.Should().Be(expectedVersion);
        apprenticeship.StandardUId.Should().Be($"AU0001_{expectedVersion}");
        apprenticeship.TrainingCourseVersionConfirmed.Should().BeTrue();
        context.GetEvents().OfType<SFA.DAS.CommitmentsV2.Messages.Events.ApprenticeshipUpdatedApprovedEvent>()
            .Single().TrainingCourseVersion.Should().Be(expectedVersion);
    }

    [Test]
    public async Task NumericStandardsContinueUsingCalculatedVersionInsteadOfOuterFields()
    {
        var lookup = new Mock<ITrainingProgrammeLookup>();
        lookup.Setup(x => x.GetCalculatedTrainingProgrammeVersion("123", It.IsAny<DateTime>()))
            .ReturnsAsync(new TrainingProgramme("123", "Standard", "1.0", "ST0001_1.0", ProgrammeType.Standard, null, null, new List<IFundingPeriod>(), 3));
        var mapper = new AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper(lookup.Object);

        var result = await mapper.Map(new AddDraftApprenticeshipCommand
        {
            CourseCode = "123", StartDate = new DateTime(2026, 1, 15),
            TrainingCourseVersion = "1.1", StandardUId = "AU0001_1.1"
        });

        result.TrainingCourseVersion.Should().Be("1.0");
        result.StandardUId.Should().Be("ST0001_1.0");
    }

    [Test]
    public async Task FrameworksContinueIgnoringVersionFields()
    {
        var lookup = new Mock<ITrainingProgrammeLookup>();
        lookup.Setup(x => x.GetTrainingProgramme("550-1-1"))
            .ReturnsAsync(new TrainingProgramme("550-1-1", "Framework", ProgrammeType.Framework, null, null));
        var mapper = new AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper(lookup.Object);

        var result = await mapper.Map(new AddDraftApprenticeshipCommand
        {
            CourseCode = "550-1-1", StartDate = new DateTime(2026, 1, 15),
            TrainingCourseVersion = "1.1", StandardUId = "AU0001_1.1"
        });

        result.TrainingCourseVersion.Should().BeNull();
        result.StandardUId.Should().BeNull();
        result.TrainingCourseVersionConfirmed.Should().BeFalse();
    }

    private static TrainingProgramme Programme() => new("ZSC00001", "Unit", ProgrammeType.Standard, null, null);

    private static Mock<ITrainingProgrammeLookup> Lookup()
    {
        var lookup = new Mock<ITrainingProgrammeLookup>();
        lookup.Setup(x => x.GetTrainingProgramme("ZSC00001")).ReturnsAsync(Programme());
        return lookup;
    }
}

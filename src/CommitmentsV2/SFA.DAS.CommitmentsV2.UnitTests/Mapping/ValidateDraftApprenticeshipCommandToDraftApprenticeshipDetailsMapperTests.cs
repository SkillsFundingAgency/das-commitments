using SFA.DAS.CommitmentsV2.Application.Commands.ValidateDraftApprenticeshipDetails;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Types;
using TrainingProgramme = SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping;

[TestFixture]
public class ValidateDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTests
{
    [Test]
    public async Task Map_WhenMapping_ThenShouldSetProperties()
    {
        var fixture = new ValidateDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTestsFixture();
        var result = await fixture.Map();

        result.FirstName.Should().Be(fixture.Command.DraftApprenticeshipRequest.FirstName);
        result.LastName.Should().Be(fixture.Command.DraftApprenticeshipRequest.LastName);
        result.Uln.Should().Be(fixture.Command.DraftApprenticeshipRequest.Uln);
        result.EmploymentPrice.Should().Be(fixture.Command.DraftApprenticeshipRequest.EmploymentPrice);
        result.Cost.Should().Be(fixture.Command.DraftApprenticeshipRequest.Cost);
        result.StartDate.Should().Be(fixture.Command.DraftApprenticeshipRequest.StartDate);
        result.ActualStartDate.Should().Be(fixture.Command.DraftApprenticeshipRequest.ActualStartDate);
        result.EmploymentEndDate.Should().Be(fixture.Command.DraftApprenticeshipRequest.EmploymentEndDate);
        result.EndDate.Should().Be(fixture.Command.DraftApprenticeshipRequest.EndDate);
        result.DateOfBirth.Should().Be(fixture.Command.DraftApprenticeshipRequest.DateOfBirth);
        result.Reference.Should().Be(fixture.Command.DraftApprenticeshipRequest.OriginatorReference);
        result.TrainingProgramme.Should().Be(fixture.TrainingProgramme);
        result.ReservationId.Should().Be(fixture.Command.DraftApprenticeshipRequest.ReservationId);
        result.StandardUId.Should().Be(fixture.TrainingProgramme.StandardUId);
        result.TrainingCourseVersion.Should().Be(fixture.TrainingProgramme.Version);
        result.TrainingCourseVersionConfirmed.Should().BeTrue();
        result.DeliveryModel.Should().Be(fixture.Command.DraftApprenticeshipRequest.DeliveryModel);
    }
}

public class ValidateDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTestsFixture
{
    public Fixture Fixture { get; set; }
    public ValidateDraftApprenticeshipDetailsCommand Command { get; set; }
    public TrainingProgramme TrainingProgramme { get; set; }
    public Mock<ITrainingProgrammeLookup> TrainingProgrammeLookup { get; set; }
    public ValidateDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper Mapper { get; set; }

    public ValidateDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTestsFixture()
    {
        Fixture = new Fixture();
        Command = Fixture.Create<ValidateDraftApprenticeshipDetailsCommand>();
        TrainingProgramme = new TrainingProgramme("TESTS", "TESTStandard", ProgrammeType.Standard, DateTime.MinValue, DateTime.MaxValue);
        TrainingProgrammeLookup = new Mock<ITrainingProgrammeLookup>();
        Mapper = new ValidateDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper(TrainingProgrammeLookup.Object);

        TrainingProgrammeLookup.Setup(l => l.GetTrainingProgramme(It.IsAny<string>())).ReturnsAsync(TrainingProgramme);
        TrainingProgrammeLookup.Setup(l => l.GetCalculatedTrainingProgrammeVersion(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(TrainingProgramme);
    }

    public Task<DraftApprenticeshipDetails> Map()
    {
        Command.DraftApprenticeshipRequest.ActualStartDate = null;
        return Mapper.Map(Command);
    }
}
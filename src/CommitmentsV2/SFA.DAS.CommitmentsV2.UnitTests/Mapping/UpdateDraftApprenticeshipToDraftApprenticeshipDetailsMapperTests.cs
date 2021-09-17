using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing;
using TrainingProgramme = SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping
{
    [TestFixture]
    public class UpdateDraftApprenticeshipToDraftApprenticeshipDetailsMapperTests : FluentTest<UpdateDraftApprenticeshipToDraftApprenticeshipDetailsMapperTestsFixture>
    {
        [Test]
        public Task Map_WhenMapping_ThenShouldSetProperties()
        {
            return TestAsync(
                f => f.MapNoDate(), 
                (f, r) =>
                {
                    r.FirstName.Should().Be(f.Command.FirstName);
                    r.LastName.Should().Be(f.Command.LastName);
                    r.Uln.Should().Be(f.Command.Uln);
                    r.Cost.Should().Be(f.Command.Cost);
                    r.StartDate.Should().Be(f.Command.StartDate);
                    r.EndDate.Should().Be(f.Command.EndDate);
                    r.DateOfBirth.Should().Be(f.Command.DateOfBirth);
                    r.Reference.Should().Be(f.Command.Reference);
                    r.TrainingProgramme.Should().Be(f.TrainingProgramme);
                    r.ReservationId.Should().Be(f.Command.ReservationId);
                    r.StandardUId.Should().Be(f.TrainingProgramme.StandardUId);
                    r.TrainingCourseVersion.Should().Be(f.TrainingProgramme.Version);
                    r.TrainingCourseVersionConfirmed.Should().BeTrue();
                });
        }

        [Test]
        public Task Map_WhenMappingWithDateAndStandardId_Then_UsesCalculatedTrainingProgramme()
        {
            return TestAsync(
                f => f.MapWithStandard(), 
                (f, r) =>
                {
                    r.FirstName.Should().Be(f.Command.FirstName);
                    r.LastName.Should().Be(f.Command.LastName);
                    r.Uln.Should().Be(f.Command.Uln);
                    r.Cost.Should().Be(f.Command.Cost);
                    r.StartDate.Should().Be(f.Command.StartDate);
                    r.EndDate.Should().Be(f.Command.EndDate);
                    r.DateOfBirth.Should().Be(f.Command.DateOfBirth);
                    r.Reference.Should().Be(f.Command.Reference);
                    r.TrainingProgramme.Should().Be(f.TrainingProgramme2);
                    r.ReservationId.Should().Be(f.Command.ReservationId);
                    r.StandardUId.Should().Be(f.TrainingProgramme2.StandardUId);
                    r.TrainingCourseVersion.Should().Be(f.TrainingProgramme2.Version);
                    r.TrainingCourseVersionConfirmed.Should().BeTrue();
                });
        }

        [Test]
        public Task Map_WhenMappingWithFramework_Then_UsesGetTrainingProgramme()
        {
            return TestAsync(
                f => f.Map(),
                (f, r) =>
                {
                    r.FirstName.Should().Be(f.Command.FirstName);
                    r.LastName.Should().Be(f.Command.LastName);
                    r.Uln.Should().Be(f.Command.Uln);
                    r.Cost.Should().Be(f.Command.Cost);
                    r.StartDate.Should().Be(f.Command.StartDate);
                    r.EndDate.Should().Be(f.Command.EndDate);
                    r.DateOfBirth.Should().Be(f.Command.DateOfBirth);
                    r.Reference.Should().Be(f.Command.Reference);
                    r.TrainingProgramme.Should().Be(f.TrainingProgramme2);
                    r.ReservationId.Should().Be(f.Command.ReservationId);
                    r.StandardUId.Should().Be(f.TrainingProgramme2.StandardUId);
                    r.TrainingCourseVersion.Should().Be(f.TrainingProgramme2.Version);
                    r.TrainingCourseVersionConfirmed.Should().BeFalse();
                });
        }

        [Test]
        public Task Map_WhenMappingWithNoCourse_Then_TrainingCourseVersionConfirmedIsFalse()
        {
            return TestAsync(
                f => f.MapNoCourse(), 
                (f, r) =>
                {
                    r.FirstName.Should().Be(f.Command.FirstName);
                    r.LastName.Should().Be(f.Command.LastName);
                    r.Uln.Should().Be(f.Command.Uln);
                    r.Cost.Should().Be(f.Command.Cost);
                    r.StartDate.Should().Be(f.Command.StartDate);
                    r.EndDate.Should().Be(f.Command.EndDate);
                    r.DateOfBirth.Should().Be(f.Command.DateOfBirth);
                    r.Reference.Should().Be(f.Command.Reference);
                    r.TrainingProgramme.Should().BeNull();
                    r.ReservationId.Should().Be(f.Command.ReservationId);
                    r.StandardUId.Should().BeNull();
                    r.TrainingCourseVersion.Should().BeNull();
                    r.TrainingCourseVersionConfirmed.Should().BeFalse();
                });
        }
    }
    
    public class UpdateDraftApprenticeshipToDraftApprenticeshipDetailsMapperTestsFixture
    {
        public Fixture Fixture { get; set; }
        public UpdateDraftApprenticeshipCommand Command { get; set; }
        public TrainingProgramme TrainingProgramme { get; set; }
        public TrainingProgramme TrainingProgramme2 { get; set; }
        public Mock<ITrainingProgrammeLookup> TrainingProgrammeLookup { get; set; }
        public UpdateDraftApprenticeshipToDraftApprenticeshipDetailsMapper Mapper { get; set; }

        public UpdateDraftApprenticeshipToDraftApprenticeshipDetailsMapperTestsFixture()
        {
            Fixture = new Fixture();
            Command = Fixture.Create<UpdateDraftApprenticeshipCommand>();
            TrainingProgramme = new TrainingProgramme("TEST", "TEST", ProgrammeType.Framework, DateTime.MinValue, DateTime.MaxValue);
            TrainingProgramme2 = new TrainingProgramme("TESTS", "TESTStandard", ProgrammeType.Standard, DateTime.MinValue, DateTime.MaxValue);
            TrainingProgrammeLookup = new Mock<ITrainingProgrammeLookup>();
            Mapper = new UpdateDraftApprenticeshipToDraftApprenticeshipDetailsMapper(TrainingProgrammeLookup.Object);
            
            TrainingProgrammeLookup.Setup(l => l.GetTrainingProgramme(It.IsAny<string>())).ReturnsAsync(TrainingProgramme);
            TrainingProgrammeLookup.Setup(l => l.GetCalculatedTrainingProgrammeVersion(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(TrainingProgramme2);
        }

        public Task<DraftApprenticeshipDetails> Map()
        {
            return Mapper.Map(Command);
        }

        public Task<DraftApprenticeshipDetails> MapNoDate()
        {
            Command.StartDate = null;
            return Mapper.Map(Command);
        }

        public Task<DraftApprenticeshipDetails> MapNoCourse()
        {
            Command.StartDate = null;
            TrainingProgrammeLookup.Setup(l => l.GetTrainingProgramme(It.IsAny<string>())).ReturnsAsync((TrainingProgramme)null);
            return Mapper.Map(Command);
        }

        public Task<DraftApprenticeshipDetails> MapWithStandard()
        {
            Command.CourseCode = Fixture.Create<int>().ToString();
            return Mapper.Map(Command);
        }
    }
}

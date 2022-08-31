using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Authorization.Services;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.Testing;
using ProgrammeType = SFA.DAS.CommitmentsV2.Types.ProgrammeType;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping
{
    [TestFixture]
    [Parallelizable]
    public class AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTests : FluentTest<AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTestsFixture>
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
                    r.Reference.Should().Be(f.Command.OriginatorReference);
                    r.TrainingProgramme.Should().Be(f.TrainingProgramme);
                    r.DeliveryModel.Should().Be(f.Command.DeliveryModel);
                    r.ReservationId.Should().Be(f.Command.ReservationId);
                    r.StandardUId.Should().Be(f.TrainingProgramme.StandardUId);
                    r.TrainingCourseVersion.Should().Be(f.TrainingProgramme.Version);
                    r.TrainingCourseVersionConfirmed.Should().BeFalse();
                    r.IsOnFlexiPaymentPilot.Should().Be(f.Command.IsOnFlexiPaymentPilot);
                });
        }

        [Test]
        public Task Map_WhenMappingStandardWithDate_Then_UsesCalculatedTrainingProgramme()
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
                    r.Reference.Should().Be(f.Command.OriginatorReference);
                    r.TrainingProgramme.Should().Be(f.TrainingProgramme2);
                    r.ReservationId.Should().Be(f.Command.ReservationId);
                    r.StandardUId.Should().Be(f.TrainingProgramme2.StandardUId);
                    r.TrainingCourseVersion.Should().Be(f.TrainingProgramme2.Version);
                    r.TrainingCourseVersionConfirmed.Should().BeTrue();
                    r.EmploymentPrice.Should().Be(f.Command.EmploymentPrice);
                    r.EmploymentEndDate.Should().Be(f.Command.EmploymentEndDate);
                    r.IsOnFlexiPaymentPilot.Should().Be(f.Command.IsOnFlexiPaymentPilot);
                });
        }

        [Test]
        public Task Map_WhenMappingFramework_Then_VersionConfirmedIsFalse()
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
                    r.Reference.Should().Be(f.Command.OriginatorReference);
                    r.TrainingProgramme.Should().Be(f.TrainingProgramme);
                    r.ReservationId.Should().Be(f.Command.ReservationId);
                    r.StandardUId.Should().Be(f.TrainingProgramme.StandardUId);
                    r.TrainingCourseVersion.Should().Be(f.TrainingProgramme.Version);
                    r.TrainingCourseVersionConfirmed.Should().BeFalse();
                    r.IsOnFlexiPaymentPilot.Should().Be(f.Command.IsOnFlexiPaymentPilot);
                });
        }

        [Test]
        public Task Map_WhenMappingWithNoCourse_Then_TrainingCourseVersionConfirmedIsFalse()
        {
            return TestAsync(
                f => f.MapNoVersionFieldsWhenStartDateIsNull(), 
                (f, r) =>
                {
                    r.FirstName.Should().Be(f.Command.FirstName);
                    r.LastName.Should().Be(f.Command.LastName);
                    r.Uln.Should().Be(f.Command.Uln);
                    r.Cost.Should().Be(f.Command.Cost);
                    r.StartDate.Should().Be(f.Command.StartDate);
                    r.EndDate.Should().Be(f.Command.EndDate);
                    r.DateOfBirth.Should().Be(f.Command.DateOfBirth);
                    r.Reference.Should().Be(f.Command.OriginatorReference);
                    r.TrainingProgramme.Should().NotBeNull();
                    r.ReservationId.Should().Be(f.Command.ReservationId);
                    r.StandardUId.Should().BeNull();
                    r.TrainingCourseVersion.Should().BeNull();
                    r.TrainingCourseVersionConfirmed.Should().BeFalse();
                    r.IsOnFlexiPaymentPilot.Should().Be(f.Command.IsOnFlexiPaymentPilot);
                });
        }
    }

    public class AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTestsFixture
    {
        public Fixture Fixture { get; set; }
        public AddDraftApprenticeshipCommand Command { get; set; }
        public Mock<IAuthorizationService> AuthorizationService { get; set; }
        public TrainingProgramme TrainingProgramme { get; set; }
        public TrainingProgramme TrainingProgramme2 { get; set; }
        public Mock<ITrainingProgrammeLookup> TrainingProgrammeLookup { get; set; }
        public AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper Mapper { get; set; }

        public AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTestsFixture()
        {
            Fixture = new Fixture();
            Command = Fixture.Build<AddDraftApprenticeshipCommand>().With(x => x.IsOnFlexiPaymentPilot, true).Create();
            AuthorizationService = new Mock<IAuthorizationService>();
            TrainingProgramme = new TrainingProgramme("TEST", "TEST", ProgrammeType.Framework, DateTime.MinValue, DateTime.MaxValue);
            TrainingProgramme2 = new TrainingProgramme("12345", "TESTStandard", ProgrammeType.Standard, DateTime.MinValue, DateTime.MaxValue);
            TrainingProgrammeLookup = new Mock<ITrainingProgrammeLookup>();
            Mapper = new AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper(AuthorizationService.Object, TrainingProgrammeLookup.Object);

            int standardCodeOut;
            TrainingProgrammeLookup.Setup(l => l.GetTrainingProgramme(It.Is<string>(s => int.TryParse(s, out standardCodeOut) == true))).ReturnsAsync(TrainingProgramme2);
            TrainingProgrammeLookup.Setup(l => l.GetTrainingProgramme(It.Is<string>(s => int.TryParse(s, out standardCodeOut) == false))).ReturnsAsync(TrainingProgramme);
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
        
        public Task<DraftApprenticeshipDetails> MapNoVersionFieldsWhenStartDateIsNull()
        {
            Command.StartDate = null;
            Command.CourseCode = Fixture.Create<int>().ToString();
            return Mapper.Map(Command);
        }

        public Task<DraftApprenticeshipDetails> MapWithStandard()
        {
            Command.CourseCode = Fixture.Create<int>().ToString();
            return Mapper.Map(Command);
        }
    }
}
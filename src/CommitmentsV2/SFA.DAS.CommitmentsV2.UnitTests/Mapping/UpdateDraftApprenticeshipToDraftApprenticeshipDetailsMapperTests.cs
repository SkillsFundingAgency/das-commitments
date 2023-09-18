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
using TrainingProgramme = SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping
{
    [TestFixture]
    public class UpdateDraftApprenticeshipToDraftApprenticeshipDetailsMapperTests
    {
        [Test]
        public async Task Map_WhenMapping_ThenShouldSetProperties()
        {
            var fixture = new UpdateDraftApprenticeshipToDraftApprenticeshipDetailsMapperTestsFixture();
            var result = await fixture.Map();
            
            result.FirstName.Should().Be(fixture.Command.FirstName);
            result.LastName.Should().Be(fixture.Command.LastName);
            result.Uln.Should().Be(fixture.Command.Uln);
            result.EmploymentPrice.Should().Be(fixture.Command.EmploymentPrice);
            result.Cost.Should().Be(fixture.Command.Cost);
            result.StartDate.Should().Be(fixture.Command.StartDate);
            result.ActualStartDate.Should().Be(fixture.Command.ActualStartDate);
            result.EmploymentEndDate.Should().Be(fixture.Command.EmploymentEndDate);
            result.EndDate.Should().Be(fixture.Command.EndDate);
            result.DateOfBirth.Should().Be(fixture.Command.DateOfBirth);
            result.Reference.Should().Be(fixture.Command.Reference);
            result.TrainingProgramme.Should().Be(fixture.TrainingProgramme);
            result.ReservationId.Should().Be(fixture.Command.ReservationId);
            result.StandardUId.Should().Be(fixture.TrainingProgramme.StandardUId);
            result.TrainingCourseVersion.Should().Be(fixture.TrainingProgramme.Version);
            result.TrainingCourseVersionConfirmed.Should().BeFalse();
            result.DeliveryModel.Should().Be(fixture.Command.DeliveryModel);
            result.IsOnFlexiPaymentPilot.Should().Be(fixture.Command.IsOnFlexiPaymentPilot);
        }

        [Test]
        public async Task Map_WhenMapping_WithNoDate_VersionPropertiesNotSet()
        {
            var fixture = new UpdateDraftApprenticeshipToDraftApprenticeshipDetailsMapperTestsFixture();
            var result = await fixture.MapNoDateAndNoVersionFields();
            
            result.FirstName.Should().Be(fixture.Command.FirstName);
            result.LastName.Should().Be(fixture.Command.LastName);
            result.Uln.Should().Be(fixture.Command.Uln);
            result.Cost.Should().Be(fixture.Command.Cost);
            result.StartDate.Should().Be(fixture.Command.StartDate);
            result.EndDate.Should().Be(fixture.Command.EndDate);
            result.DateOfBirth.Should().Be(fixture.Command.DateOfBirth);
            result.Reference.Should().Be(fixture.Command.Reference);
            result.TrainingProgramme.Should().Be(fixture.TrainingProgramme2);
            result.ReservationId.Should().Be(fixture.Command.ReservationId);
            result.StandardUId.Should().BeNull();
            result.TrainingCourseVersion.Should().BeNull();
            result.TrainingCourseVersionConfirmed.Should().BeFalse();
            result.DeliveryModel.Should().Be(fixture.Command.DeliveryModel);
        }

        [Test]
        public async Task Map_WhenMappingWithDateAndStandardId_Then_UsesCalculatedTrainingProgramme()
        {
            var fixture = new UpdateDraftApprenticeshipToDraftApprenticeshipDetailsMapperTestsFixture();
            var result = await fixture.MapWithStandard();
            
            result.FirstName.Should().Be(fixture.Command.FirstName);
            result.LastName.Should().Be(fixture.Command.LastName);
            result.Uln.Should().Be(fixture.Command.Uln);
            result.Cost.Should().Be(fixture.Command.Cost);
            result.StartDate.Should().Be(fixture.Command.StartDate);
            result.EndDate.Should().Be(fixture.Command.EndDate);
            result.DateOfBirth.Should().Be(fixture.Command.DateOfBirth);
            result.Reference.Should().Be(fixture.Command.Reference);
            result.TrainingProgramme.Should().Be(fixture.TrainingProgramme2);
            result.ReservationId.Should().Be(fixture.Command.ReservationId);
            result.StandardUId.Should().Be(fixture.TrainingProgramme2.StandardUId);
            result.TrainingCourseVersion.Should().Be(fixture.TrainingProgramme2.Version);
            result.TrainingCourseVersionConfirmed.Should().BeTrue();
            result.DeliveryModel.Should().Be(fixture.Command.DeliveryModel);
        }

        [Test]
        public async Task Map_WhenMappingWithFramework_Then_UsesGetTrainingProgramme()
        {
            var fixture = new UpdateDraftApprenticeshipToDraftApprenticeshipDetailsMapperTestsFixture();
            var result = await fixture.Map();
            
            result.FirstName.Should().Be(fixture.Command.FirstName);
            result.LastName.Should().Be(fixture.Command.LastName);
            result.Uln.Should().Be(fixture.Command.Uln);
            result.Cost.Should().Be(fixture.Command.Cost);
            result.StartDate.Should().Be(fixture.Command.StartDate);
            result.EndDate.Should().Be(fixture.Command.EndDate);
            result.DateOfBirth.Should().Be(fixture.Command.DateOfBirth);
            result.Reference.Should().Be(fixture.Command.Reference);
            result.TrainingProgramme.Should().Be(fixture.TrainingProgramme);
            result.ReservationId.Should().Be(fixture.Command.ReservationId);
            result.StandardUId.Should().Be(fixture.TrainingProgramme.StandardUId);
            result.TrainingCourseVersion.Should().Be(fixture.TrainingProgramme.Version);
            result.TrainingCourseVersionConfirmed.Should().BeFalse();
            result.DeliveryModel.Should().Be(fixture.Command.DeliveryModel);
        }

        [Test]
        public async Task Map_WhenMappingWithNoCourse_Then_TrainingCourseVersionConfirmedIsFalse()
        {
            var fixture = new UpdateDraftApprenticeshipToDraftApprenticeshipDetailsMapperTestsFixture();
            var result = await fixture.MapNoCourse();
            
            result.FirstName.Should().Be(fixture.Command.FirstName);
            result.LastName.Should().Be(fixture.Command.LastName);
            result.Uln.Should().Be(fixture.Command.Uln);
            result.Cost.Should().Be(fixture.Command.Cost);
            result.StartDate.Should().Be(fixture.Command.StartDate);
            result.EndDate.Should().Be(fixture.Command.EndDate);
            result.DateOfBirth.Should().Be(fixture.Command.DateOfBirth);
            result.Reference.Should().Be(fixture.Command.Reference);
            result.TrainingProgramme.Should().BeNull();
            result.ReservationId.Should().Be(fixture.Command.ReservationId);
            result.StandardUId.Should().BeNull();
            result.TrainingCourseVersion.Should().BeNull();
            result.TrainingCourseVersionConfirmed.Should().BeFalse();
            result.DeliveryModel.Should().Be(fixture.Command.DeliveryModel);
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

            int standardCodeOut;
            TrainingProgrammeLookup.Setup(l => l.GetTrainingProgramme(It.Is<string>(s => int.TryParse(s, out standardCodeOut) == true))).ReturnsAsync(TrainingProgramme2);
            TrainingProgrammeLookup.Setup(l => l.GetTrainingProgramme(It.Is<string>(s => int.TryParse(s, out standardCodeOut) == false))).ReturnsAsync(TrainingProgramme);
            TrainingProgrammeLookup.Setup(l => l.GetCalculatedTrainingProgrammeVersion(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(TrainingProgramme2);
        }

        public Task<DraftApprenticeshipDetails> Map()
        {
            return Mapper.Map(Command);
        }

        public Task<DraftApprenticeshipDetails> MapNoDateAndNoVersionFields()
        {
            Command.StartDate = null;
            Command.CourseCode = Fixture.Create<int>().ToString();
            return Mapper.Map(Command);
        }
        
        public Task<DraftApprenticeshipDetails> MapWithDate()
        {
            Command.StartDate = DateTime.Now;
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

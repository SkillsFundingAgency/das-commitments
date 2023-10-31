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
using ProgrammeType = SFA.DAS.CommitmentsV2.Types.ProgrammeType;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping
{
    [TestFixture]
    [Parallelizable]
    public class AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTests 
    {
        [Test]
        public async Task Map_WhenMapping_ThenShouldSetProperties()
        {
            var fixture = new AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTestsFixture();
            var result = await fixture.MapNoDate();
            
            result.FirstName.Should().Be(fixture.Command.FirstName);
            result.LastName.Should().Be(fixture.Command.LastName);
            result.Uln.Should().Be(fixture.Command.Uln);
            result.Cost.Should().Be(fixture.Command.Cost);
            result.StartDate.Should().Be(fixture.Command.StartDate);
            result.ActualStartDate.Should().Be(fixture.Command.ActualStartDate);
            result.EndDate.Should().Be(fixture.Command.EndDate);
            result.TrainingPrice.Should().Be(fixture.Command.TrainingPrice);
            result.EndPointAssessmentPrice.Should().Be(fixture.Command.EndPointAssessmentPrice);
            result.DateOfBirth.Should().Be(fixture.Command.DateOfBirth);
            result.Reference.Should().Be(fixture.Command.OriginatorReference);
            result.TrainingProgramme.Should().Be(fixture.TrainingProgramme);
            result.DeliveryModel.Should().Be(fixture.Command.DeliveryModel);
            result.ReservationId.Should().Be(fixture.Command.ReservationId);
            result.StandardUId.Should().Be(fixture.TrainingProgramme.StandardUId);
            result.TrainingCourseVersion.Should().Be(fixture.TrainingProgramme.Version);
            result.TrainingCourseVersionConfirmed.Should().BeFalse();
            result.IsOnFlexiPaymentPilot.Should().Be(fixture.Command.IsOnFlexiPaymentPilot.Value);
        }

        [Test]
        public async Task Map_WhenMappingStandardWithDate_Then_UsesCalculatedTrainingProgramme()
        {
            var fixture = new AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTestsFixture();
            var result = await fixture.MapWithStandard();
            
            result.FirstName.Should().Be(fixture.Command.FirstName);
            result.LastName.Should().Be(fixture.Command.LastName);
            result.Uln.Should().Be(fixture.Command.Uln);
            result.Cost.Should().Be(fixture.Command.Cost);
            result.StartDate.Should().Be(fixture.Command.StartDate);
            result.ActualStartDate.Should().Be(fixture.Command.ActualStartDate);
            result.EndDate.Should().Be(fixture.Command.EndDate);
            result.DateOfBirth.Should().Be(fixture.Command.DateOfBirth);
            result.Reference.Should().Be(fixture.Command.OriginatorReference);
            result.TrainingProgramme.Should().Be(fixture.TrainingProgramme2);
            result.ReservationId.Should().Be(fixture.Command.ReservationId);
            result.StandardUId.Should().Be(fixture.TrainingProgramme2.StandardUId);
            result.TrainingCourseVersion.Should().Be(fixture.TrainingProgramme2.Version);
            result.TrainingCourseVersionConfirmed.Should().BeTrue();
            result.EmploymentPrice.Should().Be(fixture.Command.EmploymentPrice);
            result.EmploymentEndDate.Should().Be(fixture.Command.EmploymentEndDate);
            result.IsOnFlexiPaymentPilot.Should().Be(fixture.Command.IsOnFlexiPaymentPilot.Value);
        }

        [Test]
        public async Task Map_WhenMappingFramework_Then_VersionConfirmedIsFalse()
        {
            var fixture = new AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTestsFixture();
            var result = await fixture.Map();
            
            result.FirstName.Should().Be(fixture.Command.FirstName);
            result.LastName.Should().Be(fixture.Command.LastName);
            result.Uln.Should().Be(fixture.Command.Uln);
            result.Cost.Should().Be(fixture.Command.Cost);
            result.StartDate.Should().Be(fixture.Command.StartDate);
            result.ActualStartDate.Should().Be(fixture.Command.ActualStartDate);
            result.EndDate.Should().Be(fixture.Command.EndDate);
            result.DateOfBirth.Should().Be(fixture.Command.DateOfBirth);
            result.Reference.Should().Be(fixture.Command.OriginatorReference);
            result.TrainingProgramme.Should().Be(fixture.TrainingProgramme);
            result.ReservationId.Should().Be(fixture.Command.ReservationId);
            result.StandardUId.Should().Be(fixture.TrainingProgramme.StandardUId);
            result.TrainingCourseVersion.Should().Be(fixture.TrainingProgramme.Version);
            result.TrainingCourseVersionConfirmed.Should().BeFalse();
            result.IsOnFlexiPaymentPilot.Should().Be(fixture.Command.IsOnFlexiPaymentPilot.Value);
        }

        [Test]
        public async Task Map_WhenMappingWithNoCourse_Then_TrainingCourseVersionConfirmedIsFalse()
        {
            var fixture = new AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTestsFixture();
            var result = await fixture.MapNoVersionFieldsWhenStartDateIsNull();
            
            result.FirstName.Should().Be(fixture.Command.FirstName);
            result.LastName.Should().Be(fixture.Command.LastName);
            result.Uln.Should().Be(fixture.Command.Uln);
            result.Cost.Should().Be(fixture.Command.Cost);
            result.StartDate.Should().Be(fixture.Command.StartDate);
            result.ActualStartDate.Should().Be(fixture.Command.ActualStartDate);
            result.EndDate.Should().Be(fixture.Command.EndDate);
            result.DateOfBirth.Should().Be(fixture.Command.DateOfBirth);
            result.Reference.Should().Be(fixture.Command.OriginatorReference);
            result.TrainingProgramme.Should().NotBeNull();
            result.ReservationId.Should().Be(fixture.Command.ReservationId);
            result.StandardUId.Should().BeNull();
            result.TrainingCourseVersion.Should().BeNull();
            result.TrainingCourseVersionConfirmed.Should().BeFalse();
            result.IsOnFlexiPaymentPilot.Should().Be(fixture.Command.IsOnFlexiPaymentPilot.Value);
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
            TrainingProgramme = new TrainingProgramme("TEST", "TEST", ProgrammeType.Framework, DateTime.MinValue,
                DateTime.MaxValue);
            TrainingProgramme2 = new TrainingProgramme("12345", "TESTStandard", ProgrammeType.Standard,
                DateTime.MinValue, DateTime.MaxValue);
            TrainingProgrammeLookup = new Mock<ITrainingProgrammeLookup>();
            Mapper = new AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper(AuthorizationService.Object,
                TrainingProgrammeLookup.Object);

            int standardCodeOut;
            TrainingProgrammeLookup
                .Setup(l => l.GetTrainingProgramme(It.Is<string>(s => int.TryParse(s, out standardCodeOut) == true)))
                .ReturnsAsync(TrainingProgramme2);
            TrainingProgrammeLookup
                .Setup(l => l.GetTrainingProgramme(It.Is<string>(s => int.TryParse(s, out standardCodeOut) == false)))
                .ReturnsAsync(TrainingProgramme);
            TrainingProgrammeLookup
                .Setup(l => l.GetCalculatedTrainingProgrammeVersion(It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(TrainingProgramme2);
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
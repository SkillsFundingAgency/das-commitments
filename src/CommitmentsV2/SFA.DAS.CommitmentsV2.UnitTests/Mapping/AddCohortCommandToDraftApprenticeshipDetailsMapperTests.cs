using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Authorization.Services;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using TrainingProgramme = SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping
{
    [TestFixture]
    [Parallelizable]
    public class AddCohortCommandToDraftApprenticeshipDetailsMapperTests
    {
        [Test]
        public async Task WhenMappingStandardWithDate_ThenShouldSetProperties()
        {
            var fixture = new AddCohortCommandToDraftApprenticeshipDetailsMapperTestsFixture();
            var draftApprenticeshipDetails = await fixture.Map();

            Assert.AreEqual(fixture.Command.FirstName, draftApprenticeshipDetails.FirstName);
            Assert.AreEqual(fixture.Command.LastName, draftApprenticeshipDetails.LastName);
            Assert.AreEqual(fixture.Command.Email, draftApprenticeshipDetails.Email);
            Assert.AreEqual(fixture.Command.Uln, draftApprenticeshipDetails.Uln);
            Assert.AreEqual(fixture.Command.Cost, draftApprenticeshipDetails.Cost);
            Assert.AreEqual(fixture.Command.StartDate, draftApprenticeshipDetails.StartDate);
            Assert.AreEqual(fixture.Command.EndDate, draftApprenticeshipDetails.EndDate);
            Assert.AreEqual(fixture.Command.DateOfBirth, draftApprenticeshipDetails.DateOfBirth);
            Assert.AreEqual(fixture.Command.OriginatorReference, draftApprenticeshipDetails.Reference);
            Assert.AreEqual(fixture.TrainingProgrammeStandard, draftApprenticeshipDetails.TrainingProgramme);
            Assert.AreEqual(fixture.TrainingProgrammeStandard.StandardUId, draftApprenticeshipDetails.StandardUId);
            Assert.AreEqual(fixture.TrainingProgrammeStandard.Version, draftApprenticeshipDetails.TrainingCourseVersion);
            Assert.AreEqual(fixture.Command.DeliveryModel, draftApprenticeshipDetails.DeliveryModel);
        }

        [Test]
        public async Task WhenMapping_ThenShouldSetReservationId()
        {
            var fixture = new AddCohortCommandToDraftApprenticeshipDetailsMapperTestsFixture();
            var draftApprenticeshipDetails = await fixture.Map();

            Assert.AreEqual(fixture.Command.ReservationId, draftApprenticeshipDetails.ReservationId);
        }

        [Test]
        public async Task WhenMappingFramework_ThenShouldSetPropertiesAndNoStandardUIdOrVersion()
        {
            var fixture = new AddCohortCommandToDraftApprenticeshipDetailsMapperTestsFixture();
            var draftApprenticeshipDetails = await fixture.MapWithFramework();

            Assert.AreEqual(fixture.Command.FirstName, draftApprenticeshipDetails.FirstName);
            Assert.AreEqual(fixture.Command.LastName, draftApprenticeshipDetails.LastName);
            Assert.AreEqual(fixture.Command.Email, draftApprenticeshipDetails.Email);
            Assert.AreEqual(fixture.Command.Uln, draftApprenticeshipDetails.Uln);
            Assert.AreEqual(fixture.Command.Cost, draftApprenticeshipDetails.Cost);
            Assert.AreEqual(fixture.Command.StartDate, draftApprenticeshipDetails.StartDate);
            Assert.AreEqual(fixture.Command.EndDate, draftApprenticeshipDetails.EndDate);
            Assert.AreEqual(fixture.Command.DateOfBirth, draftApprenticeshipDetails.DateOfBirth);
            Assert.AreEqual(fixture.Command.OriginatorReference, draftApprenticeshipDetails.Reference);
            Assert.AreEqual(fixture.TrainingProgrammeFramework, draftApprenticeshipDetails.TrainingProgramme);
            draftApprenticeshipDetails.StandardUId.Should().BeNullOrEmpty();
            draftApprenticeshipDetails.TrainingCourseVersion.Should().BeNullOrEmpty();
            draftApprenticeshipDetails.TrainingCourseVersionConfirmed.Should().BeFalse();
            Assert.AreEqual(fixture.Command.DeliveryModel, draftApprenticeshipDetails.DeliveryModel);
        }

        [Test]
        public async Task WhenMapping_And_NoMatchingCourse_ThenShouldSetPropertiesAndNoCourseInformation()
        {
            var fixture = new AddCohortCommandToDraftApprenticeshipDetailsMapperTestsFixture();
            var draftApprenticeshipDetails = await fixture.MapNoCourse();

            Assert.AreEqual(fixture.Command.FirstName, draftApprenticeshipDetails.FirstName);
            Assert.AreEqual(fixture.Command.LastName, draftApprenticeshipDetails.LastName);
            Assert.AreEqual(fixture.Command.Email, draftApprenticeshipDetails.Email);
            Assert.AreEqual(fixture.Command.Uln, draftApprenticeshipDetails.Uln);
            Assert.AreEqual(fixture.Command.Cost, draftApprenticeshipDetails.Cost);
            Assert.AreEqual(fixture.Command.StartDate, draftApprenticeshipDetails.StartDate);
            Assert.AreEqual(fixture.Command.EndDate, draftApprenticeshipDetails.EndDate);
            Assert.AreEqual(fixture.Command.DateOfBirth, draftApprenticeshipDetails.DateOfBirth);
            Assert.AreEqual(fixture.Command.OriginatorReference, draftApprenticeshipDetails.Reference);
            Assert.AreEqual(null, draftApprenticeshipDetails.TrainingProgramme);
            Assert.AreEqual(fixture.Command.DeliveryModel, draftApprenticeshipDetails.DeliveryModel);
            draftApprenticeshipDetails.StandardUId.Should().BeNullOrEmpty();
            draftApprenticeshipDetails.TrainingCourseVersion.Should().BeNullOrEmpty();
        }

        [Test]
        public async Task WhenMappingStandard_And_NoStartDate_ThenShouldSetPropertiesAndNoVersioningInformation()
        {
            var fixture = new AddCohortCommandToDraftApprenticeshipDetailsMapperTestsFixture();
            var draftApprenticeshipDetails = await fixture.MapNoStartDate();

            Assert.AreEqual(fixture.Command.FirstName, draftApprenticeshipDetails.FirstName);
            Assert.AreEqual(fixture.Command.LastName, draftApprenticeshipDetails.LastName);
            Assert.AreEqual(fixture.Command.Email, draftApprenticeshipDetails.Email);
            Assert.AreEqual(fixture.Command.Uln, draftApprenticeshipDetails.Uln);
            Assert.AreEqual(fixture.Command.Cost, draftApprenticeshipDetails.Cost);
            Assert.AreEqual(fixture.Command.StartDate, draftApprenticeshipDetails.StartDate);
            Assert.AreEqual(fixture.Command.EndDate, draftApprenticeshipDetails.EndDate);
            Assert.AreEqual(fixture.Command.DateOfBirth, draftApprenticeshipDetails.DateOfBirth);
            Assert.AreEqual(fixture.Command.OriginatorReference, draftApprenticeshipDetails.Reference);
            Assert.AreEqual(fixture.TrainingProgrammeStandard, draftApprenticeshipDetails.TrainingProgramme);
            Assert.AreEqual(fixture.Command.DeliveryModel, draftApprenticeshipDetails.DeliveryModel);
            draftApprenticeshipDetails.StandardUId.Should().BeNullOrEmpty();
            draftApprenticeshipDetails.TrainingCourseVersion.Should().BeNullOrEmpty();
            draftApprenticeshipDetails.TrainingCourseVersionConfirmed.Should().BeFalse();
        }
    }

    public class AddCohortCommandToDraftApprenticeshipDetailsMapperTestsFixture
    {
        public IFixture AutoFixture { get; }
        public AddCohortCommand Command { get; set; }
        public TrainingProgramme TrainingProgrammeFramework { get; }
        public TrainingProgramme TrainingProgrammeStandard { get; }
        public Mock<IAuthorizationService> AuthorizationService { get; }
        public Mock<ITrainingProgrammeLookup> TrainingProgrammeLookup { get; }
        public IOldMapper<AddCohortCommand, DraftApprenticeshipDetails> Mapper { get; }

        public AddCohortCommandToDraftApprenticeshipDetailsMapperTestsFixture()
        {
            AutoFixture = new Fixture();
            AutoFixture.Behaviors
               .OfType<ThrowingRecursionBehavior>()
               .ToList()
               .ForEach(b => AutoFixture.Behaviors.Remove(b));
            AutoFixture.Behaviors.Add(new OmitOnRecursionBehavior(recursionDepth: 2));

            var courseCode = AutoFixture.Create<long>().ToString();
          
            TrainingProgrammeStandard = new TrainingProgramme(AutoFixture.Create<long>().ToString(), AutoFixture.Create<string>(), 
                AutoFixture.Create<string>(), AutoFixture.Create<string>(), ProgrammeType.Standard, AutoFixture.Create<DateTime?>(), AutoFixture.Create<DateTime?>(),
                new List<IFundingPeriod>(AutoFixture.CreateMany<StandardFundingPeriod>()));

            TrainingProgrammeFramework = new TrainingProgramme(courseCode, AutoFixture.Create<string>(),
              ProgrammeType.Framework, AutoFixture.Create<DateTime?>(), AutoFixture.Create<DateTime?>());

            var command = AutoFixture.Build<AddCohortCommand>()
                .Create();

            Command = new AddCohortCommand(command.AccountId, command.AccountLegalEntityId, command.ProviderId,
                courseCode, command.DeliveryModel, command.Cost, command.StartDate, command.EndDate, command.OriginatorReference,
                command.ReservationId, command.FirstName, command.LastName, command.Email, command.DateOfBirth,
                command.Uln, command.TransferSenderId, command.PledgeApplicationId, command.UserInfo);

            AuthorizationService = new Mock<IAuthorizationService>();
            TrainingProgrammeLookup = new Mock<ITrainingProgrammeLookup>();
            Mapper = new AddCohortCommandToDraftApprenticeshipDetailsMapper(AuthorizationService.Object, TrainingProgrammeLookup.Object);

            TrainingProgrammeLookup.Setup(l => l.GetCalculatedTrainingProgrammeVersion(It.IsAny<string>(), Command.StartDate.Value))
                .ReturnsAsync(TrainingProgrammeStandard);

            int standardCodeOut;
            TrainingProgrammeLookup.Setup(l => l.GetTrainingProgramme(It.Is<string>(s => int.TryParse(s, out standardCodeOut) == true))).ReturnsAsync(TrainingProgrammeStandard);
            TrainingProgrammeLookup.Setup(l => l.GetTrainingProgramme(It.Is<string>(s => int.TryParse(s, out standardCodeOut) == false))).ReturnsAsync(TrainingProgrammeFramework);
        }
        public Task<DraftApprenticeshipDetails> Map()
        {
            return Mapper.Map(Command);
        }

        public Task<DraftApprenticeshipDetails> MapNoDate()
        {
            Command = AddCohortCommandNoDate();
            TrainingProgrammeLookup.Setup(l => l.GetTrainingProgramme(Command.CourseCode)).ReturnsAsync(TrainingProgrammeFramework);
            return Mapper.Map(Command);
        }

        public Task<DraftApprenticeshipDetails> MapNoCourse()
        {
            Command = AddCohortCommandNoDate();
            TrainingProgrammeLookup.Setup(l => l.GetTrainingProgramme(Command.CourseCode)).ReturnsAsync((TrainingProgramme)null);
            return Mapper.Map(Command);
        }

        private AddCohortCommand AddCohortCommandNoDate()
        {
            return new AddCohortCommand(Command.AccountId, Command.AccountLegalEntityId, Command.ProviderId,
                Command.CourseCode, Command.DeliveryModel, Command.Cost, null, null, Command.OriginatorReference, Command.ReservationId,
                Command.FirstName, Command.LastName, Command.Email, Command.DateOfBirth, Command.Uln,
                Command.TransferSenderId, Command.PledgeApplicationId, Command.UserInfo);
        }

        public Task<DraftApprenticeshipDetails> MapWithFramework()
        {
            Command = CommandWithFramework();
            TrainingProgrammeLookup.Setup(l => l.GetTrainingProgramme(Command.CourseCode)).ReturnsAsync(TrainingProgrammeFramework);
            return Mapper.Map(Command);
        }

        public Task<DraftApprenticeshipDetails> MapNoStartDate()
        {
            Command = AddCohortCommandNoDate();
            return Mapper.Map(Command);
        }

        private AddCohortCommand CommandWithFramework()
        {
            var frameworkId = AutoFixture.Create<string>();

            return new AddCohortCommand(Command.AccountId, Command.AccountLegalEntityId, Command.ProviderId,
                frameworkId, Command.DeliveryModel, Command.Cost, Command.StartDate, Command.EndDate, Command.OriginatorReference, Command.ReservationId,
                Command.FirstName, Command.LastName, Command.Email, Command.DateOfBirth, Command.Uln,
                Command.TransferSenderId, Command.PledgeApplicationId, Command.UserInfo);
        }
    }
}
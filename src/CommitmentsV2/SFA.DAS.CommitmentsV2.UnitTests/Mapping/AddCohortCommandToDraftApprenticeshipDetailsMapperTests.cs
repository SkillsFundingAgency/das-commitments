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

            Assert.That(draftApprenticeshipDetails.FirstName, Is.EqualTo(fixture.Command.FirstName));
            Assert.That(draftApprenticeshipDetails.LastName, Is.EqualTo(fixture.Command.LastName));
            Assert.That(draftApprenticeshipDetails.Email, Is.EqualTo(fixture.Command.Email));
            Assert.That(draftApprenticeshipDetails.Uln, Is.EqualTo(fixture.Command.Uln));
            Assert.That(draftApprenticeshipDetails.Cost, Is.EqualTo(fixture.Command.Cost));
            Assert.That(draftApprenticeshipDetails.TrainingPrice, Is.EqualTo(fixture.Command.TrainingPrice));
            Assert.That(draftApprenticeshipDetails.EndPointAssessmentPrice, Is.EqualTo(fixture.Command.EndPointAssessmentPrice));
            Assert.That(draftApprenticeshipDetails.StartDate, Is.EqualTo(fixture.Command.StartDate));
            Assert.That(draftApprenticeshipDetails.EndDate, Is.EqualTo(fixture.Command.EndDate));
            Assert.That(draftApprenticeshipDetails.DateOfBirth, Is.EqualTo(fixture.Command.DateOfBirth));
            Assert.That(draftApprenticeshipDetails.Reference, Is.EqualTo(fixture.Command.OriginatorReference));
            Assert.That(draftApprenticeshipDetails.TrainingProgramme, Is.EqualTo(fixture.TrainingProgrammeStandard));
            Assert.That(draftApprenticeshipDetails.StandardUId, Is.EqualTo(fixture.TrainingProgrammeStandard.StandardUId));
            Assert.That(draftApprenticeshipDetails.TrainingCourseVersion, Is.EqualTo(fixture.TrainingProgrammeStandard.Version));
            Assert.That(draftApprenticeshipDetails.DeliveryModel, Is.EqualTo(fixture.Command.DeliveryModel));
            Assert.That(draftApprenticeshipDetails.EmploymentPrice, Is.EqualTo(fixture.Command.EmploymentPrice));
            Assert.That(draftApprenticeshipDetails.EmploymentEndDate, Is.EqualTo(fixture.Command.EmploymentEndDate));
            Assert.That(draftApprenticeshipDetails.IsOnFlexiPaymentPilot, Is.EqualTo(fixture.Command.IsOnFlexiPaymentPilot));
        }

        [Test]
        public async Task WhenMapping_ThenShouldSetReservationId()
        {
            var fixture = new AddCohortCommandToDraftApprenticeshipDetailsMapperTestsFixture();
            var draftApprenticeshipDetails = await fixture.Map();

            Assert.That(draftApprenticeshipDetails.ReservationId, Is.EqualTo(fixture.Command.ReservationId));
        }

        [Test]
        public async Task WhenMappingFramework_ThenShouldSetPropertiesAndNoStandardUIdOrVersion()
        {
            var fixture = new AddCohortCommandToDraftApprenticeshipDetailsMapperTestsFixture();
            var draftApprenticeshipDetails = await fixture.MapWithFramework();

            Assert.That(draftApprenticeshipDetails.FirstName, Is.EqualTo(fixture.Command.FirstName));
            Assert.That(draftApprenticeshipDetails.LastName, Is.EqualTo(fixture.Command.LastName));
            Assert.That(draftApprenticeshipDetails.Email, Is.EqualTo(fixture.Command.Email));
            Assert.That(draftApprenticeshipDetails.Uln, Is.EqualTo(fixture.Command.Uln));
            Assert.That(draftApprenticeshipDetails.Cost, Is.EqualTo(fixture.Command.Cost));
            Assert.That(draftApprenticeshipDetails.TrainingPrice, Is.EqualTo(fixture.Command.TrainingPrice));
            Assert.That(draftApprenticeshipDetails.EndPointAssessmentPrice, Is.EqualTo(fixture.Command.EndPointAssessmentPrice));
            Assert.That(draftApprenticeshipDetails.StartDate, Is.EqualTo(fixture.Command.StartDate));
            Assert.That(draftApprenticeshipDetails.EndDate, Is.EqualTo(fixture.Command.EndDate));
            Assert.That(draftApprenticeshipDetails.DateOfBirth, Is.EqualTo(fixture.Command.DateOfBirth));
            Assert.That(draftApprenticeshipDetails.Reference, Is.EqualTo(fixture.Command.OriginatorReference));
            Assert.That(draftApprenticeshipDetails.TrainingProgramme, Is.EqualTo(fixture.TrainingProgrammeFramework));
            draftApprenticeshipDetails.StandardUId.Should().BeNullOrEmpty();
            draftApprenticeshipDetails.TrainingCourseVersion.Should().BeNullOrEmpty();
            draftApprenticeshipDetails.TrainingCourseVersionConfirmed.Should().BeFalse();
            Assert.That(draftApprenticeshipDetails.DeliveryModel, Is.EqualTo(fixture.Command.DeliveryModel));
            Assert.That(draftApprenticeshipDetails.IsOnFlexiPaymentPilot, Is.EqualTo(fixture.Command.IsOnFlexiPaymentPilot));
        }

        [Test]
        public async Task WhenMapping_And_NoMatchingCourse_ThenShouldSetPropertiesAndNoCourseInformation()
        {
            var fixture = new AddCohortCommandToDraftApprenticeshipDetailsMapperTestsFixture();
            var draftApprenticeshipDetails = await fixture.MapNoCourse();

            Assert.That(draftApprenticeshipDetails.FirstName, Is.EqualTo(fixture.Command.FirstName));
            Assert.That(draftApprenticeshipDetails.LastName, Is.EqualTo(fixture.Command.LastName));
            Assert.That(draftApprenticeshipDetails.Email, Is.EqualTo(fixture.Command.Email));
            Assert.That(draftApprenticeshipDetails.Uln, Is.EqualTo(fixture.Command.Uln));
            Assert.That(draftApprenticeshipDetails.Cost, Is.EqualTo(fixture.Command.Cost));
            Assert.That(draftApprenticeshipDetails.TrainingPrice, Is.EqualTo(fixture.Command.TrainingPrice));
            Assert.That(draftApprenticeshipDetails.EndPointAssessmentPrice, Is.EqualTo(fixture.Command.EndPointAssessmentPrice));
            Assert.That(draftApprenticeshipDetails.StartDate, Is.EqualTo(fixture.Command.StartDate));
            Assert.That(draftApprenticeshipDetails.EndDate, Is.EqualTo(fixture.Command.EndDate));
            Assert.That(draftApprenticeshipDetails.DateOfBirth, Is.EqualTo(fixture.Command.DateOfBirth));
            Assert.That(draftApprenticeshipDetails.Reference, Is.EqualTo(fixture.Command.OriginatorReference));
            Assert.That(draftApprenticeshipDetails.TrainingProgramme, Is.EqualTo(null));
            Assert.That(draftApprenticeshipDetails.DeliveryModel, Is.EqualTo(fixture.Command.DeliveryModel));
            draftApprenticeshipDetails.StandardUId.Should().BeNullOrEmpty();
            draftApprenticeshipDetails.TrainingCourseVersion.Should().BeNullOrEmpty();
            Assert.That(draftApprenticeshipDetails.IsOnFlexiPaymentPilot, Is.EqualTo(fixture.Command.IsOnFlexiPaymentPilot));
        }

        [Test]
        public async Task WhenMappingStandard_And_NoStartDate_ThenShouldSetPropertiesAndNoVersioningInformation()
        {
            var fixture = new AddCohortCommandToDraftApprenticeshipDetailsMapperTestsFixture();
            var draftApprenticeshipDetails = await fixture.MapNoStartDate();

            Assert.That(draftApprenticeshipDetails.FirstName, Is.EqualTo(fixture.Command.FirstName));
            Assert.That(draftApprenticeshipDetails.LastName, Is.EqualTo(fixture.Command.LastName));
            Assert.That(draftApprenticeshipDetails.Email, Is.EqualTo(fixture.Command.Email));
            Assert.That(draftApprenticeshipDetails.Uln, Is.EqualTo(fixture.Command.Uln));
            Assert.That(draftApprenticeshipDetails.Cost, Is.EqualTo(fixture.Command.Cost));
            Assert.That(draftApprenticeshipDetails.TrainingPrice, Is.EqualTo(fixture.Command.TrainingPrice));
            Assert.That(draftApprenticeshipDetails.EndPointAssessmentPrice, Is.EqualTo(fixture.Command.EndPointAssessmentPrice));
            Assert.That(draftApprenticeshipDetails.StartDate, Is.EqualTo(fixture.Command.StartDate));
            Assert.That(draftApprenticeshipDetails.EndDate, Is.EqualTo(fixture.Command.EndDate));
            Assert.That(draftApprenticeshipDetails.DateOfBirth, Is.EqualTo(fixture.Command.DateOfBirth));
            Assert.That(draftApprenticeshipDetails.Reference, Is.EqualTo(fixture.Command.OriginatorReference));
            Assert.That(draftApprenticeshipDetails.TrainingProgramme, Is.EqualTo(fixture.TrainingProgrammeStandard));
            Assert.That(draftApprenticeshipDetails.DeliveryModel, Is.EqualTo(fixture.Command.DeliveryModel));
            draftApprenticeshipDetails.StandardUId.Should().BeNullOrEmpty();
            draftApprenticeshipDetails.TrainingCourseVersion.Should().BeNullOrEmpty();
            draftApprenticeshipDetails.TrainingCourseVersionConfirmed.Should().BeFalse();
            Assert.That(draftApprenticeshipDetails.IsOnFlexiPaymentPilot, Is.EqualTo(fixture.Command.IsOnFlexiPaymentPilot));
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

            Command = new AddCohortCommand(command.RequestingParty, command.AccountId, command.AccountLegalEntityId, command.ProviderId,
                courseCode, command.DeliveryModel, command.Cost, command.StartDate, command.ActualStartDate, command.EndDate, command.OriginatorReference,
                command.ReservationId, command.FirstName, command.LastName, command.Email, command.DateOfBirth,
                command.Uln, command.TransferSenderId, command.PledgeApplicationId, command.EmploymentPrice, command.EmploymentEndDate, command.UserInfo, 
                true, true, command.TrainingPrice, command.EndPointAssessmentPrice);

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
            return new AddCohortCommand(Command.RequestingParty, Command.AccountId, Command.AccountLegalEntityId, Command.ProviderId,
                Command.CourseCode, Command.DeliveryModel, Command.Cost, null, null, null, Command.OriginatorReference, Command.ReservationId,
                Command.FirstName, Command.LastName, Command.Email, Command.DateOfBirth, Command.Uln,
                Command.TransferSenderId, Command.PledgeApplicationId, Command.EmploymentPrice, Command.EmploymentEndDate, Command.UserInfo,
                false, false, Command.TrainingPrice, Command.EndPointAssessmentPrice);
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

            return new AddCohortCommand(Command.RequestingParty, Command.AccountId, Command.AccountLegalEntityId, Command.ProviderId,
                frameworkId, Command.DeliveryModel, Command.Cost, Command.StartDate, Command.ActualStartDate, Command.EndDate, Command.OriginatorReference, Command.ReservationId,
                Command.FirstName, Command.LastName, Command.Email, Command.DateOfBirth, Command.Uln,
                Command.TransferSenderId, Command.PledgeApplicationId, Command.EmploymentPrice, Command.EmploymentEndDate, Command.UserInfo, 
                false, false, Command.TrainingPrice, Command.EndPointAssessmentPrice);
        }
    }
}
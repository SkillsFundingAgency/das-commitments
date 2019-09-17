using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.Authorization;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping
{
    [TestFixture]
    [Parallelizable]
    public class AddCohortCommandToDraftApprenticeshipDetailsMapperTests
    {
        [Test]
        public async Task WhenMapping_ThenShouldSetProperties()
        {
            var fixture = new AddCohortCommandToDraftApprenticeshipDetailsMapperTestsFixture();
            var draftApprenticeshipDetails = await fixture.Map();
            
            Assert.AreEqual(fixture.Command.FirstName, draftApprenticeshipDetails.FirstName);
            Assert.AreEqual(fixture.Command.LastName, draftApprenticeshipDetails.LastName);
            Assert.AreEqual(fixture.Command.Uln, draftApprenticeshipDetails.Uln);
            Assert.AreEqual(fixture.Command.Cost, draftApprenticeshipDetails.Cost);
            Assert.AreEqual(fixture.Command.StartDate, draftApprenticeshipDetails.StartDate);
            Assert.AreEqual(fixture.Command.EndDate, draftApprenticeshipDetails.EndDate);
            Assert.AreEqual(fixture.Command.DateOfBirth, draftApprenticeshipDetails.DateOfBirth);
            Assert.AreEqual(fixture.Command.OriginatorReference, draftApprenticeshipDetails.Reference);
            Assert.AreEqual(fixture.TrainingProgramme, draftApprenticeshipDetails.TrainingProgramme);
        }
        
        [TestCase(false, false)]
        [TestCase(true, true)]
        public async Task WhenReservationsIsEnabled_ThenShouldSetReservationId(bool isReservationsEnabled, bool expectReservationIdIsNotNull)
        {
            var fixture = new AddCohortCommandToDraftApprenticeshipDetailsMapperTestsFixture().SetIsReservationsEnabled(isReservationsEnabled);
            var draftApprenticeshipDetails = await fixture.Map();
            
            Assert.AreEqual(expectReservationIdIsNotNull ? fixture.Command.ReservationId : null, draftApprenticeshipDetails.ReservationId);
        }
    }

    public class AddCohortCommandToDraftApprenticeshipDetailsMapperTestsFixture
    {
        public IFixture AutoFixture { get; }
        public AddCohortCommand Command { get; }
        public TrainingProgramme TrainingProgramme { get; }
        public Mock<IAuthorizationService> AuthorizationService { get; }
        public Mock<ITrainingProgrammeLookup> TrainingProgrammeLookup { get; }
        public IMapper<AddCohortCommand, DraftApprenticeshipDetails> Mapper { get; }

        public AddCohortCommandToDraftApprenticeshipDetailsMapperTestsFixture()
        {
            AutoFixture = new Fixture();
            TrainingProgramme = AutoFixture.Create<TrainingProgramme>();
            Command = AutoFixture.Create<AddCohortCommand>();
            AuthorizationService = new Mock<IAuthorizationService>();
            TrainingProgrammeLookup = new Mock<ITrainingProgrammeLookup>();
            Mapper = new AddCohortCommandToDraftApprenticeshipDetailsMapper(AuthorizationService.Object, TrainingProgrammeLookup.Object);
            
            TrainingProgrammeLookup.Setup(l => l.GetTrainingProgramme(Command.CourseCode)).ReturnsAsync(TrainingProgramme);
        }
<<<<<<< HEAD
       
        private async Task AssertPropertySet(Action<AddCohortCommand> setInput,
            Func<DraftApprenticeshipDetails, bool> expectOutput)
=======

        public Task<DraftApprenticeshipDetails> Map()
        {
            return Mapper.Map(Command);
        }

        public AddCohortCommandToDraftApprenticeshipDetailsMapperTestsFixture SetIsReservationsEnabled(bool isEnabled)
>>>>>>> master
        {
            AuthorizationService.Setup(a => a.IsAuthorizedAsync(Feature.Reservations)).ReturnsAsync(isEnabled);

            return this;
        }
    }
}
using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.Authorization;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Features;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing;
using ProgrammeType = SFA.DAS.CommitmentsV2.Types.ProgrammeType;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping
{
    [TestFixture]
    [Parallelizable]
    public class AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTests : FluentTest<AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTestsFixture>
    {
        [TestCase(false, false)]
        [TestCase(true, true)]
        public Task Map_WhenMapping_ThenShouldSetProperties(bool isReservationsEnabled, bool expectReservationIdSet)
        {
            return TestAsync(
                f => f.SetReservationsEnabled(isReservationsEnabled),
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
                    r.ReservationId.Should().Be(expectReservationIdSet ? f.Command.ReservationId : null);
                });
        }
    }

    public class AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTestsFixture
    {
        public Fixture Fixture { get; set; }
        public AddDraftApprenticeshipCommand Command { get; set; }
        public Mock<IAuthorizationService> AuthorizationService { get; set; }
        public TrainingProgramme TrainingProgramme { get; set; }
        public Mock<ITrainingProgrammeLookup> TrainingProgrammeLookup { get; set; }
        public AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper Mapper { get; set; }

        public AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTestsFixture()
        {
            Fixture = new Fixture();
            Command = Fixture.Create<AddDraftApprenticeshipCommand>();
            AuthorizationService = new Mock<IAuthorizationService>();
            TrainingProgramme = new TrainingProgramme("TEST", "TEST", ProgrammeType.Framework, DateTime.MinValue, DateTime.MaxValue);
            TrainingProgrammeLookup = new Mock<ITrainingProgrammeLookup>();
            Mapper = new AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper(AuthorizationService.Object, TrainingProgrammeLookup.Object);
            
            TrainingProgrammeLookup.Setup(l => l.GetTrainingProgramme(It.IsAny<string>())).ReturnsAsync(TrainingProgramme);
        }

        public Task<DraftApprenticeshipDetails> Map()
        {
            return Mapper.Map(Command);
        }

        public AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTestsFixture SetReservationsEnabled(bool isReservationsEnabled)
        {
            AuthorizationService.Setup(a => a.IsAuthorizedAsync(Feature.Reservations)).ReturnsAsync(isReservationsEnabled);
            
            return this;
        }
    }
}
using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

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
        [Test]
        public Task Map_WhenMapping_ThenShouldSetProperties()
        {
            return TestAsync(f => f.Map(), (f, r) =>
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
            });
        }
    }

    public class AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTestsFixture
    {
        public Fixture Fixture { get; set; }
        public AddDraftApprenticeshipCommand Command { get; set; }
        public TrainingProgramme TrainingProgramme { get; set; }
        public Mock<IMapper<ITrainingProgramme,TrainingProgramme>> TrainingProgrammeMapper { get; set; }
        public Mock<ITrainingProgrammeApiClient> TrainingProgrammeApiClient { get; set; }
        public AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper Mapper { get; set; }

        public AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapperTestsFixture()
        {
            Fixture = new Fixture();
            Command = Fixture.Create<AddDraftApprenticeshipCommand>();
            TrainingProgramme = new TrainingProgramme("TEST", "TEST", ProgrammeType.Framework, DateTime.MinValue, DateTime.MaxValue);
            TrainingProgrammeMapper = new Mock<IMapper<ITrainingProgramme, TrainingProgramme>>();
            TrainingProgrammeApiClient = new Mock<ITrainingProgrammeApiClient>();
            Mapper = new AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper(TrainingProgrammeApiClient.Object, TrainingProgrammeMapper.Object);
            
            TrainingProgrammeMapper.Setup(m => m.Map(It.IsAny<ITrainingProgramme>())).ReturnsAsync(TrainingProgramme);
            TrainingProgrammeApiClient.Setup(c => c.GetTrainingProgramme(It.IsAny<string>())).ReturnsAsync(() => new Framework());
        }

        public Task<DraftApprenticeshipDetails> Map()
        {
            return Mapper.Map(Command);
        }
    }
}
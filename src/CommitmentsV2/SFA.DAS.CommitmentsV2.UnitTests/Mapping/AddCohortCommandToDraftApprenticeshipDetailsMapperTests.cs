using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping
{
    [TestFixture()]
    public class AddCohortCommandToDraftApprenticeshipDetailsMapperTests
    {
        private TrainingProgramme _trainingProgramme;
        private Mock<IMapper<ITrainingProgramme,TrainingProgramme>> _trainingProgrammeMapper;
        private Mock<ITrainingProgrammeApiClient> _trainingProgrammeApi;

        [SetUp]
        public void Arrange()
        {
            _trainingProgramme = new TrainingProgramme("TEST", "TEST", ProgrammeType.Framework, DateTime.MinValue, DateTime.MaxValue);
            _trainingProgrammeMapper = new Mock<IMapper<ITrainingProgramme, TrainingProgramme>>();
            _trainingProgrammeMapper.Setup(x => x.Map(It.IsAny<ITrainingProgramme>())).Returns(_trainingProgramme);

            _trainingProgrammeApi = new Mock<ITrainingProgrammeApiClient>();
            _trainingProgrammeApi.Setup(x => x.GetTrainingProgramme(It.IsAny<string>())).ReturnsAsync(() => new Framework());
        }

        [Test]
        public async Task Map_FirstName_ShouldBeSet()
        {
            const string firstName = "TestFirstName";
            await AssertPropertySet(input => input.FirstName = firstName, output => output.FirstName == firstName);
        }

        [Test]
        public async Task Map_LastName_ShouldBeSet()
        {
            const string lastName = "TestLastName";
            await AssertPropertySet(input => input.LastName = lastName, output => output.LastName == lastName);
        }

        [Test]
        public async Task Map_Uln_ShouldBeSet()
        {
            const string xxx = "TestULN";
            await AssertPropertySet(input => input.ULN = xxx, output => output.Uln == xxx);
        }

        [Test]
        public async Task Map_Cost_ShouldBeSet()
        {
            const int cost = 12345;
            await AssertPropertySet(input => input.Cost = cost, output => output.Cost == cost);
        }

        [Test]
        public async Task Map_StartDate_ShouldBeSet()
        {
            var startDate = new DateTime(2020,10,15);
            await AssertPropertySet(input => input.StartDate = startDate, output => output.StartDate == startDate);
        }

        [Test]
        public async Task Map_EndDate_ShouldBeSet()
        {
            var endDate = new DateTime(2022, 5, 8);
            await AssertPropertySet(input => input.EndDate = endDate, output => output.EndDate == endDate);
        }

        [Test]
        public async Task Map_DateOfBirth_ShouldBeSet()
        {
            var dateOfBirth = new DateTime(2004, 1, 2);
            await AssertPropertySet(input => input.DateOfBirth = dateOfBirth, output => output.DateOfBirth == dateOfBirth);
        }

        [Test]
        public async Task Map_ProviderRef_ShouldBeSet()
        {
            const string providerRef = "TestProviderRef";
            await AssertPropertySet(input => input.OriginatorReference = providerRef, output => output.Reference == providerRef);
        }

        [Test]
        public async Task Map_TrainingProgramme_ShouldBeSet()
        {
            await AssertPropertySet(input => input.CourseCode = "TEST", output => output.TrainingProgramme == _trainingProgramme);
        }

        [Test]
        public async Task Map_ReservationId_ShouldBeSet()
        {
            var reservationId = Guid.NewGuid();
            await AssertPropertySet(input => input.ReservationId = reservationId, output => output.ReservationId == reservationId);
        }

        private async Task AssertPropertySet(Action<AddCohortCommand> setInput, Func<DraftApprenticeshipDetails, bool> expectOutput)
        {
            var mapper = new AddCohortCommandToDraftApprenticeshipDetailsMapper(_trainingProgrammeApi.Object, _trainingProgrammeMapper.Object);

            var input = new AddCohortCommand();

            setInput.Invoke(input);

            var output = await mapper.Map(input);

            Assert.IsTrue(expectOutput(output));
        }
    }
}

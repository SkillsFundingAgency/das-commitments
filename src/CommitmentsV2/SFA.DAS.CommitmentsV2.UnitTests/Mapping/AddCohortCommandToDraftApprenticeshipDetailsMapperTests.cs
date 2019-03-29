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

        private Mock<ITrainingProgrammeApiClient> _trainingProgrammeApiClient;
        private Mock<ITrainingProgramme> _trainingProgramme;

        [SetUp]
        public void Arrange()
        {
            _trainingProgramme = new Mock<ITrainingProgramme>();
            
            _trainingProgrammeApiClient = new Mock<ITrainingProgrammeApiClient>();
            _trainingProgrammeApiClient.Setup(x => x.GetTrainingProgramme(It.IsAny<string>())).ReturnsAsync(_trainingProgramme.Object);
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
            await AssertPropertySet(input => input.OriginatorReference = providerRef, output => output.ProviderRef == providerRef);
        }

        [Test]
        public async Task Map_TrainingCode_ShouldBeSet()
        {
            const string courseCode = "TestCourseCode";
            await AssertPropertySet(input => input.CourseCode = courseCode, output => output.TrainingCode == courseCode);
        }

        [Test]
        public async Task Map_TrainingType_ShouldBeSet()
        {
            const ProgrammeType programmeType = ProgrammeType.Framework;
            _trainingProgramme.Setup(x => x.ProgrammeType).Returns(programmeType);

            await AssertPropertySet(input => input.CourseCode = "test", output => output.TrainingType == (int) programmeType);
        }

        [Test]
        public async Task Map_TrainingName_ShouldBeSet()
        {
            var trainingName = "TestTrainingName";
            _trainingProgramme.Setup(x => x.ExtendedTitle).Returns(trainingName);
            await AssertPropertySet(input => input.CourseCode = "test", output => output.TrainingName == trainingName);
        }

        [Test]
        public async Task Map_ReservationId_ShouldBeSet()
        {
            var reservationId = Guid.NewGuid();
            await AssertPropertySet(input => input.ReservationId = reservationId, output => output.ReservationId == reservationId);
        }

        private async Task AssertPropertySet(Action<AddCohortCommand> setInput, Func<DraftApprenticeshipDetails, bool> expectOutput)
        {
            var mapper = new AddCohortCommandToDraftApprenticeshipDetailsMapper(_trainingProgrammeApiClient.Object);

            var input = new AddCohortCommand();

            setInput.Invoke(input);

            var output = await mapper.Map(input);

            Assert.IsTrue(expectOutput(output));
        }
    }
}

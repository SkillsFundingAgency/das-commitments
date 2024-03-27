using AutoFixture.NUnit3;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllTrainingProgrammes;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetAllTrainingProgrammes
{
    public class WhenGettingAllTrainingProgrammes
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_The_Data_Is_Retrieved_From_The_Service(
            GetAllTrainingProgrammesQuery query,
            List<TrainingProgramme> result,
            [Frozen] Mock<ITrainingProgrammeLookup> service,
            GetAllTrainingProgrammesQueryHandler handler)
        {
            service.Setup(x => x.GetAll()).ReturnsAsync(result);
            
            var actual = await handler.Handle(query, CancellationToken.None);

            actual.TrainingProgrammes.Should().BeEquivalentTo(result, opt => opt.Excluding(x => x.Options));
        }
    }
}
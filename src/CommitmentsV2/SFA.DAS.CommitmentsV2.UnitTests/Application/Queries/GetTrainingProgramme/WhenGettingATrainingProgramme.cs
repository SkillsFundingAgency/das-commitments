using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgramme;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetTrainingProgramme
{
    public class WhenGettingATrainingProgramme
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_The_Data_Is_Retrieved_From_The_Service(
            GetTrainingProgrammeQuery query,
            TrainingProgramme result,
            [Frozen] Mock<ITrainingProgrammeLookup> service,
            GetTrainingProgrammeQueryHandler handler)
        {
            service.Setup(x => x.GetTrainingProgramme(query.Id)).ReturnsAsync(result);
            
            var actual = await handler.Handle(query, CancellationToken.None);

            actual.TrainingProgramme.Should().BeEquivalentTo(result);
        }
    }
}
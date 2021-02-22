using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllTrainingProgrammeStandards;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetAllTrainingProgrammeStandards
{
    public class WhenGettingAllTrianingProgrammeStandards
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_The_Data_Is_Retrieved_From_The_Service(
            GetAllTrainingProgrammeStandardsQuery query,
            List<TrainingProgramme> result,
            [Frozen] Mock<ITrainingProgrammeLookup> service,
            GetAllTrainingProgrammeStandardsQueryHandler handler)
        {
            service.Setup(x => x.GetAllStandards()).ReturnsAsync(result);
            
            var actual = await handler.Handle(query, CancellationToken.None);

            actual.TrainingProgrammes.Should().BeEquivalentTo(result);
        }
    }
}
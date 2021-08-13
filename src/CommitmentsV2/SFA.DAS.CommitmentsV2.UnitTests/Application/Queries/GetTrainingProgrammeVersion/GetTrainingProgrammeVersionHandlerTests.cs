using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.Testing.AutoFixture;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetTrainingProgrammeVersion
{
    public class GetTrainingProgrammeVersionHandlerTests
    {
        [Test, MoqAutoData]
        public async Task Then_ReturnTrainingProgrammeVersion(
            GetTrainingProgrammeVersionQuery query,
            TrainingProgramme trainingProgrammeResult,
            [Frozen] Mock<ITrainingProgrammeLookup> service,
            GetTrainingProgrammeVersionHandler handler)
        {
            service.Setup(x => x.GetTrainingProgrammeVersionByStandardUId(query.StandardUId)).ReturnsAsync(trainingProgrammeResult);

            var result = await handler.Handle(query, CancellationToken.None);

            result.TrainingProgramme.Should().BeEquivalentTo(trainingProgrammeResult);
        }

        [Test, MoqAutoData]
        public async Task And_StandardVersionNotFound_Then_ReturnEmptyResponse(
            GetTrainingProgrammeVersionQuery query,
            [Frozen] Mock<ITrainingProgrammeLookup> service,
            GetTrainingProgrammeVersionHandler handler)
        {
            service.Setup(x => x.GetTrainingProgrammeVersionByStandardUId(query.StandardUId)).ThrowsAsync(new Exception("Course not found"));

            var result = await handler.Handle(query, CancellationToken.None);

            result.TrainingProgramme.Should().BeNull();
        }
    }
}

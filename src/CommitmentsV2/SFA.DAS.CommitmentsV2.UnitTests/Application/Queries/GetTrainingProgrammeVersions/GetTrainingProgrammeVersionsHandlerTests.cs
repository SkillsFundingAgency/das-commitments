using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersions;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetTrainingProgrammeVersions
{
    public class GetTrainingProgrammeVersionsHandlerTests
    {
        private GetTrainingProgrammeVersionsQuery _query;
        private GetTrainingProgrammeVersionsQueryHandler _handler;

        private IEnumerable<TrainingProgramme> _versionsResult;
        private Mock<ITrainingProgrammeLookup> _mockTrainingProgrammeService;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture();

            _query = new GetTrainingProgrammeVersionsQuery(fixture.Create<int>().ToString());

            _versionsResult = fixture.CreateMany<TrainingProgramme>();

            _mockTrainingProgrammeService = new Mock<ITrainingProgrammeLookup>();

            _handler = new GetTrainingProgrammeVersionsQueryHandler(_mockTrainingProgrammeService.Object, Mock.Of<ILogger<GetTrainingProgrammeVersionsQueryHandler>>());
        }

        [Test]
        public async Task Then_ReturnTrainingProgrammeVersions()
        {
            _mockTrainingProgrammeService.Setup(x => x.GetTrainingProgrammeVersions(_query.Id))
                .ReturnsAsync(_versionsResult);

            var result = await _handler.Handle(_query, CancellationToken.None);

            result.TrainingProgrammes.Should().BeEquivalentTo(_versionsResult);
        }

        [Test]
        public async Task And_LookupServiceThrowsException_Then_ReturnEmptyResponse()
        {
            _mockTrainingProgrammeService.Setup(x => x.GetTrainingProgrammeVersions(_query.Id))
                .Throws(new Exception());

            var result = await _handler.Handle(_query, CancellationToken.None);

            result.TrainingProgrammes.Should().BeNull();
        }
    }
}

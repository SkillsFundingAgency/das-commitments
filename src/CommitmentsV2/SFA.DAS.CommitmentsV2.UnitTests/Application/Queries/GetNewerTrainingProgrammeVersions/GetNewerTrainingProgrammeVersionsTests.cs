using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetNewerTrainingProgrammeVersions;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetNewerTrainingProgrammeVersions
{
    public class GetNewerTrainingProgrammeVersionsTests
    {
        private GetNewerTrainingProgrammeVersionsQuery _query;
        private GetNewerTrainingProgrammeVersionsQueryHandler _handler;

        private IEnumerable<TrainingProgramme> _newVersions;

        private Mock<ITrainingProgrammeLookup> _mockTrainingProgrammeService;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture();

            _query = fixture.Create<GetNewerTrainingProgrammeVersionsQuery>();

            _newVersions = fixture.CreateMany<TrainingProgramme>();
            
            _mockTrainingProgrammeService = new Mock<ITrainingProgrammeLookup>();

            _handler = new GetNewerTrainingProgrammeVersionsQueryHandler(_mockTrainingProgrammeService.Object, Mock.Of<ILogger<GetNewerTrainingProgrammeVersionsQueryHandler>>());
        }

        [Test]
        public async Task Then_ReturnTrainingProgrammeVersions()
        {
            _mockTrainingProgrammeService.Setup(x => x.GetTrainingProgrammeVersions(_query.StandardUId))
                .ReturnsAsync(_newVersions);

            var result = await _handler.Handle(_query, CancellationToken.None);

            result.NewerVersions.Should().BeEquivalentTo(_newVersions);
        }

        [Test]
        public async Task And_LookupServiceThrowsException_Then_ReturnEmptyResponse()
        {
            _mockTrainingProgrammeService.Setup(x => x.GetTrainingProgrammeVersions(_query.StandardUId))
                .Throws(new Exception());

            var result = await _handler.Handle(_query, CancellationToken.None);

            result.NewerVersions.Should().BeNull();
        }
    }
}

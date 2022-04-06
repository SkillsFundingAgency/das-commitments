using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatistics;
using SFA.DAS.CommitmentsV2.Mapping.ResponseMappers;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.ResponseMappers
{
    [TestFixture]
    public class GetApprenticeshipStatisticsResponseMapperTests
    {
        private Fixture _fixture;
        private GetApprenticeshipStatisticsQueryResult _queryResult;
        private GetApprenticeshipStatisticsResponseMapper _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _queryResult = _fixture.Create<GetApprenticeshipStatisticsQueryResult>();
            _sut = new GetApprenticeshipStatisticsResponseMapper();
        }

        [Test]
        public async Task WhenMapIsCalled_ThenResponseIsMappedCorrectly()
        {
            //Act
            var response = await _sut.Map(_queryResult);

            //Assert
            response.Paused.Should().Be(_queryResult.PausedApprenticeshipCount);
            response.Approved.Should().Be(_queryResult.ApprovedApprenticeshipCount);
            response.Stopped.Should().Be(_queryResult.StoppedApprenticeshipCount);
        }
    }
}
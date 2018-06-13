using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetStatistics;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetStatistics
{
    [TestFixture]
    public sealed class WhenGettingStatistics
    {
        private Mock<IStatisticsRepository> _mockStatisticsRepository;
        private GetStatisticsQueryHandler _handler;
        private GetStatisticsRequest _exampleValidRequest;
        private Statistics _fakeRepositoryStatistics;

        [SetUp]
        public void Setup()
        {
            _mockStatisticsRepository = new Mock<IStatisticsRepository>();
            _handler = new GetStatisticsQueryHandler(_mockStatisticsRepository.Object);

            var dataFixture = new Fixture();
            _fakeRepositoryStatistics = dataFixture.Build<Statistics>().Create();

            _exampleValidRequest = new GetStatisticsRequest();

        }

        [Test]
        public async Task ThenTheStatisticsRepositoryIsCalled()
        {
            await _handler.Handle(_exampleValidRequest);

            _mockStatisticsRepository.Verify(x=>x.GetStatistics(), Times.Once);
        }


        [Test]
        public async Task ThenShouldReturnStatisticsInResponse()
        {
            _mockStatisticsRepository.Setup(x => x.GetStatistics()).ReturnsAsync(_fakeRepositoryStatistics);

            var response = await _handler.Handle(_exampleValidRequest);

            response.Data.TotalApprenticeships.Should().Be(_fakeRepositoryStatistics.TotalApprenticeships);
            response.Data.ActiveApprenticeships.Should().Be(_fakeRepositoryStatistics.ActiveApprenticeships);
            response.Data.TotalCohorts.Should().Be(_fakeRepositoryStatistics.TotalCohorts);
        }
    }
}

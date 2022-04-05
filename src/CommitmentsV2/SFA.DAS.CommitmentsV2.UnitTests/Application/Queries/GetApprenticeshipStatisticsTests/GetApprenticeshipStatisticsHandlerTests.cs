using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatistics;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeshipStatisticsTests
{
    [TestFixture]
    public class GetApprenticeshipStatisticsHandlerTests
    {
        private GetApprenticeshipStatisticsQuery _query;
        private GetApprenticeshipStatisticsQueryResult _result;
        private Mock<IApprenticeshipStatusSummaryService> _apprenticeshipStatusSummaryServiceMock;
        private GetApprenticeshipStatisticsQueryHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _query = new GetApprenticeshipStatisticsQuery();
            _result = new GetApprenticeshipStatisticsQueryResult();

            _apprenticeshipStatusSummaryServiceMock = new Mock<IApprenticeshipStatusSummaryService>();
            _apprenticeshipStatusSummaryServiceMock
                .Setup(x => x.GetApprenticeshipStatisticsFor(_query.LastNumberOfDays))
                .ReturnsAsync(_result);

            _sut = new GetApprenticeshipStatisticsQueryHandler(_apprenticeshipStatusSummaryServiceMock.Object);
        }

        [Test]
        public async Task WhenHandling_ThenCallsApprenticeshipStatusSummaryServiceWithCorrectValues()
        {
            //Arrange
            _query.LastNumberOfDays = 30;

            //Act
            await _sut.Handle(_query, default);

            //Assert
            _apprenticeshipStatusSummaryServiceMock.Verify(x => x.GetApprenticeshipStatisticsFor(_query.LastNumberOfDays), Times.Once);
        }
    }
}
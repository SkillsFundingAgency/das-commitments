using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeshipStatusSummary
{
    public class GetApprenticeshipStatusSummaryHandlerTests
    {
        private GetApprenticeshipStatusSummaryQueryHandler _handler;
        private GetApprenticeshipStatusSummaryQuery _query;
        private Mock<IApprenticeshipStatusSummaryService> _mockApprenticeshipStatusSummaryService;

        [Test]
        public async Task Handle_ThenShouldReturnResultWithValues()
        {
            //Arrange            
            _mockApprenticeshipStatusSummaryService = new Mock<IApprenticeshipStatusSummaryService>();
            _mockApprenticeshipStatusSummaryService.Setup(x => x.GetApprenticeshipStatusSummary(It.IsAny<long>(), CancellationToken.None)).Returns(Task.FromResult(It.IsAny<GetApprenticeshipStatusSummaryQueryResults>()));
            _query = new GetApprenticeshipStatusSummaryQuery(It.IsAny<long>());
            _handler = new GetApprenticeshipStatusSummaryQueryHandler(_mockApprenticeshipStatusSummaryService.Object);

            //Act
            await _handler.Handle(_query, CancellationToken.None);

            //Assert
            _mockApprenticeshipStatusSummaryService.Verify(x => x.GetApprenticeshipStatusSummary(It.IsAny<long>(), CancellationToken.None), Times.Once);
        }

    }    
}

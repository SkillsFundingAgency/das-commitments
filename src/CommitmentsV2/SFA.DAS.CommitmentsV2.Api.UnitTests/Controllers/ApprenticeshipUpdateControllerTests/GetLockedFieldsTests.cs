using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipLockedFields;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ApprenticeshipUpdateControllerTests
{
    [TestFixture]
    [Parallelizable]
    public class GetLockedFieldsTests
    {
        private GetLockedFieldsTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetLockedFieldsTestsFixture();
        }

        [Test]
        public async Task WhenGetLockedFieldsRequestReceived_ThenShouldPassApprenticeIdToQuery()
        {
            await _fixture.GetLockedFields();
            _fixture.VerifyApprenticeshipIdPassedToQuery();
        }

        [Test]
        public async Task WhenGetLockedFieldsRequestReceivedAndNoStatusFound_ThenShouldReturnNotFound()
        {
            await _fixture.WithNoStatusFound().GetLockedFields();
            _fixture.VerifyNotFoundResult();
        }

        [Test]
        public async Task WhenGetLockedFieldsRequestReceived_ThenShouldReturnOkWithQueryResult()
        {
            await _fixture.GetLockedFields();
            _fixture.VerifyResult();
        }

        private class GetLockedFieldsTestsFixture
        {
            public IFixture AutoFixture { get; }
            public Mock<IMediator> Mediator { get; }
            public Mock<IModelMapper> ModelMapper { get; }
            public ApprenticeshipUpdateController Controller { get; }
            public long ApprenticeshipId { get; }
            public GetApprenticeshipLockedFieldsQueryResult QueryResult { get; private set; }
            public IActionResult Result { get; private set; }

            public GetLockedFieldsTestsFixture()
            {
                AutoFixture = new Fixture();

                QueryResult = AutoFixture.Create<GetApprenticeshipLockedFieldsQueryResult>();

                Mediator = new Mock<IMediator>();
                Mediator.Setup(x => x.Send(It.IsAny<GetApprenticeshipLockedFieldsQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(QueryResult);

                ModelMapper = new Mock<IModelMapper>();

                ApprenticeshipId = AutoFixture.Create<long>();
                Controller = new ApprenticeshipUpdateController(Mediator.Object, ModelMapper.Object);
            }

            public GetLockedFieldsTestsFixture WithNoStatusFound()
            {
                QueryResult = null;
                Mediator.Setup(x => x.Send(It.IsAny<GetApprenticeshipLockedFieldsQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((GetApprenticeshipLockedFieldsQueryResult)null);

                return this;
            }

            public async Task GetLockedFields()
            {
                Result = await Controller.GetLockedFields(ApprenticeshipId);
            }

            public void VerifyApprenticeshipIdPassedToQuery()
            {
                Mediator.Verify(x => x.Send(It.Is<GetApprenticeshipLockedFieldsQuery>(p => p.ApprenticeshipId == ApprenticeshipId), It.IsAny<CancellationToken>()));
            }

            public void VerifyResult()
            {
                Result.Should().BeOfType<OkObjectResult>()
                    .Which.Value.Should().BeOfType<GetApprenticeshipLockedFieldsQueryResult>()
                    .Which.Should().BeSameAs(QueryResult);
            }

            public void VerifyNotFoundResult()
            {
                Result.Should().BeOfType<NotFoundResult>();
            }
        }
    }
}
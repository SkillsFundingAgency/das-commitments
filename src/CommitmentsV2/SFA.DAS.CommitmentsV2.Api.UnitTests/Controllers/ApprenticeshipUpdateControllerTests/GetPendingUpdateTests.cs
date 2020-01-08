using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ApprenticeshipUpdateControllerTests
{
    [TestFixture]
    [Parallelizable]
    public class GetPendingUpdateTests
    {
        private GetPendingUpdateTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetPendingUpdateTestsFixture();
        }

        [Test]
        public async Task WhenGetPendingUpdateRequestReceived_ThenShouldPassApprenticeIdToQuery()
        {
            await _fixture.GetPendingUpdate();
            _fixture.VerifyApprenticeshipIdPassedToQuery();
        }

        [Test]
        public async Task WhenGetPendingUpdateRequestReceivedAndNoRecordsFound_ThenShouldReturnNotFoundResponse()
        {
            await _fixture.WithNoPendingUpdateFound().GetPendingUpdate();
            _fixture.VerifyNotFoundResult();
        }


        [Test]
        public async Task WhenGetPendingUpdateRequestReceived_ThenShouldReturnResponse()
        {
            await _fixture.GetPendingUpdate();
            _fixture.VerifyResult();
        }

        private class GetPendingUpdateTestsFixture
        {
            public IFixture AutoFixture { get; }
            public Mock<IMediator> Mediator { get; }
            public Mock<IModelMapper> ModelMapper { get; }
            public ApprenticeshipUpdateController Controller { get; }
            public long ApprenticeshipId { get; }
            public GetApprenticeshipUpdateQueryResult QueryResult { get; }
            public GetApprenticeshipUpdateResponse MapperResult { get; }
            public IActionResult Result { get; private set; }

            public GetPendingUpdateTestsFixture()
            {
                AutoFixture = new Fixture();

                QueryResult = AutoFixture.Create<GetApprenticeshipUpdateQueryResult>();

                Mediator = new Mock<IMediator>();
                Mediator.Setup(x => x.Send(It.IsAny<GetApprenticeshipUpdateQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(QueryResult);

                MapperResult = AutoFixture.Create<GetApprenticeshipUpdateResponse>();

                ModelMapper = new Mock<IModelMapper>();
                ModelMapper.Setup(x => x.Map<GetApprenticeshipUpdateResponse>(It.Is<GetApprenticeshipUpdateQueryResult>(r => r == QueryResult)))
                    .ReturnsAsync(MapperResult);

                ApprenticeshipId = AutoFixture.Create<long>();

                Controller = new ApprenticeshipUpdateController(Mediator.Object, ModelMapper.Object);
            }

            public GetPendingUpdateTestsFixture WithNoPendingUpdateFound()
            {
                Mediator.Setup(x => x.Send(It.IsAny<GetApprenticeshipUpdateQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((GetApprenticeshipUpdateQueryResult)null);

                return this;
            }


            public async Task GetPendingUpdate()
            {
                Result = await Controller.GetPendingUpdate(ApprenticeshipId);
            }

            public void VerifyApprenticeshipIdPassedToQuery()
            {
                Mediator.Verify(x => x.Send(It.Is<GetApprenticeshipUpdateQuery>(p => p.ApprenticeshipId == ApprenticeshipId), It.IsAny<CancellationToken>()));
            }

            public void VerifyResult()
            {
                Assert.IsInstanceOf<OkObjectResult>(Result);
                var resultObject = (OkObjectResult) Result;
                Assert.IsInstanceOf<GetApprenticeshipUpdateResponse>(resultObject.Value);
                Assert.AreSame(MapperResult, resultObject.Value);
            }
            public void VerifyNotFoundResult()
            {
                Assert.IsInstanceOf<NotFoundResult>(Result);
            }
        }
    }
}

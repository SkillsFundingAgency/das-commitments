using System.Collections.Generic;
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
        public async Task WhenGetUpdateRequestReceived_ThenShouldPassApprenticeIdAndStatusToQuery()
        {
            await _fixture.GetApprenticeshipUpdates();
            _fixture.VerifyApprenticeshipIdAndStatusPassedToQuery();
        }

        [Test]
        public async Task WhenGetPendingUpdateRequestReceivedAndNoRecordsFound_ThenShouldReturnAnEmptyArray()
        {
            await _fixture.WithNoPendingUpdateFound().GetApprenticeshipUpdates();
            _fixture.VerifyNotFoundResult();
        }

        [Test]
        public async Task WhenGetPendingUpdateRequestReceived_ThenShouldReturnResponse()
        {
            await _fixture.GetApprenticeshipUpdates();
            _fixture.VerifyResult();
        }

        private class GetPendingUpdateTestsFixture
        {
            public IFixture AutoFixture { get; }
            public Mock<IMediator> Mediator { get; }
            public Mock<IModelMapper> ModelMapper { get; }
            public ApprenticeshipUpdateController Controller { get; }
            public long ApprenticeshipId { get; }
            public CommitmentsV2.Types.ApprenticeshipUpdateStatus Status { get; }
            public GetApprenticeshipUpdateQueryResult QueryResult { get; internal set; }
            public GetApprenticeshipUpdatesResponse MapperResult { get; internal set; }
            public IActionResult Result { get; private set; }

            public GetPendingUpdateTestsFixture()
            {
                AutoFixture = new Fixture();

                QueryResult = AutoFixture.Create<GetApprenticeshipUpdateQueryResult>();

                Mediator = new Mock<IMediator>();
                Mediator.Setup(x => x.Send(It.IsAny<GetApprenticeshipUpdateQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(QueryResult);

                MapperResult = AutoFixture.Create<GetApprenticeshipUpdatesResponse>();

                ModelMapper = new Mock<IModelMapper>();
                ModelMapper.Setup(x => x.Map<GetApprenticeshipUpdatesResponse>(It.Is<GetApprenticeshipUpdateQueryResult>(r => r == QueryResult)))
                    .ReturnsAsync(MapperResult);

                ApprenticeshipId = AutoFixture.Create<long>();
                Status = CommitmentsV2.Types.ApprenticeshipUpdateStatus.Pending;
                Controller = new ApprenticeshipUpdateController(Mediator.Object, ModelMapper.Object);
            }

            public GetPendingUpdateTestsFixture WithNoPendingUpdateFound()
            {
                QueryResult = new GetApprenticeshipUpdateQueryResult { ApprenticeshipUpdates = new List<GetApprenticeshipUpdateQueryResult.ApprenticeshipUpdate>() };
                Mediator.Setup(x => x.Send(It.IsAny<GetApprenticeshipUpdateQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(QueryResult);

                MapperResult = new GetApprenticeshipUpdatesResponse { ApprenticeshipUpdates = new List<GetApprenticeshipUpdatesResponse.ApprenticeshipUpdate>() };
                ModelMapper.Setup(x => x.Map<GetApprenticeshipUpdatesResponse>(It.Is<GetApprenticeshipUpdateQueryResult>(r => r == QueryResult)))
                   .ReturnsAsync(MapperResult);

                return this;
            }

            public async Task GetApprenticeshipUpdates()
            {
                Result = await Controller.GetApprenticeshipUpdates(ApprenticeshipId, Status);
            }

            public void VerifyApprenticeshipIdAndStatusPassedToQuery()
            {
                Mediator.Verify(x => x.Send(It.Is<GetApprenticeshipUpdateQuery>(p => p.ApprenticeshipId == ApprenticeshipId && p.Status == Status ), It.IsAny<CancellationToken>()));
            }

            public void VerifyResult()
            {
                Assert.IsInstanceOf<OkObjectResult>(Result);
                var resultObject = (OkObjectResult)Result;
                Assert.IsInstanceOf<GetApprenticeshipUpdatesResponse>(resultObject.Value);
                Assert.AreSame(MapperResult, resultObject.Value);
            }

            public void VerifyNotFoundResult()
            {
                Assert.IsNotNull(Result);
                var model = Result.VerifyReturnsModel().WithModel<GetApprenticeshipUpdatesResponse>();
                Assert.AreEqual(0, model.ApprenticeshipUpdates.Count);
            }
        }
    }
}






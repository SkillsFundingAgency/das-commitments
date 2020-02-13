using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDataLocks;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ApprenticeshipControllerTests
{
    [TestFixture]
    [Parallelizable]
    public class GetDataLocksTests
    {
        private GetDataLocksTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetDataLocksTestsFixture();
        }

        [Test]
        public async Task WhenGetRequestReceived_ThenShouldReturnResponse()
        {
            await _fixture.GetDataLocks();
            _fixture.VerifyResult();
        }

        private class GetDataLocksTestsFixture
        {
            public IFixture AutoFixture { get; }
            public Mock<IMediator> Mediator { get; }
            public Mock<IModelMapper> ModelMapper { get; }
            public ApprenticeshipController Controller { get; }
            public long ApprenticeshipId { get; }
            public GetDataLocksQueryResult QueryResult { get; }
            public GetDataLocksResponse MapperResult { get; }
            public IActionResult Result { get; private set; }

            public GetDataLocksTestsFixture()
            {
                AutoFixture = new Fixture();

                QueryResult = AutoFixture.Create<GetDataLocksQueryResult>();

                Mediator = new Mock<IMediator>();
                Mediator.Setup(x => x.Send(It.IsAny<GetDataLocksQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(QueryResult);

                MapperResult = AutoFixture.Create<GetDataLocksResponse>();

                ModelMapper = new Mock<IModelMapper>();
                ModelMapper.Setup(x =>
                        x.Map<GetDataLocksResponse>(It.Is<GetDataLocksQueryResult>(r => r == QueryResult)))
                    .ReturnsAsync(MapperResult);

                ApprenticeshipId = AutoFixture.Create<long>();

                Controller = new ApprenticeshipController(Mediator.Object, ModelMapper.Object, Mock.Of<ILogger<ApprenticeshipController>>());
            }

            public async Task GetDataLocks()
            {
                Result = await Controller.GetDataLocks(ApprenticeshipId);
            }

            public void VerifyResult()
            {
                Assert.IsInstanceOf<OkObjectResult>(Result);
                var resultObject = (OkObjectResult)Result;
                Assert.IsInstanceOf<GetDataLocksResponse>(resultObject.Value);
                Assert.AreSame(MapperResult, resultObject.Value);
            }
        }
    }
}

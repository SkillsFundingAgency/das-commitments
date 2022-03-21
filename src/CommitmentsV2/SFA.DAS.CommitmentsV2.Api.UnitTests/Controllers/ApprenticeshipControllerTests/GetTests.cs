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
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ApprenticeshipControllerTests
{
    [TestFixture]
    [Parallelizable]
    public class GetTests
    {
        private GetTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetTestsFixture();
        }

        [Test]
        public async Task WhenGetRequestReceived_ThenShouldReturnResponse()
        {
            await _fixture.Get();
            _fixture.VerifyResult();
        }

        private class GetTestsFixture
        {
            public IFixture AutoFixture { get; }
            public Mock<IMediator> Mediator { get; }
            public Mock<IModelMapper> ModelMapper { get; }
            public ApprenticeshipController Controller { get; }
            public long ApprenticeshipId { get; }
            public GetApprenticeshipQueryResult QueryResult { get; }
            public GetApprenticeshipResponse MapperResult { get; }
            public IActionResult Result { get; private set; }

            public GetTestsFixture()
            {
                AutoFixture = new Fixture();

                QueryResult = AutoFixture
                    .Build<GetApprenticeshipQueryResult>()
                    .Create();

                Mediator = new Mock<IMediator>();
                Mediator.Setup(x => x.Send(It.IsAny<GetApprenticeshipQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(QueryResult);

                MapperResult = AutoFixture.Create<GetApprenticeshipResponse>();

                ModelMapper = new Mock<IModelMapper>();
                ModelMapper.Setup(x =>
                        x.Map<GetApprenticeshipResponse>(It.Is<GetApprenticeshipQueryResult>(r => r == QueryResult)))
                    .ReturnsAsync(MapperResult);

                ApprenticeshipId = AutoFixture.Create<long>();

                Controller = new ApprenticeshipController(Mediator.Object, ModelMapper.Object, Mock.Of<ILogger<ApprenticeshipController>>());
            }

            public async Task Get()
            {
                Result = await Controller.Get(ApprenticeshipId);
            }

            public void VerifyResult()
            {
                Assert.IsInstanceOf<OkObjectResult>(Result);
                var resultObject = (OkObjectResult)Result;
                Assert.IsInstanceOf<GetApprenticeshipResponse>(resultObject.Value);
                Assert.AreSame(MapperResult, resultObject.Value);
            }
        }
    }
}

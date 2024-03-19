using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.PriceEpisodeControllerTests
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
            public PriceEpisodeController Controller { get; }
            public long ApprenticeshipId { get; }
            public GetPriceEpisodesQueryResult QueryResult { get; }
            public GetPriceEpisodesResponse MapperResult { get; }
            public IActionResult Result { get; private set; }

            public GetTestsFixture()
            {
                AutoFixture = new Fixture();

                QueryResult = AutoFixture.Create<GetPriceEpisodesQueryResult>();

                Mediator = new Mock<IMediator>();
                Mediator.Setup(x => x.Send(It.IsAny<GetPriceEpisodesQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(QueryResult);

                MapperResult = AutoFixture.Create<GetPriceEpisodesResponse>();

                ModelMapper = new Mock<IModelMapper>();
                ModelMapper.Setup(x =>
                        x.Map<GetPriceEpisodesResponse>(It.Is<GetPriceEpisodesQueryResult>(r => r == QueryResult)))
                    .ReturnsAsync(MapperResult);

                ApprenticeshipId = AutoFixture.Create<long>();

                Controller = new PriceEpisodeController(Mediator.Object, ModelMapper.Object);
            }

            public async Task Get()
            {
                Result = await Controller.Get(ApprenticeshipId);
            }

            public void VerifyResult()
            {
                Assert.That(Result, Is.InstanceOf<OkObjectResult>());
                var resultObject = (OkObjectResult) Result;
                Assert.That(resultObject.Value, Is.InstanceOf<GetPriceEpisodesResponse>());
                Assert.That(resultObject.Value, Is.SameAs(MapperResult));
            }
        }
    }
}

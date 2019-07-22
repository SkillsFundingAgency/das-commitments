using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProvider;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    [TestFixture]
    [Parallelizable]
    public class ProviderControllerTests
    {
        private ProviderControllerTestsFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new ProviderControllerTestsFixture();
        }

        [Test]
        public async Task GetProvider_WhenProviderDoesExist_ThenShouldReturnProviderResponse()
        {
            var response = await _fixture.SetGetProviderQueryResult().GetProvider();
            var okObjectResult = response as OkObjectResult;
            var getProviderResponse = okObjectResult?.Value as GetProviderResponse;
            
            Assert.NotNull(response);
            Assert.IsNotNull(okObjectResult);
            Assert.IsNotNull(getProviderResponse);
            Assert.AreEqual(_fixture.GetProviderQueryResult.ProviderId, getProviderResponse.ProviderId);
            Assert.AreEqual(_fixture.GetProviderQueryResult.Name, getProviderResponse.Name);
        }

        [Test]
        public async Task GetProvider_WhenProviderDoesNotExist_ThenShouldReturnNotFoundResponse()
        {
            var response = await _fixture.GetProvider();
            var notFoundResult = response as NotFoundResult;
            
            Assert.NotNull(response);
            Assert.NotNull(notFoundResult);
        }
    }

    public class ProviderControllerTestsFixture
    {
        public Mock<IMediator> Mediator { get; set; }
        public ProviderController Controller { get; set; }
        public long ProviderId { get; set; }
        public string ProviderName { get; set; }
        public GetProviderQueryResult GetProviderQueryResult { get; set; }

        public ProviderControllerTestsFixture()
        {
            Mediator = new Mock<IMediator>();
            Controller = new ProviderController(Mediator.Object);
            ProviderId = 1;
            ProviderName = "Foo";
            GetProviderQueryResult = new GetProviderQueryResult(ProviderId, ProviderName);
        }

        public Task<IActionResult> GetProvider()
        {
            return Controller.GetProvider(ProviderId);
        }

        public ProviderControllerTestsFixture SetGetProviderQueryResult()
        {
            Mediator.Setup(m => m.Send(It.Is<GetProviderQuery>(q => q.ProviderId == ProviderId), CancellationToken.None))
                .ReturnsAsync(GetProviderQueryResult);
            
            return this;
        }
    }
}
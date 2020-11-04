using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllProviders;
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
        public async Task GetAllProviders_ThenShouldReturnListOfProviders()
        {
            var response = await _fixture.SetGetAllProvidersQueryResult().GetAllProviders();
            var okObjectResult = response as OkObjectResult;
            var getAllProvidersResponse = okObjectResult?.Value as GetAllProvidersResponse;

            Assert.IsNotNull(response);
            Assert.IsNotNull(okObjectResult);
            Assert.IsNotNull(getAllProvidersResponse);

            Assert.AreEqual(_fixture.GetAllProvidersQueryResult.Providers.Count, getAllProvidersResponse.Providers.Count);
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
        public GetAllProvidersQueryResult GetAllProvidersQueryResult { get; set; }

        public ProviderControllerTestsFixture()
        {
            Mediator = new Mock<IMediator>();
            Controller = new ProviderController(Mediator.Object);
            ProviderId = 1;
            ProviderName = "Foo";
            GetProviderQueryResult = new GetProviderQueryResult(ProviderId, ProviderName);
            GetAllProvidersQueryResult = GetAllProvidersResult();
        }
        public Task<IActionResult> GetAllProviders()
        {
            return Controller.GetAllProviders();
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

        public ProviderControllerTestsFixture SetGetAllProvidersQueryResult()
        {
            Mediator.Setup(m => m.Send(It.IsAny<GetAllProvidersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetAllProvidersQueryResult);

            return this;
        }

        private GetAllProvidersQueryResult GetAllProvidersResult()
        {
            return new GetAllProvidersQueryResult(
                new List<Provider>
                {
                    new Provider { Ukprn = 10000001, Name = "Provider 1" },
                    new Provider { Ukprn = 10000002, Name = "Provider 2" },
                    new Provider { Ukprn = 10000003, Name = "Provider 3" }
                });
        }
    }
}
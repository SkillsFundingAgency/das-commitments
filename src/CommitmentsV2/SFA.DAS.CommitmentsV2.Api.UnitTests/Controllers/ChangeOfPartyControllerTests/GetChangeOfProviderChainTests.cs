using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ChangeOfPartyControllerTests
{
    [TestFixture]
    public class GetChangeOfProviderChainTests
    {
        private GetChangeOfProviderChainTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetChangeOfProviderChainTestsFixture();
        }

        [Test]
        public async Task GetChangeOfProviderChain()
        {
            await _fixture.GetChangeOfProviderChain();
            _fixture.VerifyResult();
        }

        private class GetChangeOfProviderChainTestsFixture
        {
            private readonly Mock<IMediator> _mediator;
            private readonly Mock<IModelMapper> _mapper;
            private readonly ChangeOfPartyController _controller;
            private readonly GetChangeOfProviderChainQueryResult _queryResult;
            private readonly GetChangeOfProviderChainResponse _mapperResult;

            private IActionResult _result;

            private readonly Fixture _autoFixture;
            private readonly long _apprenticeshipId;

            public GetChangeOfProviderChainTestsFixture()
            {
                _mediator = new Mock<IMediator>();
                _mapper = new Mock<IModelMapper>();

                _queryResult = new GetChangeOfProviderChainQueryResult();
                _mapperResult = new GetChangeOfProviderChainResponse();

                _autoFixture = new Fixture();

                _mediator.Setup(x =>
                        x.Send(It.Is<GetChangeOfProviderChainQuery>(q => q.ApprenticeshipId == _apprenticeshipId),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_queryResult);

                _mapper.Setup(x => x.Map<GetChangeOfProviderChainResponse>(_queryResult)).ReturnsAsync(_mapperResult);

                _apprenticeshipId = _autoFixture.Create<long>();

                _controller = new ChangeOfPartyController(_mediator.Object, _mapper.Object);
            }

            public async Task GetChangeOfProviderChain()
            {
                _result = await _controller.GetChangeOfProviderChain(_apprenticeshipId);
            }

            public void VerifyResult()
            {
                Assert.That(_result, Is.Not.Null);
                var okObject = _result as OkObjectResult;
                Assert.That(okObject, Is.Not.Null);
                var objectValue = okObject.Value as GetChangeOfProviderChainResponse;
                Assert.That(objectValue, Is.Not.Null);

                Assert.That(objectValue, Is.EqualTo(_mapperResult));
            }
        }
    }
}

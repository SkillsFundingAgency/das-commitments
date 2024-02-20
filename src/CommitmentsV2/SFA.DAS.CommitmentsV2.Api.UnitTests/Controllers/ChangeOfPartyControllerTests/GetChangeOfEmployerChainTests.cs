using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfEmployerChain;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ChangeOfPartyControllerTests
{
    [TestFixture]
    public class GetChangeOfEmployerChainTests
    {
        private GetChangeOfEmployerChainTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetChangeOfEmployerChainTestsFixture();
        }

        [Test]
        public async Task GetChangeOfEmployerChain()
        {
            await _fixture.GetChangeOfEmployerChain();
            _fixture.VerifyResult();
        }

        private class GetChangeOfEmployerChainTestsFixture
        {
            private readonly Mock<IMediator> _mediator;
            private readonly Mock<IModelMapper> _mapper;
            private readonly ChangeOfPartyController _controller;
            private readonly GetChangeOfEmployerChainQueryResult _queryResult;
            private readonly GetChangeOfEmployerChainResponse _mapperResult;

            private IActionResult _result;

            private readonly Fixture _autoFixture;
            private readonly long _apprenticeshipId;

            public GetChangeOfEmployerChainTestsFixture()
            {
                _mediator = new Mock<IMediator>();
                _mapper = new Mock<IModelMapper>();

                _queryResult = new GetChangeOfEmployerChainQueryResult();
                _mapperResult = new GetChangeOfEmployerChainResponse();

                _autoFixture = new Fixture();

                _mediator.Setup(x =>
                        x.Send(It.Is<GetChangeOfEmployerChainQuery>(q => q.ApprenticeshipId == _apprenticeshipId),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_queryResult);

                _mapper.Setup(x => x.Map<GetChangeOfEmployerChainResponse>(_queryResult)).ReturnsAsync(_mapperResult);

                _apprenticeshipId = _autoFixture.Create<long>();

                _controller = new ChangeOfPartyController(_mediator.Object, _mapper.Object);
            }

            public async Task GetChangeOfEmployerChain()
            {
                _result = await _controller.GetChangeOfEmployerChain(_apprenticeshipId);
            }

            public void VerifyResult()
            {
                Assert.That(_result, Is.Not.Null);
                var okObject = _result as OkObjectResult;
                Assert.That(okObject, Is.Not.Null);
                var objectValue = okObject.Value as GetChangeOfEmployerChainResponse;
                Assert.That(objectValue, Is.Not.Null);

                Assert.That(objectValue, Is.EqualTo(_mapperResult));
            }
        }
    }
}

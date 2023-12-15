using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequests;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ChangeOfPartyControllerTests
{
    [TestFixture]
    public class GetAllTests
    {
        private GetAllTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetAllTestsFixture();
        }

        [Test]
        public async Task GetAll()
        {
            await _fixture.GetAll();
            _fixture.VerifyResult();
        }

        private class GetAllTestsFixture
        {
            private readonly Mock<IMediator> _mediator;
            private readonly Mock<IModelMapper> _mapper;
            private readonly ChangeOfPartyController _controller;
            private readonly GetChangeOfPartyRequestsQueryResult _queryResult;
            private readonly GetChangeOfPartyRequestsResponse _mapperResult;

            private IActionResult _result;

            private readonly Fixture _autoFixture;
            private readonly long _apprenticeshipId;

            public GetAllTestsFixture()
            {
                _mediator = new Mock<IMediator>();
                _mapper = new Mock<IModelMapper>();

                _queryResult = new GetChangeOfPartyRequestsQueryResult();
                _mapperResult = new GetChangeOfPartyRequestsResponse();

                _autoFixture = new Fixture();

                _mediator.Setup(x =>
                        x.Send(It.Is<GetChangeOfPartyRequestsQuery>(q => q.ApprenticeshipId == _apprenticeshipId),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_queryResult);

                _mapper.Setup(x => x.Map<GetChangeOfPartyRequestsResponse>(_queryResult)).ReturnsAsync(_mapperResult);

                _apprenticeshipId = _autoFixture.Create<long>();

                _controller = new ChangeOfPartyController(_mediator.Object, _mapper.Object);
            }

            public async Task GetAll()
            {
                _result = await _controller.GetAll(_apprenticeshipId);
            }

            public void VerifyResult()
            {
                Assert.IsNotNull(_result);
                var okObject = _result as OkObjectResult;
                Assert.IsNotNull(okObject);
                var objectValue = okObject.Value as GetChangeOfPartyRequestsResponse;
                Assert.IsNotNull(objectValue);

                Assert.That(objectValue, Is.EqualTo(_mapperResult));
            }
        }
    }
}

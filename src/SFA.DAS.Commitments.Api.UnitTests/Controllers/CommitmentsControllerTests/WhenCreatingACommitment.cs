using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using Moq;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using FluentAssertions;
using System.Web.Http.Results;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Application.Exceptions;
using System;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.CommitmentsControllerTests
{
    [TestFixture]
    public class WhenCreatingACommitment
    {
        private CommitmentsController _controller;
        private Mock<IMediator> _mockMediator;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new CommitmentsController(_mockMediator.Object);
        }

        [Test]
        public async Task ThenACreateResponseCodeIsReturnedOnSuccess()
        {
            var result = await _controller.Create(new Commitment());

            result.Should().BeOfType<CreatedAtRouteNegotiatedContentResult<Commitment>>();
        }

        [Test]
        public async Task ThenTheLocationHeaderIsSetInTheResponseOnSuccessfulCreate()
        {
            var result = await _controller.Create(new Commitment()) as CreatedAtRouteNegotiatedContentResult<Commitment>;

            result.RouteName.Should().Be("DefaultApi");
            result.RouteValues["id"].Should().Be(3);
        }

        [Test]
        public async Task ThenTheMediatorIsCalled()
        {
            var result = await _controller.Create(new Commitment());

            _mockMediator.Verify(x => x.SendAsync(It.IsAny<CreateCommitmentCommand>()));
        }

        [Test]
        public async Task ThenABadResponseIsReturnedWhenAnInvalidRequestExceptionThrown()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<CreateCommitmentCommand>())).Throws<InvalidRequestException>();

            var result = await _controller.Create(new Commitment());

            result.Should().BeOfType<BadRequestResult>();
        }

    }
}

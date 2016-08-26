using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using Moq;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using FluentAssertions;
using System.Web.Http.Results;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.CommitmentsControllerTests
{
    [TestFixture]
    public class WhenCreatingACommitment
    {
        [Test]
        public async Task ThenACreateResponseCodeIsReturnedOnSuccess()
        {
            var mockMediator = new Mock<IMediator>();
            var controller = new CommitmentsController(mockMediator.Object);

            var result = await controller.Create(new Commitment());

            result.Should().BeOfType<CreatedAtRouteNegotiatedContentResult<Commitment>>();
        }

        [Test]
        public async Task ThenTheLocationHeaderIsSetInTheResponseOnSuccessfulCreate()
        {
            var mockMediator = new Mock<IMediator>();
            var controller = new CommitmentsController(mockMediator.Object);

            var result = await controller.Create(new Commitment()) as CreatedAtRouteNegotiatedContentResult<Commitment>;

            result.RouteName.Should().Be("DefaultApi");
            result.RouteValues["id"].Should().Be(3);
        }

        [Test]
        public async Task ThenTheMediatorIsCalled()
        {
            var mockMediator = new Mock<IMediator>();
            var controller = new CommitmentsController(mockMediator.Object);

            var result = await controller.Create(new Commitment());

            mockMediator.Verify(x => x.SendAsync(It.IsAny<CreateCommitmentCommand>()));
        }
    }
}

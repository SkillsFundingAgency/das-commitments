using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Results;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Application.Queries.GetEmployerCommitments;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Api.UnitTests.CommitmentsControllerTests
{
    [TestFixture]
    public class WhenIGetAllEmployerCommitments
    {
        [Test]
        public async Task ThenAListOfCommitmentsWillBeReturned()
        {
            var autoDataFixture = new Fixture();
            var mediatorResponse = autoDataFixture.Build<GetEmployerCommitmentsResponse>().With(x => x.HasError, false).Create();

            Mock<IMediator> mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.SendAsync(It.IsAny<GetEmployerCommitmentsRequest>())).Returns(Task.FromResult(mediatorResponse));
            var controller = new CommitmentsController(mockMediator.Object);

            var result = await controller.Get(1235L) as OkNegotiatedContentResult<IList<Commitment>>;

            result.Should().NotBeNull();
            result.Content.Should().BeSameAs(mediatorResponse.Commitments);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledWithTheProviderId()
        {
            const long testAccountId = 1235L;
            Mock<IMediator> mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.SendAsync(It.IsAny<GetEmployerCommitmentsRequest>())).Returns(Task.FromResult(new GetEmployerCommitmentsResponse()));

            var controller = new CommitmentsController(mockMediator.Object);

            var result = await controller.Get(testAccountId);

            mockMediator.Verify(x => x.SendAsync(It.Is<GetEmployerCommitmentsRequest>(arg => arg.AccountId == testAccountId)));
        }

        [Test]
        public async Task ThenShouldReturnA404StatusIfProviderIdIsInvalid()
        {
            Mock<IMediator> mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.SendAsync(It.IsAny<GetEmployerCommitmentsRequest>())).Returns(Task.FromResult(new GetEmployerCommitmentsResponse { HasError = true }));

            var controller = new CommitmentsController(mockMediator.Object);

            var result = await controller.Get(-1L);

            result.Should().BeOfType<BadRequestResult>();
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Results;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Application.Queries.GetProviderCommitments;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Api.UnitTests.CommitmentsControllerTests
{
    [TestFixture]
    public class WhenIGetAllProviderCommitments
    {
        [Test]
        public async Task ThenAListOfCommitmentsWillBeReturned()
        {
            var autoDataFixture = new Fixture();
            var mediatorResponse = autoDataFixture.Build<GetProviderCommitmentsResponse>().With(x => x.HasError, false).Create();

            Mock<IMediator> mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.SendAsync(It.IsAny<GetProviderCommitmentsRequest>())).Returns(Task.FromResult(mediatorResponse));
            var controller = new CommitmentsController(mockMediator.Object);

            var result = await controller.Get(1234L) as OkNegotiatedContentResult<IList<Commitment>>;

            result.Should().NotBeNull();
            result.Content.Should().BeSameAs(mediatorResponse.Commitments);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledWithTheProviderId()
        {
            const long testProviderId = 1234L;
            Mock<IMediator> mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.SendAsync(It.IsAny<GetProviderCommitmentsRequest>())).Returns(Task.FromResult(new GetProviderCommitmentsResponse()));

            var controller = new CommitmentsController(mockMediator.Object);

            var result = await controller.Get(testProviderId);

            mockMediator.Verify(x => x.SendAsync(It.Is<GetProviderCommitmentsRequest>(arg => arg.ProviderId == testProviderId)));
        }

        [Test]
        public async Task ThenShouldReturnA404StatusIfProviderIdIsInvalid()
        {
            Mock<IMediator> mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.SendAsync(It.IsAny<GetProviderCommitmentsRequest>())).Returns(Task.FromResult(new GetProviderCommitmentsResponse { HasError = true }));

            var controller = new CommitmentsController(mockMediator.Object);

            var result = await controller.Get(0L);

            result.Should().BeOfType<BadRequestResult>();
        }
    }
}

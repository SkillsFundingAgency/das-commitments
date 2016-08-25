using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Results;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Queries.GetProviderCommitments;

namespace SFA.DAS.Commitments.Api.UnitTests.CommitmentsControllerTests
{
    [TestFixture]
    public class WhenIGetAllProviderCommitments
    {
        private Mock<IMediator> _mockMediator;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
        }

        [Test]
        public async Task ThenAListOfCommitmentsWillBeReturned()
        {
            var autoDataFixture = new Fixture();
            var mediatorResponse = autoDataFixture.Build<GetProviderCommitmentsResponse>().With(x => x.HasError, false).Create();
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetProviderCommitmentsRequest>())).Returns(Task.FromResult(mediatorResponse));
            var controller = new CommitmentsController(_mockMediator.Object);

            var result = await controller.Get(1234L) as OkNegotiatedContentResult<IList<CommitmentListItem>>;

            result.Should().NotBeNull();
            result.Content.Should().BeSameAs(mediatorResponse.Data);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledWithTheProviderId()
        {
            const long testProviderId = 1234L;
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetProviderCommitmentsRequest>())).Returns(Task.FromResult(new GetProviderCommitmentsResponse()));
            var controller = new CommitmentsController(_mockMediator.Object);

            var result = await controller.Get(testProviderId);

            _mockMediator.Verify(x => x.SendAsync(It.Is<GetProviderCommitmentsRequest>(arg => arg.ProviderId == testProviderId)));
        }

        [Test]
        public async Task ThenShouldReturnA404StatusIfProviderIdIsInvalid()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetProviderCommitmentsRequest>())).Returns(Task.FromResult(new GetProviderCommitmentsResponse { HasError = true }));
            var controller = new CommitmentsController(_mockMediator.Object);

            var result = await controller.Get(0L);

            result.Should().BeOfType<BadRequestResult>();
        }
    }
}

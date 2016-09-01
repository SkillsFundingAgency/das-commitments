using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using MediatR;
using SFA.DAS.Commitments.Api.Controllers;
using Ploeh.AutoFixture.NUnit3;
using SFA.DAS.Commitments.Application.Queries.GetEmployerCommitments;
using System.Web.Http.Results;
using SFA.DAS.Commitments.Api.Types;
using FluentAssertions;
using SFA.DAS.Commitments.Application.Exceptions;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.EmployerControllerTests
{
    [TestFixture]
    public class WhenIGetAllEmployerCommitments
    {
        private Mock<IMediator> _mockMediator;
        private EmployerController _controller;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new EmployerController(_mockMediator.Object);
        }

        [Test, AutoData]
        public async Task ThenAListOfCommitmentsWillBeReturned(GetEmployerCommitmentsResponse mediatorResponse)
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetEmployerCommitmentsRequest>())).ReturnsAsync(mediatorResponse);

            var result = await _controller.GetCommitments(1234L) as OkNegotiatedContentResult<IList<CommitmentListItem>>;

            result.Should().NotBeNull();
            result.Content.Should().BeSameAs(mediatorResponse.Data);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledWithTheProviderId()
        {
            const long testAccountId = 1234L;
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetEmployerCommitmentsRequest>())).ReturnsAsync(new GetEmployerCommitmentsResponse());

            var result = await _controller.GetCommitments(testAccountId);

            _mockMediator.Verify(x => x.SendAsync(It.Is<GetEmployerCommitmentsRequest>(arg => arg.AccountId == testAccountId)));
        }

        [Test]
        public async Task ThenShouldReturnBadRequestIfThrowsAnInvalidRequestException()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetEmployerCommitmentsRequest>())).Throws<InvalidRequestException>();

            var result = await _controller.GetCommitments(-1L);

            result.Should().BeOfType<BadRequestResult>();
        }
    }
}

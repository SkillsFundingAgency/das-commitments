using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Results;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Exceptions;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.ProviderControllerTests
{
    [TestFixture]
    public class WhenUpdatingAnApprenticeship
    {
        private const long TestProviderId = 1L;
        private const long TestCommitmentId = 2L;
        private const long TestApprenticeshipId = 3L;
        private ProviderController _controller;
        private Mock<IMediator> _mockMediator;
        private Apprenticeship _newApprenticeship;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new ProviderController(_mockMediator.Object);
            _newApprenticeship = new Apprenticeship
            {
                CommitmentId = TestCommitmentId,
                Id = TestApprenticeshipId
            };

        }

        [Test]
        public async Task ThenANoContentCodeIsReturnedOnSuccess()
        {
            var result = await _controller.PutApprenticeship(TestProviderId, _newApprenticeship);

            result.Should().BeOfType<StatusCodeResult>();

            (result as StatusCodeResult).StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledToCreateApprenticeship()
        {
            var result = await _controller.PutApprenticeship(TestProviderId, _newApprenticeship);

            _mockMediator.Verify(x => x.SendAsync(It.Is<UpdateApprenticeshipCommand>(a => a.ProviderId == TestProviderId && a.CommitmentId == TestCommitmentId && a.ApprenticeshipId == TestApprenticeshipId && a.Apprenticeship == _newApprenticeship)));
        }

        [Test]
        public async Task ThenABadResponseIsReturnedWhenAnInvalidRequestExceptionThrown()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<UpdateApprenticeshipCommand>())).Throws<InvalidRequestException>();

            var result = await _controller.PutApprenticeship(TestProviderId, _newApprenticeship);

            result.Should().BeOfType<BadRequestResult>();
        }
    }
}

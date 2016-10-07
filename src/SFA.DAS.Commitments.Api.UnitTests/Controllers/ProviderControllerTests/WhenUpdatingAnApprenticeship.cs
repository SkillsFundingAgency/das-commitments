using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Results;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;

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
        private ProviderOrchestrator _providerOrchestrator;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _providerOrchestrator = new ProviderOrchestrator(_mockMediator.Object);
            _controller = new ProviderController(_providerOrchestrator);

            _newApprenticeship = new Apprenticeship
            {
                CommitmentId = TestCommitmentId,
                Id = TestApprenticeshipId
            };
        }

        [Test]
        public async Task ThenANoContentCodeIsReturnedOnSuccess()
        {
            var result = await _controller.PutApprenticeship(TestProviderId, TestCommitmentId, TestApprenticeshipId, _newApprenticeship);

            result.Should().BeOfType<StatusCodeResult>();

            (result as StatusCodeResult).StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledToCreateApprenticeship()
        {
            var result = await _controller.PutApprenticeship(TestProviderId, TestCommitmentId, TestApprenticeshipId, _newApprenticeship);

            _mockMediator.Verify(x => x.SendAsync(It.Is<UpdateApprenticeshipCommand>(a => a.Caller.CallerType == CallerType.Provider && a.Caller.Id == TestProviderId && a.CommitmentId == TestCommitmentId && a.ApprenticeshipId == TestApprenticeshipId && a.Apprenticeship == _newApprenticeship)));
        }

        [Test]
        public void ThenABadResponseIsReturnedWhenAnInvalidRequestExceptionThrown()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<UpdateApprenticeshipCommand>())).ThrowsAsync(new ValidationException(""));

            Assert.ThrowsAsync<ValidationException>(async () => await _controller.PutApprenticeship(TestProviderId, TestCommitmentId, TestApprenticeshipId, _newApprenticeship));
        }
    }
}

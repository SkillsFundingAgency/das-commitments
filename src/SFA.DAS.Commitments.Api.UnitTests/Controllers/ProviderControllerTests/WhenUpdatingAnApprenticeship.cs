using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web.Http.Results;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Interfaces;
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
        private Apprenticeship.Apprenticeship _newApprenticeship;
        private ProviderOrchestrator _providerOrchestrator;

        private Apprenticeship.ApprenticeshipRequest _newApprenticeshipRequest;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _providerOrchestrator = new ProviderOrchestrator(_mockMediator.Object, Mock.Of<ICommitmentsLogger>());
            _controller = new ProviderController(_providerOrchestrator);

            _newApprenticeship = new Apprenticeship.Apprenticeship
            {
                CommitmentId = TestCommitmentId,
                Id = TestApprenticeshipId
            };
            _newApprenticeshipRequest = new Apprenticeship.ApprenticeshipRequest
            {
                Apprenticeship = _newApprenticeship,
                LastUpdatedByInfo = new LastUpdateInfo { EmailAddress = "test@Email.com", Name = "Bob" }
            };
        }

        [Test]
        public async Task ThenANoContentCodeIsReturnedOnSuccess()
        {
            var result = await 
                _controller.PutApprenticeship(TestProviderId, TestCommitmentId, TestApprenticeshipId, _newApprenticeshipRequest);

            result.Should().BeOfType<StatusCodeResult>();

            (result as StatusCodeResult).StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledToCreateApprenticeship()
        {
            await _controller.PutApprenticeship(TestProviderId, TestCommitmentId, TestApprenticeshipId, _newApprenticeshipRequest);

            _mockMediator.Verify(
                x =>
                    x.SendAsync(
                        It.Is<UpdateApprenticeshipCommand>(
                            a =>
                                a.Caller.CallerType == CallerType.Provider && a.Caller.Id == TestProviderId && a.CommitmentId == TestCommitmentId && a.ApprenticeshipId == TestApprenticeshipId &&
                                a.Apprenticeship == _newApprenticeship && a.UserName == _newApprenticeshipRequest.LastUpdatedByInfo.Name)));
        }

        [Test]
        public void ThenABadResponseIsReturnedWhenAnInvalidRequestExceptionThrown()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<UpdateApprenticeshipCommand>())).ThrowsAsync(new ValidationException(""));

            Assert.ThrowsAsync<ValidationException>(async () => 
                await _controller.PutApprenticeship(TestProviderId, TestCommitmentId, TestApprenticeshipId, _newApprenticeshipRequest));
        }
    }
}

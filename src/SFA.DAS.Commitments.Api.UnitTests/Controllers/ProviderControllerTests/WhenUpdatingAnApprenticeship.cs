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
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Interfaces;
using ApiApprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;

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
        private Domain.Entities.Apprenticeship _newApprenticeship;
        private ProviderOrchestrator _providerOrchestrator;

        private ApiApprenticeship.ApprenticeshipRequest _newApprenticeshipRequest;
        private ApprenticeshipsOrchestrator _apprenticeshipsOrchestrator;
        private Mock<IApprenticeshipMapper> _mockApprenticeshipMapper;
        private Mock<FacetMapper> _mockFacetMapper;


        [SetUp]
        public void Setup()
        {
            _mockFacetMapper = new Mock<FacetMapper>(Mock.Of<ICurrentDateTime>());

            _mockMediator = new Mock<IMediator>();
            _mockApprenticeshipMapper = new Mock<IApprenticeshipMapper>();
            _providerOrchestrator = new ProviderOrchestrator(
                _mockMediator.Object, 
                Mock.Of<ICommitmentsLogger>(),
                _mockFacetMapper.Object,
                new ApprenticeshipFilterService(_mockFacetMapper.Object),
                _mockApprenticeshipMapper.Object,
                Mock.Of<ICommitmentMapper>());

            _apprenticeshipsOrchestrator = new ApprenticeshipsOrchestrator(_mockMediator.Object, Mock.Of<IDataLockMapper>(), Mock.Of<IApprenticeshipMapper>(), Mock.Of<ICommitmentsLogger>());
            _controller = new ProviderController(_providerOrchestrator, _apprenticeshipsOrchestrator);

            _newApprenticeship = new Domain.Entities.Apprenticeship
            {
                CommitmentId = TestCommitmentId,
                Id = TestApprenticeshipId
            };

            _newApprenticeshipRequest = new ApiApprenticeship.ApprenticeshipRequest
            {
                Apprenticeship = new ApiApprenticeship.Apprenticeship(),
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
            
            UpdateApprenticeshipCommand command = new UpdateApprenticeshipCommand();
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<UpdateApprenticeshipCommand>()))
                .ReturnsAsync(new Unit())
                .Callback((UpdateApprenticeshipCommand c) => command = c);

            _mockApprenticeshipMapper.Setup(
                m => m.Map(It.IsAny<ApiApprenticeship.Apprenticeship>(), CallerType.Provider))
                .Returns(_newApprenticeship);

            await _controller.PutApprenticeship(TestProviderId, TestCommitmentId, TestApprenticeshipId, _newApprenticeshipRequest);

            command.Caller.CallerType.Should().Be(CallerType.Provider);
            command.Caller.Id.Should().Be(TestProviderId);
            command.CommitmentId.Should().Be(TestCommitmentId);
            command.ApprenticeshipId.Should().Be(TestApprenticeshipId);

            command.ApprenticeshipId.Should().Be(TestApprenticeshipId);
            command.Apprenticeship.Should().Be(_newApprenticeship);
            command.UserName.Should().Be(_newApprenticeshipRequest.LastUpdatedByInfo.Name);
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

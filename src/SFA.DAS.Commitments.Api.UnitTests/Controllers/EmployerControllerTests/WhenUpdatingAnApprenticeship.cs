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
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.EmployerControllerTests
{
    [TestFixture]
    public class WhenUpdatingAnApprenticeship
    {
        private const long TestAccountId = 1L;
        private const long TestCommitmentId = 2L;
        private const long TestApprenticeshipId = 3L;
        private EmployerController _controller;
        private Mock<IMediator> _mockMediator;
        private EmployerOrchestrator _employerOrchestrator;
        private ApprenticeshipsOrchestrator _apprenticeshipOrchestor;
        private Mock<IApprenticeshipMapper> _apprenticeshipMapper;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _apprenticeshipMapper = new Mock<IApprenticeshipMapper>();
            _employerOrchestrator = new EmployerOrchestrator(_mockMediator.Object, Mock.Of<ICommitmentsLogger>(), new FacetMapper(Mock.Of<ICurrentDateTime>()), new ApprenticeshipFilterService(new FacetMapper(Mock.Of<ICurrentDateTime>())), _apprenticeshipMapper.Object, Mock.Of<ICommitmentMapper>());
            _apprenticeshipOrchestor = new ApprenticeshipsOrchestrator(_mockMediator.Object, Mock.Of<IDataLockMapper>(), _apprenticeshipMapper.Object, Mock.Of<ICommitmentsLogger>());

            _controller = new EmployerController(_employerOrchestrator, _apprenticeshipOrchestor);
        }

        [Test]
        public async Task ThenANoContentCodeIsReturnedOnSuccess()
        {
            var result = 
                await _controller.PutApprenticeship(TestAccountId, TestCommitmentId, TestApprenticeshipId, 
                    new Apprenticeship.ApprenticeshipRequest { Apprenticeship = new Apprenticeship.Apprenticeship() });

            result.Should().BeOfType<StatusCodeResult>();

            (result as StatusCodeResult).StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledToCreateApprenticeship()
        {
            var newApprenticeship = new Apprenticeship.ApprenticeshipRequest
            {
                Apprenticeship = new Apprenticeship.Apprenticeship(),
                LastUpdatedByInfo = new LastUpdateInfo { EmailAddress = "test@email.com", Name = "Bob" }
            };

            var expectedApprenticeship = new Domain.Entities.Apprenticeship();

            _apprenticeshipMapper.Setup(m => m.Map(newApprenticeship.Apprenticeship, CallerType.Employer))
                .Returns(expectedApprenticeship);

            var result = await _controller.PutApprenticeship(TestAccountId, TestCommitmentId, TestApprenticeshipId, newApprenticeship);

            _mockMediator.Verify(x => x.SendAsync(
                It.Is<UpdateApprenticeshipCommand>(
                    a =>
                        a.Caller.CallerType == CallerType.Employer && a.Caller.Id == TestAccountId && a.CommitmentId == TestCommitmentId && a.ApprenticeshipId == TestApprenticeshipId &&
                        a.Apprenticeship == expectedApprenticeship && a.UserName == newApprenticeship.LastUpdatedByInfo.Name)));
        }

        [Test]
        public void ThenABadResponseIsReturnedWhenAnInvalidRequestExceptionThrown()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<UpdateApprenticeshipCommand>())).ThrowsAsync(new ValidationException(""));

            Assert.ThrowsAsync<ValidationException>(async () => await _controller.PutApprenticeship(TestAccountId, TestCommitmentId, TestApprenticeshipId, new Apprenticeship.ApprenticeshipRequest { Apprenticeship = new Apprenticeship.Apprenticeship() }));
        }
    }
}

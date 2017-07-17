using System;
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
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Interfaces;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.EmployerControllerTests
{
    [TestFixture]
    public class WhenCreatingAnApprenticeship
    {
        private const long TestAccountId = 1L;
        private const long TestCommitmentId = 2L;
        private EmployerController _controller;
        private Mock<IMediator> _mockMediator;
        private EmployerOrchestrator _employerOrchestrator;
        private ApprenticeshipsOrchestrator _apprenticeshipOrchestor;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _employerOrchestrator = new EmployerOrchestrator(_mockMediator.Object, Mock.Of<ICommitmentsLogger>(), new FacetMapper(), new ApprenticeshipFilterService(new FacetMapper()), Mock.Of<IApprenticeshipMapper>(), Mock.Of<ICommitmentMapper>());
            _apprenticeshipOrchestor = new ApprenticeshipsOrchestrator(_mockMediator.Object, Mock.Of<ICommitmentsLogger>());

            _controller = new EmployerController(_employerOrchestrator, _apprenticeshipOrchestor);
        }

        [Test]
        public async Task ThenACreateResponseCodeIsReturnedOnSuccess()
        {
            var result = await _controller.CreateApprenticeship(TestAccountId, TestCommitmentId, 
                new Apprenticeship.ApprenticeshipRequest { Apprenticeship = new Apprenticeship.Apprenticeship() });

            result.Should().BeOfType<CreatedAtRouteNegotiatedContentResult<Apprenticeship.Apprenticeship>>();
        }

        [Test]
        public async Task ThenTheLocationHeaderIsSetInTheResponseOnSuccessfulCreate()
        {
            const long CreatedApprenticeshipId = 10L;
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<CreateApprenticeshipCommand>())).ReturnsAsync(CreatedApprenticeshipId);
            var result = await _controller.CreateApprenticeship(TestAccountId, TestCommitmentId, 
                new Apprenticeship.ApprenticeshipRequest { Apprenticeship = new Apprenticeship.Apprenticeship() }) as CreatedAtRouteNegotiatedContentResult<Apprenticeship.Apprenticeship>;

            result.RouteName.Should().Be("GetApprenticeshipForEmployer");
            result.RouteValues["accountId"].Should().Be(TestAccountId);
            result.RouteValues["commitmentId"].Should().Be(TestCommitmentId);
            result.RouteValues["apprenticeshipId"].Should().Be(CreatedApprenticeshipId);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledToCreateApprenticeship()
        {
            var newApprenticeship = new Apprenticeship.ApprenticeshipRequest
            {
                Apprenticeship = new Apprenticeship.Apprenticeship(),
                LastUpdatedByInfo = new LastUpdateInfo { EmailAddress = "test@email.com", Name = "Bob" }
            };
            var result = await _controller.CreateApprenticeship(TestAccountId, TestCommitmentId, newApprenticeship);

            _mockMediator.Verify(
                x =>
                    x.SendAsync(
                        It.Is<CreateApprenticeshipCommand>(
                            a =>
                                a.Caller.CallerType == CallerType.Employer && a.Caller.Id == TestAccountId && a.CommitmentId == TestCommitmentId && a.Apprenticeship == newApprenticeship.Apprenticeship &&
                                a.UserName == newApprenticeship.LastUpdatedByInfo.Name)));
        }

        [Test]
        public void ThenABadResponseIsReturnedWhenAnInvalidRequestExceptionThrown()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<CreateApprenticeshipCommand>())).ThrowsAsync(new ValidationException(""));

            Func<Task> act = async () => 
            await _controller.CreateApprenticeship(TestAccountId, TestCommitmentId, 
                new Apprenticeship.ApprenticeshipRequest { Apprenticeship = new Apprenticeship.Apprenticeship() });

            act.ShouldThrow<ValidationException>();

        }
    }
}

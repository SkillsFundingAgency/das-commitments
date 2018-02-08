using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Results;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.EmployerControllerTests
{
    [TestFixture]
    public class WhenPuttingApprenticeshipStopDate
    {
        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();

            _employerOrchestrator = new EmployerOrchestrator(_mockMediator.Object, Mock.Of<ICommitmentsLogger>(),
                new FacetMapper(Mock.Of<ICurrentDateTime>()),
                new ApprenticeshipFilterService(new FacetMapper(Mock.Of<ICurrentDateTime>())),
                Mock.Of<IApprenticeshipMapper>(), Mock.Of<ICommitmentMapper>());

            _apprenticeshipOrchestor = new ApprenticeshipsOrchestrator(_mockMediator.Object, Mock.Of<IDataLockMapper>(),
                Mock.Of<IApprenticeshipMapper>(), Mock.Of<ICommitmentsLogger>());

            _controller = new EmployerController(_employerOrchestrator, _apprenticeshipOrchestor);
        }

        private const long TestProviderId = 1L;
        private const long TestApprenticeshipId = 3L;

        private const long TestCommitmentId = 4L;

        private EmployerController _controller;
        private Mock<IMediator> _mockMediator;
        private EmployerOrchestrator _employerOrchestrator;
        private ApprenticeshipsOrchestrator _apprenticeshipOrchestor;

        [Test]
        public async Task ThenANoContentCodeIsReturnedOnSuccess()
        {
            var result = await _controller.PutApprenticeshipStopDate(
                TestProviderId,
                TestApprenticeshipId,
                TestCommitmentId,
                new ApprenticeshipStopDate { NewStopDate = DateTime.Today });

            result.Should().BeOfType<StatusCodeResult>();

            (result as StatusCodeResult).StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
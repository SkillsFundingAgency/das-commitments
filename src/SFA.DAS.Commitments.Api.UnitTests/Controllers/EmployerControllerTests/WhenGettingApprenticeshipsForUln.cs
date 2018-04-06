using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Results;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Application.Queries.GetActiveApprenticeshipsByUln;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.HashingService;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship.Apprenticeship;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.EmployerControllerTests
{
    [TestFixture]
    public class WhenGettingApprenticeshipsForUln
    {
        private Mock<IMediator> _mockMediator;
        private EmployerOrchestrator _employerOrchestrator;
        private EmployerController _controller;
        private ApprenticeshipsOrchestrator _apprenticeshipsOrchestrator;
        private Mock<ICurrentDateTime> _currentDateTime;

        private const long TestAccountId = 1L;
        private const string TestUln = "6791776799";

        [SetUp]
        public void SetUp()
        {
            _currentDateTime = new Mock<ICurrentDateTime>();
            _currentDateTime.Setup(x => x.Now).Returns(new DateTime(2018, 1, 1));

            _mockMediator = new Mock<IMediator>();
            _mockMediator.Setup(m => m.SendAsync(It.IsAny<GetActiveApprenticeshipsByUlnRequest>()))
                .ReturnsAsync(new GetActiveApprenticeshipsByUlnResponse
                {
                    Data = new List<ApprenticeshipResult>()
                });

            _employerOrchestrator = new EmployerOrchestrator(_mockMediator.Object, Mock.Of<ICommitmentsLogger>(),
                new FacetMapper(_currentDateTime.Object),
                new ApprenticeshipFilterService(new FacetMapper(_currentDateTime.Object)), new ApprenticeshipMapper(),
                Mock.Of<ICommitmentMapper>(), Mock.Of<ITransferRequestMapper>(), Mock.Of<IHashingService>());

            _apprenticeshipsOrchestrator = new ApprenticeshipsOrchestrator(
                _mockMediator.Object,
                Mock.Of<IDataLockMapper>(),
                Mock.Of<IApprenticeshipMapper>(),
                Mock.Of<ICommitmentsLogger>());

            _controller = new EmployerController(_employerOrchestrator, _apprenticeshipsOrchestrator);
        }

        [Test]
        public async Task ShouldHandleEmptyResponse()
        {
            var result = await _controller.GetActiveApprenticeshipsForUln(TestAccountId, TestUln);
            var response = result as OkNegotiatedContentResult<IEnumerable<Apprenticeship>>;
            response.Should().NotBeNull();
            response.Content.Count().Should().Be(0);
        }

        [Test]
        public async Task ControllerTest()
        {
            _mockMediator.Setup(m => m.SendAsync(It.IsAny<GetActiveApprenticeshipsByUlnRequest>()))
                .ReturnsAsync(new GetActiveApprenticeshipsByUlnResponse
                {
                    Data = new List<ApprenticeshipResult>
                               {
                                   new ApprenticeshipResult { PaymentStatus = PaymentStatus.Active, StartDate = _currentDateTime.Object.Now.AddMonths(-2) },
                                   new ApprenticeshipResult { PaymentStatus = PaymentStatus.Active, StartDate = _currentDateTime.Object.Now.AddMonths(2) },
                                   new ApprenticeshipResult { PaymentStatus = PaymentStatus.Active }
                               }
                });

            var result = await _controller.GetActiveApprenticeshipsForUln(TestAccountId, TestUln);
            var response = result as OkNegotiatedContentResult<IEnumerable<Apprenticeship>>;

            response.Should().NotBeNull();
            response.Content.Count().Should().Be(3);
        }
    }
}

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
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeships;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.HashingService;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.EmployerControllerTests
{
    [TestFixture]
    public class WhenGettingAllApprenticeships
    {
        private Mock<IMediator> _mockMediator;
        private EmployerOrchestrator _employerOrchestrator;
        private EmployerController _controller;
        private ApprenticeshipsOrchestrator _apprenticeshipsOrchestrator;
        private Mock<ICurrentDateTime> _currentDateTime;

        [SetUp]
        public void SetUp()
        {
            _currentDateTime = new Mock<ICurrentDateTime>();
            _currentDateTime.Setup(x => x.Now).Returns(new DateTime(2018, 1, 1));

            _mockMediator = new Mock<IMediator>();
            _mockMediator.Setup(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()))
                .ReturnsAsync(new GetApprenticeshipsResponse
                                  {
                                      Apprenticeships = new List<Domain.Entities.Apprenticeship>()
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
            var result = await _controller.GetApprenticeships(1L, new ApprenticeshipSearchQuery()) as OkNegotiatedContentResult<ApprenticeshipSearchResponse>;
            result.Content.Apprenticeships.Count().Should().Be(0);
        }

        [Test]
        public async Task ControllerTest()
        {
            _mockMediator.Setup(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()))
                .ReturnsAsync(new GetApprenticeshipsResponse
                {
                    Apprenticeships = new List<Domain.Entities.Apprenticeship>
                               {
                                   new Domain.Entities.Apprenticeship { PaymentStatus = Domain.Entities.PaymentStatus.Active, StartDate = _currentDateTime.Object.Now.AddMonths(-2) },
                                   new Domain.Entities.Apprenticeship { PaymentStatus = Domain.Entities.PaymentStatus.Active, StartDate = _currentDateTime.Object.Now.AddMonths(2) },
                                   new Domain.Entities.Apprenticeship { PaymentStatus = Domain.Entities.PaymentStatus.Active },
                                   new Domain.Entities.Apprenticeship { PaymentStatus = Domain.Entities.PaymentStatus.Active },
                                   new Domain.Entities.Apprenticeship { PaymentStatus = Domain.Entities.PaymentStatus.Active }
                               },
                });

            var query = new ApprenticeshipSearchQuery
                            {
                                ApprenticeshipStatuses = new List<ApprenticeshipStatus> { ApprenticeshipStatus.Live }
                            };
            var result = await _controller.GetApprenticeships(1L, query) as OkNegotiatedContentResult<ApprenticeshipSearchResponse>;
            result.Content.Apprenticeships.Count().Should().Be(4);
            result.Content.Facets.ApprenticeshipStatuses.Count.Should().Be(2);
        }
    }
}

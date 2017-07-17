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
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.EmployerControllerTests
{
    [TestFixture]
    public class WhenGettingAllApprenticeships
    {
        private Mock<IMediator> _mockMediator;
        private EmployerOrchestrator _employerOrchestrator;
        private ApprenticeshipsOrchestrator _apprenticeshipOrchestor;
        private EmployerController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockMediator = new Mock<IMediator>();
            _mockMediator.Setup(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()))
                .ReturnsAsync(new GetApprenticeshipsResponse
                                  {
                                      Data = new List<Apprenticeship>(),
                                  });

            _employerOrchestrator = new EmployerOrchestrator(_mockMediator.Object, Mock.Of<ICommitmentsLogger>(), new FacetMapper(), new ApprenticeshipFilterService(new FacetMapper()), Mock.Of<IApprenticeshipMapper>(), Mock.Of<ICommitmentMapper>());
            _apprenticeshipOrchestor = new ApprenticeshipsOrchestrator(_mockMediator.Object, Mock.Of<ICommitmentsLogger>());

            _controller = new EmployerController(_employerOrchestrator, _apprenticeshipOrchestor);
        }

        [Test]
        public async Task ShouldHandleEmptyResponse()
        {
            var restult = await _controller.GetApprenticeships(1L, new ApprenticeshipSearchQuery()) as OkNegotiatedContentResult<ApprenticeshipSearchResponse>;
            restult.Content.Apprenticeships.Count().Should().Be(0);
        }

        [Test]
        public async Task ControllerTest()
        {
            _mockMediator.Setup(m => m.SendAsync(It.IsAny<GetApprenticeshipsRequest>()))
                .ReturnsAsync(new GetApprenticeshipsResponse
                {
                    Data = new List<Apprenticeship>
                               {
                                   new Apprenticeship { PaymentStatus = PaymentStatus.Active, StartDate = DateTime.Now.AddMonths(-2) },
                                   new Apprenticeship { PaymentStatus = PaymentStatus.Active, StartDate = DateTime.Now.AddMonths(2) },
                                   new Apprenticeship { PaymentStatus = PaymentStatus.Active },
                                   new Apprenticeship { PaymentStatus = PaymentStatus.Active },
                                   new Apprenticeship { PaymentStatus = PaymentStatus.Active }
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

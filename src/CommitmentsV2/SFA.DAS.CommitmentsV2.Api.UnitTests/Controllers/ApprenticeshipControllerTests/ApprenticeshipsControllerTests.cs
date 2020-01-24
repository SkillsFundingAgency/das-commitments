using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using GetApprenticeshipsResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetApprenticeshipsResponse;
using ApprenticeshipDetailsResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetApprenticeshipsResponse.ApprenticeshipDetailsResponse;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ApprenticeshipControllerTests
{
    public class ApprenticeshipsControllerTests
    {
        private Mock<IMediator> _mediator;
        private Mock<ILogger<ApprenticeshipController>> _logger;
        private Mock<IModelMapper> _mapper;
        private ApprenticeshipController _controller;

        [SetUp]
        public void Init()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<ApprenticeshipController>>();
            _mapper = new Mock<IModelMapper>();

            _controller = new ApprenticeshipController(_mediator.Object, _mapper.Object, _logger.Object);
        }

        [Test]
        public async Task GetApprovedApprentices()
        {
            //Arrange
            var request = new GetApprenticeshipsRequest
            {
                ProviderId = 10
            };

            //Act
            await _controller.GetApprenticeships(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsQuery>(r => 
                    r.ProviderId.Equals(request.ProviderId)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetApprovedApprenticesByPage()
        {
            //Arrange
            var request = new GetApprenticeshipsRequest
            {
                ProviderId = 10,
                PageNumber = 4,
                PageItemCount = 17
            };

            //Act
            await _controller.GetApprenticeships(request);

            //Assert
            _mediator.Verify(m => m.Send(
                It.Is<GetApprenticeshipsQuery>(r => 
                    r.ProviderId.Equals(request.ProviderId) &&
                    r.PageNumber.Equals(request.PageNumber) &&
                    r.PageItemCount.Equals(request.PageItemCount)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ThenTheQueryResultIsMapped()
        {
            //Arrange
            var expectedProviderId = 10;
            var request = new GetApprenticeshipsRequest
            {
                ProviderId = expectedProviderId
            };
            _mediator.Setup(x => x.Send(It.Is<GetApprenticeshipsQuery>(c => c.ProviderId.Equals(expectedProviderId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetApprenticeshipsQueryResult());
            
            //Act
            var result = await _controller.GetApprenticeships(request) as OkObjectResult;

            //Assert
            Assert.IsNotNull(result);
            _mapper.Verify(x=>x.Map<GetApprenticeshipsResponse>(It.IsAny<GetApprenticeshipsQueryResult>()), Times.Once);
        }

        [Test]
        public async Task ReturnNotFoundIfNullIsReturned()
        {
            //Act
            var result = await _controller.GetApprenticeships(new GetApprenticeshipsRequest()) as NotFoundResult;

            //Assert
            Assert.IsNotNull(result);
        }
    }
}

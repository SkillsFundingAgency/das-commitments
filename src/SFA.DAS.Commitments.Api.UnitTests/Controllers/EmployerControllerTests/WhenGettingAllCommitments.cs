﻿using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using MediatR;
using SFA.DAS.Commitments.Api.Controllers;
using Ploeh.AutoFixture.NUnit3;
using System.Web.Http.Results;

using FluentAssertions;
using FluentValidation;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Application.Queries.GetCommitments;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.EmployerControllerTests
{
    [TestFixture]
    public class WhenIGetAllEmployerCommitments
    {
        private Mock<IMediator> _mockMediator;
        private EmployerController _controller;
        private EmployerOrchestrator _employerOrchestrator;
        private ApprenticeshipsOrchestrator _apprenticeshipsOrchestrator;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _employerOrchestrator = new EmployerOrchestrator(_mockMediator.Object, Mock.Of<ICommitmentsLogger>(), new FacetMapper(Mock.Of<ICurrentDateTime>()), new ApprenticeshipFilterService(new FacetMapper(Mock.Of<ICurrentDateTime>())), Mock.Of<IApprenticeshipMapper>(), Mock.Of<ICommitmentMapper>());

            _apprenticeshipsOrchestrator = new ApprenticeshipsOrchestrator(
                _mockMediator.Object,
                Mock.Of<IDataLockMapper>(),
                Mock.Of<IApprenticeshipMapper>(),
                Mock.Of<ICommitmentsLogger>());

            _controller = new EmployerController(_employerOrchestrator, _apprenticeshipsOrchestrator);
        }

        [Test, AutoData]
        public async Task ThenAListOfCommitmentsWillBeReturned(GetCommitmentsResponse mediatorResponse)
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentsRequest>())).ReturnsAsync(mediatorResponse);

            var result = await _controller.GetCommitments(1234L) as OkNegotiatedContentResult<IEnumerable<CommitmentListItem>>;

            result.Should().NotBeNull();
            //result.Content.Should().BeSameAs(mediatorResponse.Data);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledWithTheProviderId()
        {
            const long testAccountId = 1234L;
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentsRequest>())).ReturnsAsync(new GetCommitmentsResponse());

            var result = await _controller.GetCommitments(testAccountId);

            _mockMediator.Verify(x => x.SendAsync(It.Is<GetCommitmentsRequest>(arg => arg.Caller.CallerType == CallerType.Employer && arg.Caller.Id == testAccountId)));
        }

        [Test]
        public void ThenShouldReturnBadRequestIfThrowsAnInvalidRequestException()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetCommitmentsRequest>())).ThrowsAsync(new ValidationException(""));

            Assert.ThrowsAsync<ValidationException>(async () => await _controller.GetCommitments(-1L));
        }
    }
}

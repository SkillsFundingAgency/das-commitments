using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture.NUnit3;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Types.ProviderPayment;
using SFA.DAS.Commitments.Application.Queries.GetCustomProviderPaymentsPriority;
using SFA.DAS.Commitments.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Results;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Application.Services;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.EmployerControllerTests
{
    [TestFixture]
    public sealed class WhenGettingProviderPaymentPriorites
    {
        private EmployerController _controller;
        private Mock<IMediator> _mockMediator;
        private EmployerOrchestrator _employerOrchestrator;
        private ApprenticeshipsOrchestrator _apprenticeshipOrchestor;

        [SetUp]
        public void Setup()
        {
            var mapper = new FacetMapper();
            _mockMediator = new Mock<IMediator>();
            _employerOrchestrator = new EmployerOrchestrator(
                _mockMediator.Object, 
                Mock.Of<ICommitmentsLogger>(), 
                mapper, new ApprenticeshipFilterService(mapper),
                Mock.Of<IApprenticeshipMapper>(),
                Mock.Of<ICommitmentMapper>());
            _apprenticeshipOrchestor = new ApprenticeshipsOrchestrator(_mockMediator.Object, Mock.Of<IDataLockMapper>(), Mock.Of<IApprenticeshipMapper>(), Mock.Of<ICommitmentsLogger>());

            _controller = new EmployerController(_employerOrchestrator, _apprenticeshipOrchestor);
        }

        [Test, AutoData]
        public async Task ThenAListOfProviderPaymentPriorityItemsWillBeReturned(GetProviderPaymentsPriorityResponse mediatorResponse)
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetProviderPaymentsPriorityRequest>())).ReturnsAsync(mediatorResponse);

            var result = await _controller.GetCustomProviderPaymentPriority(1234L) as OkNegotiatedContentResult<IList<ProviderPaymentPriorityItem>>;

            result.Should().NotBeNull();
            result.Content.Should().BeSameAs(mediatorResponse.Data);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledWithTheEmployerAccountId()
        {
            const long testAccountId = 1234L;
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetProviderPaymentsPriorityRequest>())).ReturnsAsync(new GetProviderPaymentsPriorityResponse { Data = new List<ProviderPaymentPriorityItem>() });

            var result = await _controller.GetCustomProviderPaymentPriority(testAccountId);

            _mockMediator.Verify(x => x.SendAsync(It.Is<GetProviderPaymentsPriorityRequest>(arg => arg.EmployerAccountId == testAccountId)));
        }

        [Test]
        public void ThenShouldReturnBadRequestIfThrowsAnInvalidRequestException()
        {
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetProviderPaymentsPriorityRequest>())).ThrowsAsync(new ValidationException(""));

            Assert.ThrowsAsync<ValidationException>(async () => await _controller.GetCustomProviderPaymentPriority(-1L));
        }
    }
}

using System.Threading.Tasks;

using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.BulkUploadApprenticships;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Provider
{
    [TestFixture]
    public sealed class WhenBulkUploading
    {
        private Mock<IMediator> _mockMediator;
        private ProviderOrchestrator _orchestrator;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _orchestrator = new ProviderOrchestrator(_mockMediator.Object, Mock.Of<ICommitmentsLogger>());
        }

        [Test]
        public async Task ShouldCallTheMediatorBulkUpload()
        {
            await _orchestrator.CreateApprenticeships(1L, 2L, new BulkApprenticeshipRequest());

            _mockMediator.Verify(x => x.SendAsync(It.IsAny<BulkUploadApprenticeshipsCommand>()), Times.Once);
        }
    }
}

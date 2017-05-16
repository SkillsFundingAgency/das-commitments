using System.Threading.Tasks;

using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using SFA.DAS.Commitments.Application.Commands.BulkUploadApprenticships;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
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
            _orchestrator = new ProviderOrchestrator(
                _mockMediator.Object, 
                Mock.Of<ICommitmentsLogger>(), 
                Mock.Of<FacetMapper>(),
                new ApprenticeshipFilterService());
        }

        [Test]
        public async Task ShouldCallTheMediatorBulkUpload()
        {
            var providerId = 1L;
            var commitmentId = 2L;
            var request = new BulkApprenticeshipRequest { LastUpdatedByInfo = new LastUpdateInfo { EmailAddress = "test@email.com", Name = "Bob" }, UserId = "User" };
            await _orchestrator.CreateApprenticeships(providerId, commitmentId, request);

            _mockMediator.Verify(
                x =>
                    x.SendAsync(
                        It.Is<BulkUploadApprenticeshipsCommand>(
                            y =>
                                y.Caller.Id == providerId && y.Caller.CallerType == CallerType.Provider && y.CommitmentId == commitmentId && y.UserId == request.UserId &&
                                y.UserName == request.LastUpdatedByInfo.Name)), Times.Once);
        }
    }
}

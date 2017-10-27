using System.Collections.Generic;
using System.Threading.Tasks;

using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using SFA.DAS.Commitments.Application.Commands.BulkUploadApprenticships;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Provider
{
    [TestFixture]
    public sealed class WhenBulkUploading : ProviderOrchestratorTestBase
    {
        [Test]
        public async Task ShouldCallTheMediatorBulkUpload()
        {
            var providerId = 1L;
            var commitmentId = 2L;
            var request = new BulkApprenticeshipRequest
                              {
                                    LastUpdatedByInfo = new LastUpdateInfo { EmailAddress = "test@email.com", Name = "Bob" },
                                    UserId = "User",
                                    Apprenticeships = new List<Types.Apprenticeship.Apprenticeship>()
                              };

            await Orchestrator.CreateApprenticeships(providerId, commitmentId, request);
            MockMediator.Verify(
                x =>
                    x.SendAsync(
                        It.Is<BulkUploadApprenticeshipsCommand>(
                            y =>
                                y.Caller.Id == providerId && y.Caller.CallerType == CallerType.Provider && y.CommitmentId == commitmentId && y.UserId == request.UserId &&
                                y.UserName == request.LastUpdatedByInfo.Name)), Times.Once);
        }
    }
}

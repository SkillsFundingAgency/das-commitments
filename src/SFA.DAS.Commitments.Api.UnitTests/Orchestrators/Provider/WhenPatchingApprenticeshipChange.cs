using System;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using SFA.DAS.Commitments.Application.Commands.AcceptApprenticeshipChange;
using SFA.DAS.Commitments.Application.Commands.RejectApprenticeshipChange;
using SFA.DAS.Commitments.Application.Commands.UndoApprenticeshipChange;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Provider
{
    [TestFixture]
    public class WhenPatchingApprenticeshipChange : ProviderOrchestratorTestBase
    {

        [SetUp]
        public void Arrange()
        {
            MockMediator.Setup(x => x.SendAsync(It.IsAny<AcceptApprenticeshipChangeCommand>())).ReturnsAsync(new Unit());
            MockMediator.Setup(x => x.SendAsync(It.IsAny<RejectApprenticeshipChangeCommand>())).ReturnsAsync(new Unit());
            MockMediator.Setup(x => x.SendAsync(It.IsAny<UndoApprenticeshipChangeCommand>())).ReturnsAsync(new Unit());
        }

        [TestCase(ApprenticeshipUpdateStatus.Approved, typeof(AcceptApprenticeshipChangeCommand))]
        [TestCase(ApprenticeshipUpdateStatus.Rejected, typeof(RejectApprenticeshipChangeCommand))]
        [TestCase(ApprenticeshipUpdateStatus.Deleted, typeof(UndoApprenticeshipChangeCommand))]
        public async Task ThenAppropriateCommandIsSentToMediator(ApprenticeshipUpdateStatus updateStatus, Type expectedCommand)
        {
            //Arrange
            var submission = new ApprenticeshipUpdateSubmission
            {
                UpdateStatus = updateStatus,
                LastUpdatedByInfo = new LastUpdateInfo(),
                UserId = "TEST"
            };

            //Act
            await Orchestrator.PatchApprenticeshipUpdate(1, 1, submission);

            //Assert
            MockMediator.Verify(x => x.SendAsync(It.Is<IAsyncRequest>(o => o.GetType() == expectedCommand)), Times.Once);
        }

    }
}

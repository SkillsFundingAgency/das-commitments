using System;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using SFA.DAS.Commitments.Application.Commands.AcceptApprenticeshipChange;
using SFA.DAS.Commitments.Application.Commands.RejectApprenticeshipChange;
using SFA.DAS.Commitments.Application.Commands.UndoApprenticeshipChange;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Provider
{
    [TestFixture]
    public class WhenPatchingApprenticeshipChange
    {
        private ProviderOrchestrator _orchestrator;
        private Mock<IMediator> _mediator;

        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();
            _mediator.Setup(x => x.SendAsync(It.IsAny<AcceptApprenticeshipChangeCommand>())).ReturnsAsync(new Unit());
            _mediator.Setup(x => x.SendAsync(It.IsAny<RejectApprenticeshipChangeCommand>())).ReturnsAsync(new Unit());
            _mediator.Setup(x => x.SendAsync(It.IsAny<UndoApprenticeshipChangeCommand>())).ReturnsAsync(new Unit());

            _orchestrator = new ProviderOrchestrator(
                _mediator.Object,
                Mock.Of<ICommitmentsLogger>(),
                new FacetMapper(),
                new ApprenticeshipFilterService(new FacetMapper()),
                Mock.Of<IApprenticeshipMapper>()
                );
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
            await _orchestrator.PatchApprenticeshipUpdate(1, 1, submission);

            //Assert
            _mediator.Verify(x => x.SendAsync(It.Is<IAsyncRequest>(o => o.GetType() == expectedCommand)), Times.Once);
        }

    }
}

using SFA.DAS.CommitmentsV2.Application.Commands.ResolveOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.CommandHandlers;

[TestFixture]
[Parallelizable]
public class ResolveOverlappingTrainingDateRequestCommandHandlerTests
{
    [Test]
    public async Task Handle_ResolveOverlappingTrainingDateRequest_ThenShouldResolvePendingOverlappingTrainingDateRequest()
    {
        var fixture = new ResolveOverlappingTrainingDateRequestCommandHandlerTestsFixture();
        await fixture.Handle();
        fixture.Verify_OverlappingTrainingDateRequest_Resolved();
    }

    [Test]
    public void Handle__Throw_Exception_When_RequestResolutionType_IsNull()
    {
        var fixture = new ResolveOverlappingTrainingDateRequestCommandHandlerTestsFixture();
        fixture.WithNullRequestResolutionType();
        Assert.ThrowsAsync<ArgumentNullException>(() => fixture.Handle());
    }

    private class ResolveOverlappingTrainingDateRequestCommandHandlerTestsFixture
    {
        private ResolveOverlappingTrainingDateRequestCommand _command;
        private readonly Mock<IResolveOverlappingTrainingDateRequestService> _resolveOverlappingTrainingDateRequestService;
        private readonly IRequestHandler<ResolveOverlappingTrainingDateRequestCommand> _sut;

        public ResolveOverlappingTrainingDateRequestCommandHandlerTestsFixture()
        {
            _command = new ResolveOverlappingTrainingDateRequestCommand
            {
                ApprenticeshipId = 1,
                ResolutionType = Types.OverlappingTrainingDateRequestResolutionType.ApprenticeshipIsStillActive
            };

            _resolveOverlappingTrainingDateRequestService = new Mock<IResolveOverlappingTrainingDateRequestService>();
            _sut = new ResolveOverlappingTrainingDateRequestCommandHandler(_resolveOverlappingTrainingDateRequestService.Object);
        }

        public ResolveOverlappingTrainingDateRequestCommandHandlerTestsFixture WithNullRequestResolutionType()
        {
            _command = new ResolveOverlappingTrainingDateRequestCommand
            {
                ApprenticeshipId = 1,
                ResolutionType = null
            };

            return this;
        }

        public async Task Handle()
        {
            await _sut.Handle(_command, CancellationToken.None);
        }

        public void Verify_OverlappingTrainingDateRequest_Resolved()
        {
            _resolveOverlappingTrainingDateRequestService.Verify(x => x.Resolve(_command.ApprenticeshipId, null, Types.OverlappingTrainingDateRequestResolutionType.ApprenticeshipIsStillActive), Times.Once);
        }
    }
}
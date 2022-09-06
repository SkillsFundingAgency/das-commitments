using MediatR;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.ResolveOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.CommandHandlers
{
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
            private Mock<IResolveOverlappingTrainingDateRequestService> _resolveOverlappingTrainingDateRequestService;
            private IRequestHandler<ResolveOverlappingTrainingDateRequestCommand> _sut;

            public ResolveOverlappingTrainingDateRequestCommandHandlerTestsFixture()
            {
                _command = new ResolveOverlappingTrainingDateRequestCommand()
                {
                    ApprenticeshipId = 1,
                    ResolutionType = Types.OverlappingTrainingDateRequestResolutionType.ApprentieshipIsStillActive
                };

                _resolveOverlappingTrainingDateRequestService = new Mock<IResolveOverlappingTrainingDateRequestService>();
                _sut = new ResolveOverlappingTrainingDateRequestCommandHandler(_resolveOverlappingTrainingDateRequestService.Object);
            }

            public ResolveOverlappingTrainingDateRequestCommandHandlerTestsFixture WithNullRequestResolutionType()
            {
                _command = new ResolveOverlappingTrainingDateRequestCommand()
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
                _resolveOverlappingTrainingDateRequestService.Verify(x => x.Resolve(_command.ApprenticeshipId, null, Types.OverlappingTrainingDateRequestResolutionType.ApprentieshipIsStillActive), Times.Once);
            }
        }
    }
}
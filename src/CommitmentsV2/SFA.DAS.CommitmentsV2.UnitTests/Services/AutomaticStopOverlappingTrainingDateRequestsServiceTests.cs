using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPendingOverlappingTrainingDatesToStop;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class AutomaticStopOverlappingTrainingDateRequestsServiceTests
    {
        private Fixture _fixture;
        private Mock<IMediator> _mediatorMock;
        private Mock<ILogger<AutomaticStopOverlappingTrainingDateRequestsService>> _loggerMock;
        private AutomaticStopOverlappingTrainingDateRequestsService _service;
        private Mock<IMessageSession> _messageSession;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _mediatorMock = new Mock<IMediator>();
            _messageSession = new Mock<IMessageSession>();

            _loggerMock = new Mock<ILogger<AutomaticStopOverlappingTrainingDateRequestsService>>();
            _service = new AutomaticStopOverlappingTrainingDateRequestsService(_mediatorMock.Object, _messageSession.Object, _loggerMock.Object);
        }

        [Test]
        public async Task AutomaticallyStopOverlappingTrainingDateRequest_WhenNoPendingRecords_ReturnsEmpty()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPendingOverlappingTrainingDatesToStopQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetPendingOverlappingTrainingDatesToStopResult());

            // Act
            await _service.AutomaticallyStopOverlappingTrainingDateRequests();

            // Assert
            _messageSession.Verify(m => m.Send(It.IsAny<AutomaticallyStopOverlappingTrainingDateRequestCommand>(), It.IsAny<SendOptions>()), Times.Never);

        }

        [Test]
        public async Task AutomaticallyStopOverlappingTrainingDateRequest_WhenPendingRecordsExist_SendsStopApprenticeshipCommands()
        {
            // Arrange
            var overlappingTrainingDatesToStop = _fixture.Create<GetPendingOverlappingTrainingDatesToStopResult>();
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPendingOverlappingTrainingDatesToStopQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(overlappingTrainingDatesToStop);

            // Act
            await _service.AutomaticallyStopOverlappingTrainingDateRequests();

            // Assert
            foreach (var request in overlappingTrainingDatesToStop.OverlappingTrainingDateRequests)
            {
                if (request.DraftApprenticeship != null)
                {
                    _messageSession.Verify(m => m.Send(It.Is<AutomaticallyStopOverlappingTrainingDateRequestCommand>(cmd =>
                          cmd.AccountId == request.PreviousApprenticeship.Cohort.EmployerAccountId &&
                            cmd.ApprenticeshipId == request.PreviousApprenticeshipId &&
                            cmd.StopDate == request.DraftApprenticeship.StartDate.Value &&
                            cmd.MadeRedundant == false &&
                            cmd.UserInfo == Types.UserInfo.System &&
                            cmd.Party == Types.Party.Employer), It.IsAny<SendOptions>()), Times.Once);
                }
            }
        }

        [Test]
        public void AutomaticallyStopOverlappingTrainingDateRequest_WhenExceptionThrown_LogsErrorAndRethrows()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPendingOverlappingTrainingDatesToStopQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act + Assert
            _service.Awaiting(s => s.AutomaticallyStopOverlappingTrainingDateRequests())
                .Should().ThrowAsync<Exception>().WithMessage("Test exception");

        }
    }
}

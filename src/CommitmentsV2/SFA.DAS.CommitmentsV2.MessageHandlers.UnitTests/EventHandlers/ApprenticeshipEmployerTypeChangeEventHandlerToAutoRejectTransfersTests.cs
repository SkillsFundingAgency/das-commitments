using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EmployerAccounts.Messages.Events;
using ApprenticeshipEmployerType = SFA.DAS.Common.Domain.Types.ApprenticeshipEmployerType;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class ApprenticeshipEmployerTypeChangeEventHandlerToAutoRejectTransfersTests
{
    private const long SenderAccountId = 1001;
    private const long FirstTransferRequestId = 501;
    private const long SecondTransferRequestId = 502;

    private static bool IsRejectCommand(object message, long transferRequestId) =>
        message is RejectTransferRequestCommand command
        && command.TransferRequestId == transferRequestId
        && command.UserInfo.IsSystem();

    [Test]
    public async Task Handle_WhenEmployerBecomesNonLevy_ThenRejectTransferRequestCommandsAreSentForPendingSenderRequests()
    {
        // Arrange
        var fixture = new ApprenticeshipEmployerTypeChangeEventHandlerToAutoRejectTransfersTestsFixture()
            .WithPendingTransferRequestForSender(SenderAccountId, FirstTransferRequestId)
            .WithPendingTransferRequestForSender(SenderAccountId, SecondTransferRequestId)
            .WithPendingTransferRequestForSender(9999, 503);

        var sentCommands = new List<RejectTransferRequestCommand>();
        fixture.MessageHandlerContext
            .Setup(m => m.Send(
                It.Is<object>(o => o is RejectTransferRequestCommand),
                It.Is<SendOptions>(_ => true)))
            .Callback<object, SendOptions>((command, _) => sentCommands.Add((RejectTransferRequestCommand)command))
            .Returns(Task.CompletedTask);

        // Act
        await fixture.Handle();

        // Assert
        Assert.That(sentCommands.Select(c => c.TransferRequestId), Is.EquivalentTo(new[] { FirstTransferRequestId, SecondTransferRequestId }));
        Assert.That(sentCommands, Has.All.Matches<RejectTransferRequestCommand>(c => c.UserInfo.IsSystem()));
    }

    [Test]
    public async Task Handle_WhenEmployerBecomesLevy_ThenNoRejectTransferRequestCommandsAreSent()
    {
        // Arrange
        var fixture = new ApprenticeshipEmployerTypeChangeEventHandlerToAutoRejectTransfersTestsFixture()
            .WithPendingTransferRequestForSender(SenderAccountId, FirstTransferRequestId)
            .WithEmployerType(ApprenticeshipEmployerType.Levy);

        // Act
        await fixture.Handle();

        // Assert
        fixture.MessageHandlerContext.Verify(m => m.Send(
                It.Is<object>(o => IsRejectCommand(o, FirstTransferRequestId)),
                It.Is<SendOptions>(_ => true)),
            Times.Never);
    }

    [Test]
    public async Task Handle_WhenEmployerTypeIsUnknown_ThenNoRejectTransferRequestCommandsAreSent()
    {
        // Arrange
        var fixture = new ApprenticeshipEmployerTypeChangeEventHandlerToAutoRejectTransfersTestsFixture()
            .WithPendingTransferRequestForSender(SenderAccountId, FirstTransferRequestId)
            .WithEmployerType(ApprenticeshipEmployerType.Unknown);

        // Act
        await fixture.Handle();

        // Assert
        fixture.MessageHandlerContext.Verify(m => m.Send(
                It.Is<object>(o => IsRejectCommand(o, FirstTransferRequestId)),
                It.Is<SendOptions>(_ => true)),
            Times.Never);
    }

    [Test]
    public async Task Handle_WhenTransferRequestIsApprovedOrRejected_ThenNoRejectTransferRequestCommandIsSent()
    {
        // Arrange
        var fixture = new ApprenticeshipEmployerTypeChangeEventHandlerToAutoRejectTransfersTestsFixture()
            .WithTransferRequestForSender(SenderAccountId, 601, TransferApprovalStatus.Approved)
            .WithTransferRequestForSender(SenderAccountId, 602, TransferApprovalStatus.Rejected);

        // Act
        await fixture.Handle();

        // Assert
        fixture.MessageHandlerContext.Verify(m => m.Send(
                It.Is<object>(o => IsRejectCommand(o, 601)),
                It.Is<SendOptions>(_ => true)),
            Times.Never);

        fixture.MessageHandlerContext.Verify(m => m.Send(
                It.Is<object>(o => IsRejectCommand(o, 602)),
                It.Is<SendOptions>(_ => true)),
            Times.Never);
    }
}

public class ApprenticeshipEmployerTypeChangeEventHandlerToAutoRejectTransfersTestsFixture : IDisposable
{
    private readonly ProviderCommitmentsDbContext _db;
    private readonly ApprenticeshipEmployerTypeChangeEventHandlerToAutoRejectTransfers _handler;

    public Mock<IMessageHandlerContext> MessageHandlerContext { get; } = new();
    public ApprenticeshipEmployerTypeChangeEvent Event { get; private set; } = new()
    {
        AccountId = 1001,
        ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
        Created = DateTime.UtcNow
    };

    public ApprenticeshipEmployerTypeChangeEventHandlerToAutoRejectTransfersTestsFixture()
    {
        _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
            .Options);

        _handler = new ApprenticeshipEmployerTypeChangeEventHandlerToAutoRejectTransfers(
            new Lazy<ProviderCommitmentsDbContext>(() => _db),
            Mock.Of<ILogger<ApprenticeshipEmployerTypeChangeEventHandlerToAutoRejectTransfers>>());
    }

    public ApprenticeshipEmployerTypeChangeEventHandlerToAutoRejectTransfersTestsFixture WithEmployerType(ApprenticeshipEmployerType employerType)
    {
        Event.ApprenticeshipEmployerType = employerType;
        return this;
    }

    public ApprenticeshipEmployerTypeChangeEventHandlerToAutoRejectTransfersTestsFixture WithPendingTransferRequestForSender(long senderAccountId, long transferRequestId)
    {
        return WithTransferRequestForSender(senderAccountId, transferRequestId, TransferApprovalStatus.Pending);
    }

    public ApprenticeshipEmployerTypeChangeEventHandlerToAutoRejectTransfersTestsFixture WithTransferRequestForSender(
        long senderAccountId,
        long transferRequestId,
        TransferApprovalStatus status)
    {
        var cohort = new Cohort
        {
            Id = transferRequestId + 1000,
            TransferSenderId = senderAccountId,
            EmployerAccountId = senderAccountId + 1
        };

        var transferRequest = new TransferRequest
        {
            Id = transferRequestId,
            Status = status,
            Cohort = cohort,
            CommitmentId = cohort.Id
        };

        _db.Cohorts.Add(cohort);
        _db.TransferRequests.Add(transferRequest);
        _db.SaveChanges();

        return this;
    }

    public Task Handle() => _handler.Handle(Event, MessageHandlerContext.Object);

    public void Dispose() => _db.Dispose();
}

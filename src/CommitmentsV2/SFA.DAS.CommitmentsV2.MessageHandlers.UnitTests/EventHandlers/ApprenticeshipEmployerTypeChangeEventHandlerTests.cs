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
public class ApprenticeshipEmployerTypeChangeEventHandlerTests
{
    [Test]
    public async Task Handle_WhenEmployerBecomesNonLevy_ThenRejectTransferRequestCommandsAreSentForPendingSenderRequests()
    {
        // Arrange
        var fixture = new ApprenticeshipEmployerTypeChangeEventHandlerTestsFixture()
            .WithPendingTransferRequestForSender(1001, 501)
            .WithPendingTransferRequestForSender(1001, 502)
            .WithPendingTransferRequestForSender(9999, 503);

        // Act
        await fixture.Handle();

        // Assert
        fixture.MessageHandlerContext.Verify(m => m.Send(
                It.Is<RejectTransferRequestCommand>(c =>
                    c.TransferRequestId == 501 &&
                    c.UserInfo.IsSystem()),
                It.IsAny<SendOptions>()),
            Times.Once);

        fixture.MessageHandlerContext.Verify(m => m.Send(
                It.Is<RejectTransferRequestCommand>(c => c.TransferRequestId == 502),
                It.IsAny<SendOptions>()),
            Times.Once);

        fixture.MessageHandlerContext.Verify(m => m.Send(
                It.IsAny<RejectTransferRequestCommand>(),
                It.IsAny<SendOptions>()),
            Times.Exactly(2));
    }

    [Test]
    public async Task Handle_WhenEmployerBecomesLevy_ThenNoRejectTransferRequestCommandsAreSent()
    {
        // Arrange
        var fixture = new ApprenticeshipEmployerTypeChangeEventHandlerTestsFixture()
            .WithPendingTransferRequestForSender(1001, 501)
            .WithEmployerType(ApprenticeshipEmployerType.Levy);

        // Act
        await fixture.Handle();

        // Assert
        fixture.MessageHandlerContext.Verify(m => m.Send(
                It.IsAny<RejectTransferRequestCommand>(),
                It.IsAny<SendOptions>()),
            Times.Never);
    }

    [Test]
    public async Task Handle_WhenEmployerTypeIsUnknown_ThenNoRejectTransferRequestCommandsAreSent()
    {
        // Arrange
        var fixture = new ApprenticeshipEmployerTypeChangeEventHandlerTestsFixture()
            .WithPendingTransferRequestForSender(1001, 501)
            .WithEmployerType(ApprenticeshipEmployerType.Unknown);

        // Act
        await fixture.Handle();

        // Assert
        fixture.MessageHandlerContext.Verify(m => m.Send(
                It.IsAny<RejectTransferRequestCommand>(),
                It.IsAny<SendOptions>()),
            Times.Never);
    }

    [Test]
    public async Task Handle_WhenTransferRequestIsApprovedOrRejected_ThenNoRejectTransferRequestCommandIsSent()
    {
        // Arrange
        var fixture = new ApprenticeshipEmployerTypeChangeEventHandlerTestsFixture()
            .WithTransferRequestForSender(1001, 601, TransferApprovalStatus.Approved)
            .WithTransferRequestForSender(1001, 602, TransferApprovalStatus.Rejected);

        // Act
        await fixture.Handle();

        // Assert
        fixture.MessageHandlerContext.Verify(m => m.Send(
                It.IsAny<RejectTransferRequestCommand>(),
                It.IsAny<SendOptions>()),
            Times.Never);
    }
}

public class ApprenticeshipEmployerTypeChangeEventHandlerTestsFixture : IDisposable
{
    private readonly ProviderCommitmentsDbContext _db;
    private readonly ApprenticeshipEmployerTypeChangeEventHandler _handler;

    public Mock<IMessageHandlerContext> MessageHandlerContext { get; } = new();
    public ApprenticeshipEmployerTypeChangeEvent Event { get; private set; } = new()
    {
        AccountId = 1001,
        ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
        Created = DateTime.UtcNow
    };

    public ApprenticeshipEmployerTypeChangeEventHandlerTestsFixture()
    {
        _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
            .Options);

        _handler = new ApprenticeshipEmployerTypeChangeEventHandler(
            new Lazy<ProviderCommitmentsDbContext>(() => _db),
            Mock.Of<ILogger<ApprenticeshipEmployerTypeChangeEventHandler>>());
    }

    public ApprenticeshipEmployerTypeChangeEventHandlerTestsFixture WithEmployerType(ApprenticeshipEmployerType employerType)
    {
        Event.ApprenticeshipEmployerType = employerType;
        return this;
    }

    public ApprenticeshipEmployerTypeChangeEventHandlerTestsFixture WithPendingTransferRequestForSender(long senderAccountId, long transferRequestId)
    {
        return WithTransferRequestForSender(senderAccountId, transferRequestId, TransferApprovalStatus.Pending);
    }

    public ApprenticeshipEmployerTypeChangeEventHandlerTestsFixture WithTransferRequestForSender(
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

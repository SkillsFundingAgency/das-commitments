using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;
using SFA.DAS.LearnerData.Messages;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.ExternalMessageHandlers.UnitTests.EventHandlers;

[TestFixture]
public class LearnerDataUpdatedEventHandlerTests
{
    private Fixture _fixture;
    private ProviderCommitmentsDbContext _dbContext;
    private Mock<ILogger<LearnerDataUpdatedEventHandler>> _mockLogger;
    private Mock<IMessageHandlerContext> _mockContext;
    private LearnerDataUpdatedEventHandler _handler;
    private UnitOfWorkContext _unitOfWorkContext;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _mockLogger = new Mock<ILogger<LearnerDataUpdatedEventHandler>>();
        _mockContext = new Mock<IMessageHandlerContext>();
        _unitOfWorkContext = new UnitOfWorkContext();

        var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ProviderCommitmentsDbContext(options);
        _handler = new LearnerDataUpdatedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext), _mockLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    [Test]
    public async Task Handle_WhenLearnerDataUpdatedEventReceived_LogsInformation()
    {
        var message = _fixture.Create<LearnerDataUpdatedEvent>();

        await _handler.Handle(message, _mockContext.Object);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Handling LearnerDataUpdatedEvent for learner {message.LearnerId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenNoDraftApprenticeshipsFound_LogsWarning()
    {
        var message = _fixture.Create<LearnerDataUpdatedEvent>();

        await _handler.Handle(message, _mockContext.Object);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"No draft apprenticeship found for learner {message.LearnerId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenDraftApprenticeshipFound_FlagsItCorrectly()
    {
        var message = _fixture.Create<LearnerDataUpdatedEvent>();
        var cohort = new Cohort
        {
            Id = _fixture.Create<long>(),
            WithParty = Party.Provider,
            Reference = _fixture.Create<string>()
        };
        
        var draftApprenticeship = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = false,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString(),
            Cohort = cohort
        };

        _dbContext.Cohorts.Add(cohort);
        _dbContext.DraftApprenticeships.Add(draftApprenticeship);
        await _dbContext.SaveChangesAsync();

        await _handler.ProcessLearnerDataChanges(message);

        var updatedApprenticeship = await _dbContext.DraftApprenticeships
            .FirstOrDefaultAsync(da => da.LearnerDataId == message.LearnerId);

        updatedApprenticeship.Should().NotBeNull();
        updatedApprenticeship.HasLearnerDataChanges.Should().BeTrue();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Flagged draft apprenticeship {draftApprenticeship.Id} for learner data changes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenMultipleDraftApprenticeshipsFound_FlagsAllCorrectly()
    {
        var message = _fixture.Create<LearnerDataUpdatedEvent>();
        var cohort1 = new Cohort
        {
            Id = _fixture.Create<long>(),
            WithParty = Party.Provider,
            Reference = _fixture.Create<string>()
        };
        var cohort2 = new Cohort
        {
            Id = _fixture.Create<long>(),
            WithParty = Party.Provider,
            Reference = _fixture.Create<string>()
        };
        
        var draftApprenticeship1 = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = false,
            FirstName = "Test1",
            LastName = "User1",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString(),
            Cohort = cohort1
        };
        var draftApprenticeship2 = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = false,
            FirstName = "Test2",
            LastName = "User2",
            DateOfBirth = DateTime.UtcNow.AddYears(-21),
            Uln = _fixture.Create<long>().ToString(),
            Cohort = cohort2
        };

        _dbContext.Cohorts.AddRange(cohort1, cohort2);
        _dbContext.DraftApprenticeships.AddRange(draftApprenticeship1, draftApprenticeship2);
        await _dbContext.SaveChangesAsync();

        await _handler.ProcessLearnerDataChanges(message);

        var updatedApprenticeships = await _dbContext.DraftApprenticeships
            .Where(da => da.LearnerDataId == message.LearnerId)
            .ToListAsync();

        updatedApprenticeships.Should().HaveCount(2);
        updatedApprenticeships.Should().AllSatisfy(da => da.HasLearnerDataChanges.Should().BeTrue());
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Flagged draft apprenticeship {draftApprenticeship1.Id} for learner data changes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Flagged draft apprenticeship {draftApprenticeship2.Id} for learner data changes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenAlreadyFlagged_UpdatesChangeDate()
    {
        var message = _fixture.Create<LearnerDataUpdatedEvent>();
        var cohort = new Cohort
        {
            Id = _fixture.Create<long>(),
            WithParty = Party.Provider,
            Reference = _fixture.Create<string>()
        };
        
        var draftApprenticeship = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = true,
            LastLearnerDataSync = null,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString(),
            Cohort = cohort
        };

        _dbContext.Cohorts.Add(cohort);
        _dbContext.DraftApprenticeships.Add(draftApprenticeship);
        await _dbContext.SaveChangesAsync();

        await _handler.ProcessLearnerDataChanges(message);

        var updatedApprenticeship = await _dbContext.DraftApprenticeships
            .FirstOrDefaultAsync(da => da.LearnerDataId == message.LearnerId);

        updatedApprenticeship.Should().NotBeNull();
        updatedApprenticeship.HasLearnerDataChanges.Should().BeTrue();
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenNoDraftApprenticeshipsFound_LogsWarning()
    {
        var message = _fixture.Create<LearnerDataUpdatedEvent>();

        await _handler.ProcessLearnerDataChanges(message);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"No draft apprenticeship found for learner {message.LearnerId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenCohortWithEmployer_TransitionsBackToProvider()
    {
        var message = _fixture.Create<LearnerDataUpdatedEvent>();
        var cohort = new Cohort
        {
            Id = _fixture.Create<long>(),
            WithParty = Party.Employer,
            Reference = _fixture.Create<string>()
        };
        
        var draftApprenticeship = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = false,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString(),
            Cohort = cohort
        };

        _dbContext.Cohorts.Add(cohort);
        _dbContext.DraftApprenticeships.Add(draftApprenticeship);
        await _dbContext.SaveChangesAsync();

        await _handler.ProcessLearnerDataChanges(message);

        var updatedCohort = await _dbContext.Cohorts
            .FirstOrDefaultAsync(c => c.Id == cohort.Id);

        updatedCohort.Should().NotBeNull();
        updatedCohort.WithParty.Should().Be(Party.Provider);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Cohort {cohort.Id} is WithEmployer, transitioning back to WithProvider due to learner data changes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Successfully transitioned cohort {cohort.Id} from WithEmployer to WithProvider")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenCohortWithTransferSender_TransitionsBackToProvider()
    {
        var message = _fixture.Create<LearnerDataUpdatedEvent>();
        var cohort = new Cohort
        {
            Id = _fixture.Create<long>(),
            WithParty = Party.TransferSender,
            Reference = _fixture.Create<string>()
        };
        
        var draftApprenticeship = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = false,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString(),
            Cohort = cohort
        };

        _dbContext.Cohorts.Add(cohort);
        _dbContext.DraftApprenticeships.Add(draftApprenticeship);
        await _dbContext.SaveChangesAsync();

        await _handler.ProcessLearnerDataChanges(message);

        var updatedCohort = await _dbContext.Cohorts
            .FirstOrDefaultAsync(c => c.Id == cohort.Id);

        updatedCohort.Should().NotBeNull();
        updatedCohort.WithParty.Should().Be(Party.Provider);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Cohort {cohort.Id} is WithTransferSender, transitioning back to WithProvider due to learner data changes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Successfully transitioned cohort {cohort.Id} from WithTransferSender to WithProvider")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }


    [Test]
    public async Task ProcessLearnerDataChanges_CallsSaveChangesAsync()
    {
        var message = _fixture.Create<LearnerDataUpdatedEvent>();
        var cohort = new Cohort
        {
            Id = _fixture.Create<long>(),
            WithParty = Party.Provider,
            Reference = _fixture.Create<string>()
        };
        
        var draftApprenticeship = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = false,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString(),
            Cohort = cohort
        };

        _dbContext.Cohorts.Add(cohort);
        _dbContext.DraftApprenticeships.Add(draftApprenticeship);
        await _dbContext.SaveChangesAsync();

        await _handler.ProcessLearnerDataChanges(message);

        var updatedApprenticeship = await _dbContext.DraftApprenticeships
            .FirstOrDefaultAsync(da => da.LearnerDataId == message.LearnerId);

        updatedApprenticeship.Should().NotBeNull();
        updatedApprenticeship.HasLearnerDataChanges.Should().BeTrue();
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenCohortWithEmployer_CreatesSystemUserInfo()
    {
        var message = _fixture.Create<LearnerDataUpdatedEvent>();
        var cohort = new Cohort
        {
            Id = _fixture.Create<long>(),
            WithParty = Party.Employer,
            Reference = _fixture.Create<string>()
        };
        
        var draftApprenticeship = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = false,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString(),
            Cohort = cohort
        };

        _dbContext.Cohorts.Add(cohort);
        _dbContext.DraftApprenticeships.Add(draftApprenticeship);
        await _dbContext.SaveChangesAsync();

        await _handler.ProcessLearnerDataChanges(message);

        var updatedCohort = await _dbContext.Cohorts
            .FirstOrDefaultAsync(c => c.Id == cohort.Id);

        updatedCohort.Should().NotBeNull();
        updatedCohort.WithParty.Should().Be(Party.Provider);
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenCohortWithTransferSender_CreatesSystemMessage()
    {
        var message = _fixture.Create<LearnerDataUpdatedEvent>();
        var cohort = new Cohort
        {
            Id = _fixture.Create<long>(),
            WithParty = Party.TransferSender,
            Reference = _fixture.Create<string>()
        };
        
        var draftApprenticeship = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = false,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString(),
            Cohort = cohort
        };

        _dbContext.Cohorts.Add(cohort);
        _dbContext.DraftApprenticeships.Add(draftApprenticeship);
        await _dbContext.SaveChangesAsync();

        await _handler.ProcessLearnerDataChanges(message);

        var messages = await _dbContext.Messages
            .Where(m => m.CommitmentId == cohort.Id)
            .ToListAsync();

        messages.Should().HaveCount(1);
        messages.First().CreatedBy.Should().Be(0);
        messages.First().Author.Should().Be("System");
        messages.First().Text.Should().Be("Cohort returned to provider due to learner data changes requiring updates");
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenCohortWithTransferSender_RejectsPendingTransferRequestsSilently()
    {
        var message = _fixture.Create<LearnerDataUpdatedEvent>();
        var cohort = new Cohort
        {
            Id = _fixture.Create<long>(),
            WithParty = Party.TransferSender,
            Reference = _fixture.Create<string>()
        };
        
        var transferRequest = new TransferRequest
        {
            Id = _fixture.Create<long>(),
            Status = TransferApprovalStatus.Pending,
            Cost = 1000,
            TrainingCourses = "[]",
            CommitmentId = cohort.Id,
            CreatedOn = DateTime.UtcNow,
            Cohort = cohort
        };

        var draftApprenticeship = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = false,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString(),
            Cohort = cohort
        };

        _dbContext.Cohorts.Add(cohort);
        _dbContext.TransferRequests.Add(transferRequest);
        _dbContext.DraftApprenticeships.Add(draftApprenticeship);
        await _dbContext.SaveChangesAsync();

        await _handler.ProcessLearnerDataChanges(message);

        var updatedTransferRequest = await _dbContext.TransferRequests
            .FirstOrDefaultAsync(tr => tr.Id == transferRequest.Id);

        updatedTransferRequest.Should().NotBeNull();
        updatedTransferRequest.Status.Should().Be(TransferApprovalStatus.Rejected);
        updatedTransferRequest.TransferApprovalActionedByEmployerName.Should().Be("System");
        updatedTransferRequest.TransferApprovalActionedOn.Should().NotBeNull();

        var rejectedEvents = _unitOfWorkContext.GetEvents().OfType<TransferRequestRejectedEvent>().ToList();
        rejectedEvents.Should().BeEmpty();
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenCohortWithTransferSender_RejectsMultiplePendingTransferRequests()
    {
        var message = _fixture.Create<LearnerDataUpdatedEvent>();
        var cohort = new Cohort
        {
            Id = _fixture.Create<long>(),
            WithParty = Party.TransferSender,
            Reference = _fixture.Create<string>()
        };
        
        var transferRequest1 = new TransferRequest
        {
            Id = _fixture.Create<long>(),
            Status = TransferApprovalStatus.Pending,
            Cost = 1000,
            TrainingCourses = "[]",
            CommitmentId = cohort.Id,
            CreatedOn = DateTime.UtcNow,
            Cohort = cohort
        };

        var transferRequest2 = new TransferRequest
        {
            Id = _fixture.Create<long>(),
            Status = TransferApprovalStatus.Pending,
            Cost = 2000,
            TrainingCourses = "[]",
            CommitmentId = cohort.Id,
            CreatedOn = DateTime.UtcNow,
            Cohort = cohort
        };

        var draftApprenticeship = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = false,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString(),
            Cohort = cohort
        };

        _dbContext.Cohorts.Add(cohort);
        _dbContext.TransferRequests.AddRange(transferRequest1, transferRequest2);
        _dbContext.DraftApprenticeships.Add(draftApprenticeship);
        await _dbContext.SaveChangesAsync();

        await _handler.ProcessLearnerDataChanges(message);

        var updatedTransferRequests = await _dbContext.TransferRequests
            .Where(tr => tr.Cohort.Id == cohort.Id)
            .ToListAsync();

        updatedTransferRequests.Should().HaveCount(2);
        updatedTransferRequests.Should().AllSatisfy(tr => tr.Status.Should().Be(TransferApprovalStatus.Rejected));
        
        var rejectedEvents = _unitOfWorkContext.GetEvents().OfType<TransferRequestRejectedEvent>().ToList();
        rejectedEvents.Should().BeEmpty();
    }

    [Test]
    public async Task ProcessLearnerDataChanges_WhenCohortWithTransferSender_OnlyRejectsPendingTransferRequests()
    {
        var message = _fixture.Create<LearnerDataUpdatedEvent>();
        var cohort = new Cohort
        {
            Id = _fixture.Create<long>(),
            WithParty = Party.TransferSender,
            Reference = _fixture.Create<string>()
        };
        
        var pendingTransferRequest = new TransferRequest
        {
            Id = _fixture.Create<long>(),
            Status = TransferApprovalStatus.Pending,
            Cost = 1000,
            TrainingCourses = "[]",
            CommitmentId = cohort.Id,
            CreatedOn = DateTime.UtcNow,
            Cohort = cohort
        };

        var approvedTransferRequest = new TransferRequest
        {
            Id = _fixture.Create<long>(),
            Status = TransferApprovalStatus.Approved,
            Cost = 2000,
            TrainingCourses = "[]",
            CommitmentId = cohort.Id,
            CreatedOn = DateTime.UtcNow,
            Cohort = cohort
        };

        var rejectedTransferRequest = new TransferRequest
        {
            Id = _fixture.Create<long>(),
            Status = TransferApprovalStatus.Rejected,
            Cost = 3000,
            TrainingCourses = "[]",
            CommitmentId = cohort.Id,
            CreatedOn = DateTime.UtcNow,
            Cohort = cohort
        };

        var draftApprenticeship = new DraftApprenticeship
        {
            Id = _fixture.Create<long>(),
            LearnerDataId = message.LearnerId,
            HasLearnerDataChanges = false,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Uln = _fixture.Create<long>().ToString(),
            Cohort = cohort
        };

        _dbContext.Cohorts.Add(cohort);
        _dbContext.TransferRequests.AddRange(pendingTransferRequest, approvedTransferRequest, rejectedTransferRequest);
        _dbContext.DraftApprenticeships.Add(draftApprenticeship);
        await _dbContext.SaveChangesAsync();

        await _handler.ProcessLearnerDataChanges(message);

        var updatedPendingTransferRequest = await _dbContext.TransferRequests
            .FirstOrDefaultAsync(tr => tr.Id == pendingTransferRequest.Id);
        updatedPendingTransferRequest.Status.Should().Be(TransferApprovalStatus.Rejected);

        var updatedApprovedTransferRequest = await _dbContext.TransferRequests
            .FirstOrDefaultAsync(tr => tr.Id == approvedTransferRequest.Id);
        updatedApprovedTransferRequest.Status.Should().Be(TransferApprovalStatus.Approved);

        var updatedRejectedTransferRequest = await _dbContext.TransferRequests
            .FirstOrDefaultAsync(tr => tr.Id == rejectedTransferRequest.Id);
        updatedRejectedTransferRequest.Status.Should().Be(TransferApprovalStatus.Rejected);
    }
} 
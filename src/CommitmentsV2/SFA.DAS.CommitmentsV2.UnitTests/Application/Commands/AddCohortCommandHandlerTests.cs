﻿using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

[TestFixture]
[Parallelizable]
public class AddCohortCommandHandlerTests
{
    [Test]
    public async Task ShouldCreateCohort()
    {
        const string expectedHash = "ABC123";

        const long providerId = 1;
        const long accountId = 2;
        const long accountLegalEntityId = 3;
        long? transferSenderId = 4;
        int? pledgeApplicationId = 5;

        using var fixtures = new AddCohortCommandHandlerTestFixture()
            .WithGeneratedHash(expectedHash);

        var response = await fixtures.Handle(accountId, accountLegalEntityId, providerId, transferSenderId, pledgeApplicationId, "Course1");

        fixtures.CohortDomainServiceMock.Verify(x => x.CreateCohort(providerId, accountId, accountLegalEntityId, transferSenderId, pledgeApplicationId,
            It.IsAny<DraftApprenticeshipDetails>(),
            fixtures.UserInfo,
            AddCohortCommandHandlerTestFixture.RequestingParty,
            Constants.MinimumAgeAtApprenticeshipStart,
            Constants.MaximumAgeAtApprenticeshipStart,
            It.IsAny<CancellationToken>()));

        Assert.That(response.Reference, Is.EqualTo(expectedHash));
    }
}

public class TestLogger : ILogger<AddCohortCommandHandler>
{
    private readonly List<(LogLevel logLevel, Exception exception, string message)> _logMessages = new List<(LogLevel logLevel, Exception exception, string message)>();

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        _logMessages.Add((logLevel, exception, formatter(state, exception)));
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }
}

public class AddCohortCommandHandlerTestFixture : IDisposable
{
    public ProviderCommitmentsDbContext Db { get; set; }

    public AddCohortCommandHandlerTestFixture()
    {
        Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
            .Options);

        EncodingServiceMock = new Mock<IEncodingService>();

        DraftApprenticeshipDetailsMapperMock =
            new Mock<IOldMapper<AddCohortCommand, DraftApprenticeshipDetails>>();
        DraftApprenticeshipDetailsMapperMock.Setup(x => x.Map(It.IsAny<AddCohortCommand>()))
            .ReturnsAsync(() => new DraftApprenticeshipDetails());

        var commitment = new Cohort();
        commitment.Apprenticeships.Add(new DraftApprenticeship());

        CohortDomainServiceMock = new Mock<ICohortDomainService>();
        CohortDomainServiceMock.Setup(x => x.CreateCohort(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long?>(), It.IsAny<int?>(),
                It.IsAny<DraftApprenticeshipDetails>(), It.IsAny<UserInfo>(), It.IsAny<Party>(), Constants.MinimumAgeAtApprenticeshipStart, Constants.MaximumAgeAtApprenticeshipStart, It.IsAny<CancellationToken>()))
            .ReturnsAsync(commitment);

        Logger = new TestLogger();
        UserInfo = new UserInfo();
    }

    public Mock<IEncodingService> EncodingServiceMock { get; }
    public IEncodingService EncodingService => EncodingServiceMock.Object;

    public Mock<IOldMapper<AddCohortCommand, DraftApprenticeshipDetails>> DraftApprenticeshipDetailsMapperMock { get; }

    public Mock<ICohortDomainService> CohortDomainServiceMock { get; }

    public TestLogger Logger { get; }
    public UserInfo UserInfo { get; }
    public const Party RequestingParty = Party.Provider;

    public AddCohortCommandHandlerTestFixture WithGeneratedHash(string hash)
    {
        EncodingServiceMock
            .Setup(hs => hs.Encode(It.IsAny<long>(), It.Is<EncodingType>(encoding => encoding == EncodingType.CohortReference)))
            .Returns(hash);

        return this;
    }

    public async Task<AddCohortResult> Handle(long accountId, long accountLegalEntity, long providerId, long? transferSenderId, int? pledgeApplicationId, string courseCode)
    {
        await Db.SaveChangesAsync();
            
        var command = new AddCohortCommand(
            RequestingParty,
            accountId,
            accountLegalEntity,
            providerId,
            courseCode, 
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            transferSenderId,
            pledgeApplicationId,
            null,
            null,
            UserInfo,
            false,
            false,
            null,
            null,
            null,
            Constants.MinimumAgeAtApprenticeshipStart,
            Constants.MaximumAgeAtApprenticeshipStart);

        var handler = new AddCohortCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db),
            EncodingService,
            Logger,
            DraftApprenticeshipDetailsMapperMock.Object,
            CohortDomainServiceMock.Object);

        var response = await handler.Handle(command, CancellationToken.None);
        await Db.SaveChangesAsync();

        return response;
    }

    public void Dispose()
    {
        Db?.Dispose();
        GC.SuppressFinalize(this);
    }
}
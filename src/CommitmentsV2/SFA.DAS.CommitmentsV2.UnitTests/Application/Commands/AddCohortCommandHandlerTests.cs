using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Api.Types;

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

        var response = await fixtures.Handle(accountId, accountLegalEntityId, providerId, transferSenderId, pledgeApplicationId, "Course1", Guid.NewGuid());

        fixtures.CohortDomainServiceMock.Verify(x => x.CreateCohort(providerId, accountId, accountLegalEntityId, transferSenderId, pledgeApplicationId,
            It.IsAny<DraftApprenticeshipDetails>(),
            fixtures.UserInfo,
            AddCohortCommandHandlerTestFixture.RequestingParty,
            It.IsAny<CancellationToken>()));

        response.Reference.Should().Be(expectedHash);
    }

    [Test]
    public async Task ShouldCallAutoCreateReservationAndAllocateItToFirstApprenticeship()
    {
        const string expectedHash = "ABC123";

        const long providerId = 1;
        const long accountId = 2;
        const long accountLegalEntityId = 3;
        long? transferSenderId = null;
        int? pledgeApplicationId = null;

        using var fixtures = new AddCohortCommandHandlerTestFixture()
            .WithGeneratedHash(expectedHash);

       await fixtures.Handle(accountId, accountLegalEntityId, providerId, transferSenderId, pledgeApplicationId, "Course1", null);

        fixtures.ReservationsApiClientMock.Verify(x =>
            x.CreateAutoReservation(
                It.Is<CreateAutoReservationRequest>(p =>
                    p.AccountId == accountId && p.StartDate == fixtures.StartDate && p.ProviderId == providerId &&
                    p.Id != null),
                It.IsAny<CancellationToken>()));
        fixtures.Db.DraftApprenticeships.FirstOrDefault().ReservationId.Should()
            .Be(fixtures.AutoReservationResponse.Id);
    }
}

public class TestLogger : ILogger<AddCohortHandler>
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
                It.IsAny<DraftApprenticeshipDetails>(), It.IsAny<UserInfo>(), It.IsAny<Party>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(commitment);

        AutoReservationResponse = new CreateAutoReservationResponse
        {
            Id = Guid.NewGuid()
        };

        ReservationsApiClientMock = new Mock<IReservationsApiClient>();
        ReservationsApiClientMock
            .Setup(
                x => x.CreateAutoReservation(It.IsAny<CreateAutoReservationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AutoReservationResponse);

        Logger = new TestLogger();
        UserInfo = new UserInfo();
    }

    public Mock<IEncodingService> EncodingServiceMock { get; }
    public IEncodingService EncodingService => EncodingServiceMock.Object;

    public Mock<IOldMapper<AddCohortCommand, DraftApprenticeshipDetails>> DraftApprenticeshipDetailsMapperMock { get; }

    public Mock<ICohortDomainService> CohortDomainServiceMock { get; }
    public Mock<IReservationsApiClient> ReservationsApiClientMock { get; }
    public CreateAutoReservationResponse AutoReservationResponse { get; set; }
    public DateTime StartDate = new (2024, 01, 01);
    public TestLogger Logger { get; }
    public UserInfo UserInfo { get; }
    public CreateAutoReservationResponse AutReservationResoAutoReservationResponse { set; get; }

    public const Party RequestingParty = Party.Provider;

    public AddCohortCommandHandlerTestFixture WithGeneratedHash(string hash)
    {
        EncodingServiceMock
            .Setup(hs => hs.Encode(It.IsAny<long>(), It.Is<EncodingType>(encoding => encoding == EncodingType.CohortReference)))
            .Returns(hash);

        return this;
    }

    public async Task<AddCohortResult> Handle(long accountId, long accountLegalEntity, long providerId, long? transferSenderId, int? pledgeApplicationId, string courseCode, Guid? reservationId)
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
            new DateTime(2024, 01, 01),
            null,
            null,
            null,
            reservationId,
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
            null);

        var handler = new AddCohortHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db),
            EncodingService,
            Logger,
            DraftApprenticeshipDetailsMapperMock.Object,
            CohortDomainServiceMock.Object,
            ReservationsApiClientMock.Object
            );

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
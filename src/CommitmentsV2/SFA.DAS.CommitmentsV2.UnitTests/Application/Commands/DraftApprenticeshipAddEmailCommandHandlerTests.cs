using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.Email;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

public class DraftApprenticeshipAddEmailCommandHandlerTests
{
    [Test]
    public async Task WhenHandlingCommand_ShouldUpdateTheEmail()
    {
        var email = "Test@email.com";
        using var fixture = new DraftApprenticeshipAddEmailCommandHandlerTestsFixture(email);
        await fixture.Handle();
        fixture.VerifyEmailUpdated();
    }


    [Test]
    public async Task WhenHandlingDraftApprenticeshipAddEmailCommand_IfDomainExceptionIsReturned_Then_ThrowDomainException()
    {        
        using var fixture = new DraftApprenticeshipAddEmailCommandHandlerTestsFixture("test");
        var action = async () => await fixture.Handle();

        await action.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.Count() == 1);
    }
}
public class DraftApprenticeshipAddEmailCommandHandlerTestsFixture : IDisposable
{
    public DraftApprenticeshipAddEmailCommand Command { get; set; }
    public DraftApprenticeshipAddEmailCommandHandler Handler { get; set; }

    public  Mock<IOverlapCheckService> OverlapCheckService { get; set; }
    public ProviderCommitmentsDbContext Db { get; set; }
    public UnitOfWorkContext UnitOfWorkContext { get; set; }
    public Party Party { get; set; }
    public long DraftApprenticeshipId { get; set; }

    public Mock<IResolveOverlappingTrainingDateRequestService> _resolveOverlappingTrainingDateRequestService;
    public DraftApprenticeshipAddEmailCommandHandlerTestsFixture(string email)
    {
        Party = Party.Provider;
        UnitOfWorkContext = new UnitOfWorkContext();
        Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                             .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                                             .Options);
        OverlapCheckService = new Mock<IOverlapCheckService>();
        var fixture = new Fixture();
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var Cohort = new CommitmentsV2.Models.Cohort()
           .Set(c => c.Id, 111)
           .Set(c => c.ProviderId, 333)
           .Set(c => c.WithParty, Party);

        var DraftApprenticeship = fixture.Build<CommitmentsV2.Models.DraftApprenticeship>()
         .With(s => s.Cohort, Cohort)
         .With(s => s.PaymentStatus, PaymentStatus.Active)
         .With(s => s.EndDate, DateTime.UtcNow)
         .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
         .With(s => s.IsApproved, false)
         .Without(s => s.ProviderRef)
         .Without(s => s.EpaOrg)
         .Without(s => s.ApprenticeshipUpdate)
         .Without(s => s.PreviousApprenticeship)
         .Without(s => s.ApprenticeshipConfirmationStatus)
         .Create();

        Db.DraftApprenticeships.Add(DraftApprenticeship);

        Db.SaveChanges();

        DraftApprenticeshipId = DraftApprenticeship.Id;

        var authenticationService = new Mock<IAuthenticationService>();
        authenticationService.Setup(x => x.GetUserParty()).Returns(() => Party);

        var lazyProviderDbContext = new Lazy<ProviderCommitmentsDbContext>(() => Db);

        Command = new DraftApprenticeshipAddEmailCommand
        {
            ApprenticeshipId = DraftApprenticeship.Id,
            CohortId = Cohort.Id,
            Email = email
        };

        Handler = new DraftApprenticeshipAddEmailCommandHandler(lazyProviderDbContext,
            Mock.Of<ILogger<DraftApprenticeshipAddEmailCommandHandler>>(), OverlapCheckService.Object);
    }
    public async Task Handle()
    {
        await Handler.Handle(Command, CancellationToken.None);
        await Db.SaveChangesAsync();
    }
    internal void VerifyEmailUpdated()
    {
        Assert.That(Db.DraftApprenticeships.First(x => x.Id == DraftApprenticeshipId).Email, Is.EqualTo(Command.Email));
    }

    public void Dispose()
    {
        Db?.Dispose();
        GC.SuppressFinalize(this);
    }
}

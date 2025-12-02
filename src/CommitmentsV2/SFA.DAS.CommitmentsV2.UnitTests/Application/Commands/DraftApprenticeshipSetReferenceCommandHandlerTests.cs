using Microsoft.Extensions.Logging;
using NUnit.Framework.Internal;
using SFA.DAS.CommitmentsV2.Application.Commands.Reference;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;
using IAuthenticationService = SFA.DAS.CommitmentsV2.Authentication.IAuthenticationService;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

public class DraftApprenticeshipSetReferenceCommandHandlerTests
{
    [Test]
    public void WhenHandlingCommand_IfReferenceIsEmpty_Then_ThrowDomainException()
    {
        using var fixture = new DraftApprenticeshipSetReferenceCommandHandlerTestsFixture(string.Empty);

        Assert.ThrowsAsync<DomainException>(async () => await fixture.Handle());
    }

    [Test]
    public async Task WhenHandlingCommand_ShouldUpdateTheReference()
    {
        using var fixture = new DraftApprenticeshipSetReferenceCommandHandlerTestsFixture("Test Reference");
        await fixture.Handle();
        fixture.VerifyReferenceUpdated();
    }   
}
public class DraftApprenticeshipSetReferenceCommandHandlerTestsFixture : IDisposable
{
    public DraftApprenticeshipSetReferenceCommand Command { get; set; }
    public IRequestHandler<DraftApprenticeshipSetReferenceCommand> Handler { get; set; }
    public ProviderCommitmentsDbContext Db { get; set; }
    public UnitOfWorkContext UnitOfWorkContext { get; set; }
    public Party Party { get; set; }
    public long DraftApprenticeshipId { get; set; }

    public Mock<IResolveOverlappingTrainingDateRequestService> _resolveOverlappingTrainingDateRequestService;
    public DraftApprenticeshipSetReferenceCommandHandlerTestsFixture(string reference)
    {
        Party = Party.Provider; 
        UnitOfWorkContext = new UnitOfWorkContext();
        Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                             .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                                             .Options);
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
         .With(s=> s.IsApproved, false)
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

        Command = new DraftApprenticeshipSetReferenceCommand
        {
            ApprenticeshipId = DraftApprenticeship.Id,
            CohortId = Cohort.Id,
            Party = Party,
            Reference = reference
        };

        Handler = new DraftApprenticeshipSetReferenceCommandHandler(lazyProviderDbContext,
            Mock.Of<ILogger<DraftApprenticeshipSetReferenceCommandHandler>>());
    }
    public async Task Handle()
    {
        await Handler.Handle(Command, CancellationToken.None);
        await Db.SaveChangesAsync();
    }
    internal void VerifyReferenceUpdated()
    {
        Assert.That(Db.DraftApprenticeships.First(x => x.Id == DraftApprenticeshipId).ProviderRef, Is.EqualTo(Command.Reference));
    }
   
    public void Dispose()
    {
        Db?.Dispose();
        GC.SuppressFinalize(this);
    }
}


using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.CommitmentsV2.Application.Commands.Email;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
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
        using var fixture = new DraftApprenticeshipAddEmailCommandHandlerTestsFixture(email, Party.Provider);
        await fixture.Handle();
        fixture.VerifyEmailUpdated();
    }


    [Test]
    public async Task WhenHandlingDraftApprenticeshipAddEmailCommand_IfDomainExceptionIsReturned_Then_ThrowDomainException()
    {
        using var fixture = new DraftApprenticeshipAddEmailCommandHandlerTestsFixture("test", Party.Provider);
        var action = async () => await fixture.Handle();

        await action.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.Count() == 1);
    }

    [Test]
    public async Task WhenHandlingDraftApprenticeshipAddEmailCommand_ValidateWithParty()
    {
        using var fixture = new DraftApprenticeshipAddEmailCommandHandlerTestsFixture("Test1@email.com", Party.Employer);

        var action = async () => await fixture.Handle();

        await action.Should().ThrowAsync<DomainException>().Where(ex => ex.DomainErrors.First().ErrorMessage.Contains("cohort is not assigned to you"));
    }

    [Test]
    public async Task WhenValidatingApprenticeship_Is_Null()
    {
        using var fixture = new DraftApprenticeshipAddEmailCommandHandlerTestsFixture("Test1@email.com", Party.Provider);
        var action = async () => await fixture.Validate_DraftAppreticeship_Null();
        await action.Should().ThrowAsync<ApplicationException>().Where(ex => ex.Message.Contains("not found"));
    }

    [Test]
    public async Task WhenHandlingDraftApprenticeshipAddEmailCommand_Email_Is_Whitespace()
    {
        using var fixture = new DraftApprenticeshipAddEmailCommandHandlerTestsFixture(" ", Party.Provider);
        await fixture.Handle();
        fixture.VerifyEmailIfWhiteSpaceUpdated();
    }

    [Test]
    public async Task WhenValidatingCohort_Is_Null()
    {
        using var fixture = new DraftApprenticeshipAddEmailCommandHandlerTestsFixture("Test1@email.com", Party.Provider);
        var action = async () => await fixture.Validate_Cohort_Null();
        await action.Should().ThrowAsync<ApplicationException>().Where(ex => ex.Message.Contains("not found"));
    }

    [Test]
    public async Task WhenValidatingEmail_Is_Empty()
    {
        using var fixture = new DraftApprenticeshipAddEmailCommandHandlerTestsFixture("", Party.Provider);
        var action = await fixture.Validate();
        action.Errors.Count.Should().Be(0);
    }
   
    [Test]
    public async Task WhenValidatingEmailOverlap()
    {
        using var fixture = new DraftApprenticeshipAddEmailCommandHandlerTestsFixture("Test1@email.com", Party.Provider);
        var action = await fixture.ValidateOverlap();
        action.Errors.Count.Should().Be(1);
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

    public DraftApprenticeship draftApprenticeship { get; set; }
    public DraftApprenticeshipAddEmailCommandHandlerTestsFixture(string email, Party party)
    {        
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
           .Set(c => c.WithParty, party);

        draftApprenticeship = fixture.Build<CommitmentsV2.Models.DraftApprenticeship>()
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

        Db.DraftApprenticeships.Add(draftApprenticeship);

        Db.SaveChanges();

        DraftApprenticeshipId = draftApprenticeship.Id;

        var authenticationService = new Mock<IAuthenticationService>();
        authenticationService.Setup(x => x.GetUserParty()).Returns(() => Party);

        var lazyProviderDbContext = new Lazy<ProviderCommitmentsDbContext>(() => Db);

        Command = new DraftApprenticeshipAddEmailCommand
        {
            ApprenticeshipId = draftApprenticeship.Id,
            CohortId = Cohort.Id,
            Email = email,
            Party = Party.Provider
        };

        Handler = new DraftApprenticeshipAddEmailCommandHandler(lazyProviderDbContext,
            Mock.Of<ILogger<DraftApprenticeshipAddEmailCommandHandler>>(), OverlapCheckService.Object);
    }
    public async Task Handle()
    {
        await Handler.Handle(Command, CancellationToken.None);
        await Db.SaveChangesAsync();
    }

    public async Task Validate_DraftAppreticeship_Null()
    {
        await Handler.Validate(Command, null, CancellationToken.None);
    }

    public async Task Validate_Cohort_Null()
    {
        Command.CohortId = 123;
        await Handler.Validate(Command, draftApprenticeship, CancellationToken.None);
    }

    public async Task<ViewEditDraftApprenticeshipEmailValidationResult> Validate()
    {        
      return  await Handler.Validate(Command, draftApprenticeship, CancellationToken.None);
    }

    public async Task<ViewEditDraftApprenticeshipEmailValidationResult> ValidateOverlap()
    {
        OverlapCheckService.Setup(t => t.CheckForEmailOverlaps(It.IsAny<string>(), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), It.IsAny<long>(), It.IsAny<long>(), CancellationToken.None)).
            ReturnsAsync(new EmailOverlapCheckResult(1, OverlapStatus.DateWithin, false));
        return await Handler.Validate(Command, draftApprenticeship, CancellationToken.None);
    }

    internal void VerifyEmailUpdated()
    {
        Db.DraftApprenticeships.First(x => x.Id == DraftApprenticeshipId).Email.Should().Be(Command.Email);
    }

    internal void VerifyEmailIfWhiteSpaceUpdated()
    {
        Db.DraftApprenticeships.First(x => x.Id == DraftApprenticeshipId).Email.Should().BeNull();
    }

    public void Dispose()
    {
        Db?.Dispose();
        GC.SuppressFinalize(this);
    }
}

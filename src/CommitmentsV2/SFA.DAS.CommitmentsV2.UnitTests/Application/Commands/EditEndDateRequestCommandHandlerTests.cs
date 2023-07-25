using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeEndDateRequest;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    public class EditEndDateRequestCommandHandlerTests
    {

        [TestCase(Party.Provider)]
        [TestCase(Party.None)]
        [TestCase(Party.TransferSender)]
        public void WhenHandlingCommand_IfPartyIsNotEmployer_Then_ThrowDomainException(Party party)
        {
            var f = new EditEndDateRequestCommandHandlerTestsFixture();
            f.Party = party;

            Assert.ThrowsAsync<DomainException>(async () => await f.Handle());
        }

        [Test]
        public async Task WhenHandlingCommand_ShouldUpdateTheEndDate()
        {
            var f = new EditEndDateRequestCommandHandlerTestsFixture();
            await f.Handle();
            f.VerifyEndDateUpdated();
        }

        [Test]
        public async Task Handle_WhenHandlingCommand_UpdatingTheEndDate_ThenResolveOltd()
        {
            var f = new EditEndDateRequestCommandHandlerTestsFixture();
            await f.Handle();
            f.VerifyEndDateUpdated();

            f._resolveOverlappingTrainingDateRequestService
                .Verify(x => x.Resolve(f.ApprenticeshipId, null, Types.OverlappingTrainingDateRequestResolutionType.ApprenticeshipEndDateUpdate), Times.Once);
        }
    }
    public class EditEndDateRequestCommandHandlerTestsFixture
    {
        public EditEndDateRequestCommand Command { get; set; }
        public IRequestHandler<EditEndDateRequestCommand> Handler { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }
        public Party Party { get; set; }
        public long ApprenticeshipId { get; set; }

        public Mock<IResolveOverlappingTrainingDateRequestService> _resolveOverlappingTrainingDateRequestService;
        public EditEndDateRequestCommandHandlerTestsFixture()
        {
            Party = Party.Employer;
            UnitOfWorkContext = new UnitOfWorkContext();
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                                 .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                                 .Options);
            var fixture = new Fixture();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var Cohort = new CommitmentsV2.Models.Cohort()
               .Set(c => c.Id, 111)
               .Set(c => c.EmployerAccountId, 222)
               .Set(c => c.ProviderId, 333)
               .Set(c => c.AccountLegalEntity, new AccountLegalEntity());
            var Apprenticeship = fixture.Build<CommitmentsV2.Models.Apprenticeship>()
             .With(s => s.Cohort, Cohort)
             .With(s => s.PaymentStatus, PaymentStatus.Completed)
             .With(s => s.EndDate, DateTime.UtcNow)
             .With(s => s.CompletionDate, DateTime.UtcNow.AddDays(10))
             .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
             .Without(s => s.DataLockStatus)
             .Without(s => s.EpaOrg)
             .Without(s => s.ApprenticeshipUpdate)
             .Without(s => s.Continuation)
             .Without(s => s.PreviousApprenticeship)
             .Without(s => s.ApprenticeshipConfirmationStatus)
             .Create();

            Db.Apprenticeships.Add(Apprenticeship);

            Db.SaveChanges();

            ApprenticeshipId = Apprenticeship.Id;

            var authenticationService = new Mock<IAuthenticationService>();
            authenticationService.Setup(x => x.GetUserParty()).Returns(() => Party);

            var lazyProviderDbContext = new Lazy<ProviderCommitmentsDbContext>(() => Db);

            var newEndDate = Apprenticeship.EndDate.Value.AddDays(1);

            Command = new EditEndDateRequestCommand
            {
                ApprenticeshipId = Apprenticeship.Id,
                EndDate = newEndDate,
                UserInfo = new UserInfo()
            };

            _resolveOverlappingTrainingDateRequestService = new Mock<IResolveOverlappingTrainingDateRequestService>();

            _resolveOverlappingTrainingDateRequestService
                .Setup(x => x.Resolve(Apprenticeship.Id, null,
                    Types.OverlappingTrainingDateRequestResolutionType.ApprenticeshipEndDateUpdate))
                .Returns(Task.CompletedTask);

            Handler = new EditEndDateRequestCommandHandler(lazyProviderDbContext,
                Mock.Of<ICurrentDateTime>(),
                authenticationService.Object,
                _resolveOverlappingTrainingDateRequestService.Object);
        }
        public async Task Handle()
        {
            await Handler.Handle(Command, CancellationToken.None);
            await Db.SaveChangesAsync();
        }
        internal void VerifyEndDateUpdated()
        {
            Assert.AreEqual(Command.EndDate, Db.Apprenticeships.First(x => x.Id == ApprenticeshipId).EndDate);
        }
    }
}

using AutoFixture;
using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeEndDateRequest;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    public class UpdateEndDateOfCompletedRecordRequestCommandHandlerTests
    {
        [TestCase(Party.Provider)]
        [TestCase(Party.None)]
        [TestCase(Party.TransferSender)]
        public void WhenHandlingCommand_IfPartyIsNotEmployer_Then_ThrowDomainException(Party party)
        {
            var f = new UpdateEndDateOfCompletedRecordCommandHandlerTestsFixture();
            f.SetParty(party);

            Assert.ThrowsAsync<DomainException>(async () => await f.Handle()); 
        }

        //[Test]
        //public async Task WhenHandlingCommand_ShouldUpdateTheEndDate()
        //{
        //    var f = new UpdateEndDateOfCompletedRecordCommandHandlerTestsFixture();
        //    await f.Handle();
        //    f.VerifyEndDateUpdated();
        //}
    }

    public class UpdateEndDateOfCompletedRecordCommandHandlerTestsFixture
    {
        public UpdateEndDateOfCompletedRecordRequestCommand Command { get; set; }
        public IRequestHandler<UpdateEndDateOfCompletedRecordRequestCommand> Handler { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }

        public Party Party { get; set; }

        public UpdateEndDateOfCompletedRecordCommandHandlerTestsFixture()
        {
            Party = Party.Employer;
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                                 .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                                 .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                                                 .Options);
            var Apprenticeship = ObjectActivator.CreateInstance<Apprenticeship>()
                .Set(a => a.Id, 11)
                .Set(a => a.EndDate, DateTime.UtcNow)
                .Set(a => a.StartDate, DateTime.UtcNow.AddDays(-10))
                .Set(a => a.CompletionDate, DateTime.UtcNow.AddDays(10))
                .Set(a => a.PaymentStatus, PaymentStatus.Completed);

            //Apprenticeship.StartDate = Apprenticeship.EndDate.Value.AddDays(-10);
            //Apprenticeship.CompletionDate = Apprenticeship.EndDate.Value.AddDays(10);
            //Apprenticeship.PaymentStatus = PaymentStatus.Completed;

            Db.Apprenticeships.Add(Apprenticeship);

            Db.SaveChanges();

            var authenticationService = new Mock<IAuthenticationService>();
            authenticationService.Setup(x => x.GetUserParty()).Returns(() => Party);

            var lazyProviderDbContext = new Lazy<ProviderCommitmentsDbContext>(() => Db);

            var newEndDate = Apprenticeship.EndDate.Value.AddDays(1);

            Command = new UpdateEndDateOfCompletedRecordRequestCommand
            {
                ApprenticeshipId = Apprenticeship.Id,
                EndDate = newEndDate,
                UserInfo = new UserInfo()
            };

            Handler = new UpdateEndDateOfCompletedRecordRequestCommandHandler(lazyProviderDbContext,
                Mock.Of<ICurrentDateTime>(),
                authenticationService.Object,
                Mock.Of<ILogger<UpdateEndDateOfCompletedRecordRequestCommandHandler>>());
        }

        public async Task Handle()
        {
            await Handler.Handle(Command, CancellationToken.None);
            await Db.SaveChangesAsync();
        }

        internal void SetParty(Party party)
        {
            Party = party;
        }

        internal void VerifyEndDateUpdated()
        {
            Assert.AreEqual(Command.EndDate, Db.Apprenticeships.First(x => x.Id == 11).EndDate);
        }
    }
}

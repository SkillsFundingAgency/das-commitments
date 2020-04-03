﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateLevyStatusToLevy;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class UpdateLevyStatusToLevyCommandHandlerTests
    {
        [Test]
        public void Handle_WhenHandlingCommand_ThenShouldUpdateTheLevyStatus()
        {
            var f = new UpdateLevyStatusToLevyCommandHandlerTestsFixture();
            f.SetAccount()
                .Handle();

            Assert.IsTrue(f.IsValid(ApprenticeshipEmployerType.Levy));
        }

        [Test]
        public void Handle_WhenHandlingCommand_AndAccountNotFound_ThenShouldnotUpdateTheLevyStatus()
        {
            var f = new UpdateLevyStatusToLevyCommandHandlerTestsFixture();
            f.SetAccount();
            f.Command.AccountId = 2;
            f.Handle();

            Assert.IsTrue(f.IsValid(ApprenticeshipEmployerType.NonLevy));
        }
    }

    public class UpdateLevyStatusToLevyCommandHandlerTestsFixture
    {
        public IFixture AutoFixture { get; set; }
        public UpdateLevyStatusToLevyCommand Command { get; set; }
        public Mock<ProviderCommitmentsDbContext> Db { get; set; }
        public IRequestHandler<UpdateLevyStatusToLevyCommand> Handler { get; set; }
        public long AccountId { get; set; }

        public UpdateLevyStatusToLevyCommandHandlerTestsFixture()
        {
            AutoFixture = new Fixture();
            AccountId = 1;
            Command = new UpdateLevyStatusToLevyCommand { AccountId = AccountId };
            Db = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options) { CallBase = true };
            Handler = new UpdateLevyStatusToLevyCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db.Object), Mock.Of<ILogger<UpdateLevyStatusToLevyCommandHandler>>());

            AutoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        public Task Handle()
        {
            return Handler.Handle(Command, CancellationToken.None);
        }

        public UpdateLevyStatusToLevyCommandHandlerTestsFixture SetAccount()
        {
            var account = new Account(AccountId, "", "", "", DateTime.UtcNow);

            Db.Object.Accounts.Add(account);
            Db.Object.SaveChanges();

            return this;
        }

        public bool IsValid(ApprenticeshipEmployerType levyStatus)
        {
            return levyStatus == Db.Object.Accounts.First().LevyStatus;
        }
    }
}
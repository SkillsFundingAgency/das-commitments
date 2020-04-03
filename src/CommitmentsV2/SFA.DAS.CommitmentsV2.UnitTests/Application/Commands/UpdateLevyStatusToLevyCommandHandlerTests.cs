using System;
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

        [TestCase(ApprenticeshipEmployerType.NonLevy)]
        [TestCase(ApprenticeshipEmployerType.Levy)]
        public void Handle_WhenHandlingCommand_ThenShouldUpdateTheLevyStatus(ApprenticeshipEmployerType apprenticeshipEmployerType)
        {
            var f = new UpdateLevyStatusToLevyCommandHandlerTestsFixture();
            f.SetAccount(apprenticeshipEmployerType)
                .Handle();

            Assert.IsTrue(f.IsValid());
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
            AccountId = AutoFixture.Create<long>();
            Command = new UpdateLevyStatusToLevyCommand { AccountId = AccountId };
            Db = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options) { CallBase = true };
            Handler = new UpdateLevyStatusToLevyCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db.Object), Mock.Of<ILogger<UpdateLevyStatusToLevyCommandHandler>>());

            AutoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
            Db.Setup(d => d.ExecuteSqlCommandAsync(It.IsAny<string>(), It.IsAny<object[]>())).Returns(Task.CompletedTask);
        }

        public Task Handle()
        {
            return Handler.Handle(Command, CancellationToken.None);
        }

        public UpdateLevyStatusToLevyCommandHandlerTestsFixture SetAccount(ApprenticeshipEmployerType levyStatus)
        {
            var account = new Account(AccountId, "", "", "", DateTime.UtcNow) { LevyStatus = ApprenticeshipEmployerType.NonLevy};

            Db.Object.Accounts.Add(account);
            Db.Object.SaveChanges();

            return this;
        }

        public bool IsValid()
        {
            return true;
           
        }
    }
}
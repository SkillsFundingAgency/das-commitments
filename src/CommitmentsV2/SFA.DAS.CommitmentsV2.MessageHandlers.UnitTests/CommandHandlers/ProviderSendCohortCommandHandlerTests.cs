using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Moq;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.CommandHandlers
{
    [TestFixture]
    public class ProviderSendCohortCommandHandlerTests
    {
        public ProviderSendCohortCommandHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ProviderSendCohortCommandHandlerTestsFixture();
        }

        [Test]
        public async Task When_HandlingCommand_CohortIsSentToOtherParty()
        {
            await _fixture.Handle();
            _fixture.VerifyCohortIsSentToOtherParty();
        }

        public class ProviderSendCohortCommandHandlerTestsFixture
        {
            private ProviderSendCohortCommandHandler _handler;
            private ProviderSendCohortCommand _command;
            public Mock<ProviderCommitmentsDbContext> _dbContext { get; set; }
            private Mock<IMessageHandlerContext> _messageHandlerContext;
            private Mock<Cohort> _cohort;

            public ProviderSendCohortCommandHandlerTestsFixture()
            {
                var autoFixture = new Fixture();

                _dbContext = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options) { CallBase = true };

                _handler = new ProviderSendCohortCommandHandler(Mock.Of<ILogger<ProviderSendCohortCommandHandler>>(),
                    new Lazy<ProviderCommitmentsDbContext>(() => _dbContext.Object));

                _messageHandlerContext = new Mock<IMessageHandlerContext>();

                _command = autoFixture.Create<ProviderSendCohortCommand>();

                _cohort = new Mock<Cohort>();
                _cohort.Setup(x => x.Id).Returns(_command.CohortId);
                _cohort.Setup(x => x.IsApprovedByAllParties).Returns(false);
                _cohort.Setup(x =>
                    x.SendToOtherParty(Party.Provider, It.IsAny<string>(), It.IsAny<UserInfo>(), It.IsAny<DateTime>()));

                _dbContext.Object.Cohorts.Add(_cohort.Object);
                _dbContext.Object.SaveChanges();
            }

            public async Task Handle()
            {
                await _handler.Handle(_command, _messageHandlerContext.Object);
            }

            public void VerifyCohortIsSentToOtherParty()
            {
                _cohort.Verify(x => x.SendToOtherParty(Party.Provider, It.IsAny<string>(), It.IsAny<UserInfo>(), It.IsAny<DateTime>()), Times.Once);
            }
        }
    }
}

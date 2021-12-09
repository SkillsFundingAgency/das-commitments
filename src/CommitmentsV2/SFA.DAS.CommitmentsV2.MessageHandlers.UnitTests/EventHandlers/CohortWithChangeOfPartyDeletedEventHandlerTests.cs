using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class CohortWithChangeOfPartyDeletedEventHandlerTests
    {
        private CohortWithChangeOfPartyDeletedEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CohortWithChangeOfPartyDeletedEventHandlerTestsFixture();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public async Task Handle_WhenHandlingEvent_If_Deleted_By_Originator_Then_Request_Is_Withdrawn(Party party)
        {
            await _fixture.WithDeletionByParty(party)
                .WithRequestOriginator(party)
                .Handle();

            _fixture.VerifyWithdrawn();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]

        public async Task Handle_WhenHandlingEvent_If_Deleted_By_OtherParty_Then_Request_Is_Rejected(Party party)
        {
            await _fixture.WithDeletionByParty(party.GetOtherParty())
                .WithRequestOriginator(party)
                .Handle();

            _fixture.VerifyRejected();
        }

        private class CohortWithChangeOfPartyDeletedEventHandlerTestsFixture
        {
            private readonly CohortWithChangeOfPartyDeletedEventHandler _handler;
            private readonly Mock<ProviderCommitmentsDbContext> _db;
            private readonly Cohort _cohort;
            private readonly Mock<IMessageHandlerContext> _messageHandlerContext;
            private readonly Mock<ChangeOfPartyRequest> _changeOfPartyRequest;
            private readonly CohortWithChangeOfPartyDeletedEvent _event;

            public CohortWithChangeOfPartyDeletedEventHandlerTestsFixture()
            {
                var autoFixture = new Fixture();

                _event = autoFixture.Create<CohortWithChangeOfPartyDeletedEvent>();

                _cohort = new Cohort();
                _cohort.SetValue(x => x.Id, _event.CohortId);
                _cohort.SetValue(x => x.Approvals, Party.None);
                _cohort.SetValue(x => x.WithParty, Party.Employer);

                _changeOfPartyRequest = new Mock<ChangeOfPartyRequest>();
                _changeOfPartyRequest.Setup(x => x.Id).Returns(_event.ChangeOfPartyRequestId);
                _changeOfPartyRequest.Setup(x => x.SetCohort(_cohort, _event.UserInfo));
                _changeOfPartyRequest.Setup(x => x.Withdraw(It.IsAny<Party>(), It.IsAny<UserInfo>()));
                _changeOfPartyRequest.Setup(x => x.Reject(It.IsAny<Party>(), It.IsAny<UserInfo>()));

                _db = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options) { CallBase = true };

                _db
                    .Setup(context => context.Cohorts)
                    .ReturnsDbSet(new List<Cohort> { _cohort });

                _db
                    .Setup(context => context.ChangeOfPartyRequests)
                    .ReturnsDbSet(new List<ChangeOfPartyRequest> { _changeOfPartyRequest.Object });


                _messageHandlerContext = new Mock<IMessageHandlerContext>();

                _handler = new CohortWithChangeOfPartyDeletedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db.Object), Mock.Of<ILogger<CohortWithChangeOfPartyDeletedEventHandler>>());
            }

            public CohortWithChangeOfPartyDeletedEventHandlerTestsFixture WithDeletionByParty(Party party)
            {
                _event.SetValue(x => x.DeletedBy, party);
                return this;
            }

            public CohortWithChangeOfPartyDeletedEventHandlerTestsFixture WithRequestOriginator(Party party)
            {
                _changeOfPartyRequest.Setup(x => x.OriginatingParty).Returns(party);
                return this;
            }

            public async Task Handle()
            {
                await _handler.Handle(_event, _messageHandlerContext.Object);
                _db.Object.SaveChanges();
            }

            public void VerifyWithdrawn()
            {
                _changeOfPartyRequest.Verify(x => x.Withdraw(_event.DeletedBy,_event.UserInfo), Times.Once);
            }

            public void VerifyRejected()
            {
                _changeOfPartyRequest.Verify(x => x.Reject(_event.DeletedBy, _event.UserInfo), Times.Once);
            }
        }
    }
}

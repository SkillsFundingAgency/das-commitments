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
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class CohortWithChangeOfPartyFullyApprovedEventHandlerTests
    {
        private CohortWithChangeOfPartyFullyApprovedEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CohortWithChangeOfPartyFullyApprovedEventHandlerTestsFixture();
        }

        [Test]
        public async Task Handle_WhenHandlingEvent_Request_Is_Approved()
        {
            await _fixture.Handle();
            _fixture.VerifyApproved();
        }

        [Test]
        public async Task Handle_WhenHandlingEvent_If_Request_Is_Already_Approved_Then_Does_Nothing()
        {
            _fixture.WithRequestAlreadyApproved();
            await _fixture.Handle();
            _fixture.VerifyNoStateChange();
        }

        private class CohortWithChangeOfPartyFullyApprovedEventHandlerTestsFixture
        {
            private readonly CohortWithChangeOfPartyFullyApprovedEventHandler _handler;
            private readonly Mock<ProviderCommitmentsDbContext> _db;
            private readonly Cohort _cohort;
            private readonly Mock<IMessageHandlerContext> _messageHandlerContext;
            private readonly Mock<ChangeOfPartyRequest> _changeOfPartyRequest;
            private readonly CohortWithChangeOfPartyFullyApprovedEvent _event;

            public CohortWithChangeOfPartyFullyApprovedEventHandlerTestsFixture()
            {
                var autoFixture = new Fixture();

                _event = autoFixture.Create<CohortWithChangeOfPartyFullyApprovedEvent>();

                _cohort = new Cohort();
                _cohort.SetValue(x => x.Id, _event.CohortId);
                _cohort.SetValue(x => x.Approvals, Party.None);
                _cohort.SetValue(x => x.WithParty, Party.Employer);

                _changeOfPartyRequest = new Mock<ChangeOfPartyRequest>();
                _changeOfPartyRequest.Setup(x => x.Id).Returns(_event.ChangeOfPartyRequestId);
                _changeOfPartyRequest.Setup(x => x.Approve(It.IsAny<Party>(), It.IsAny<UserInfo>()));

                _db = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options) { CallBase = true };

                _db
                    .Setup(context => context.Cohorts)
                    .ReturnsDbSet(new List<Cohort> { _cohort });

                _db
                    .Setup(context => context.ChangeOfPartyRequests)
                    .ReturnsDbSet(new List<ChangeOfPartyRequest> { _changeOfPartyRequest.Object });


                _messageHandlerContext = new Mock<IMessageHandlerContext>();

                _handler = new CohortWithChangeOfPartyFullyApprovedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db.Object), Mock.Of<ILogger<CohortWithChangeOfPartyFullyApprovedEventHandler>>());
            }

            public CohortWithChangeOfPartyFullyApprovedEventHandlerTestsFixture WithRequestAlreadyApproved()
            {
                _changeOfPartyRequest.Setup(x => x.Status).Returns(ChangeOfPartyRequestStatus.Approved);
                return this;
            }

            public async Task Handle()
            {
                await _handler.Handle(_event, _messageHandlerContext.Object);
                _db.Object.SaveChanges();
            }

            public void VerifyApproved()
            {
                _changeOfPartyRequest.Verify(x => x.Approve(_event.ApprovedBy, _event.UserInfo), Times.Once);
            }

            public void VerifyNoStateChange()
            {
                _changeOfPartyRequest.Verify(x => x.Approve(It.IsAny<Party>(), It.IsAny<UserInfo>()), Times.Never);
            }
        }
    }
}

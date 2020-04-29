using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class CohortWithChangeOfPartyCreatedEventHandlerTests
    {
        private CohortWithChangeOfPartyCreatedEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CohortWithChangeOfPartyCreatedEventHandlerTestsFixture();
        }

        [Test]
        public async Task Handle_WhenHandlingEvent_ChangeOfParty_CohortId_Is_Set()
        {
            await _fixture.Handle();
            _fixture.VerifyCohortIsSet();
        }

        private class CohortWithChangeOfPartyCreatedEventHandlerTestsFixture
        {
            private readonly CohortWithChangeOfPartyCreatedEventHandler _handler;
            private readonly ProviderCommitmentsDbContext _db;
            private readonly CohortWithChangeOfPartyCreatedEvent _event;
            private readonly Cohort _cohort;
            private readonly Mock<IMessageHandlerContext> _messageHandlerContext;
            private readonly Mock<ChangeOfPartyRequest> _changeOfPartyRequest;

            public CohortWithChangeOfPartyCreatedEventHandlerTestsFixture()
            {
                var autoFixture = new Fixture();

                _event = autoFixture.Create<CohortWithChangeOfPartyCreatedEvent>();

                _cohort = new Cohort();
                _cohort.SetValue(x => x.Id, _event.CohortId);
                _cohort.SetValue(x => x.Approvals, Party.None);
                _cohort.SetValue(x => x.WithParty, Party.Employer);

                _changeOfPartyRequest = new Mock<ChangeOfPartyRequest>();
                _changeOfPartyRequest.Setup(x => x.Id).Returns(_event.ChangeOfPartyRequestId);
                _changeOfPartyRequest.Setup(x => x.SetCohort(_cohort));

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                    .Options);

                _db.Cohorts.Add(_cohort);
                _db.ChangeOfPartyRequests.Add(_changeOfPartyRequest.Object);
                _db.SaveChanges();

                _messageHandlerContext = new Mock<IMessageHandlerContext>();

                _handler = new CohortWithChangeOfPartyCreatedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            }

            public async Task Handle()
            {
                await _handler.Handle(_event, _messageHandlerContext.Object);
                _db.SaveChanges();
            }

            public void VerifyCohortIsSet()
            {
                _changeOfPartyRequest.Verify(x => x.SetCohort(_cohort), Times.Once);
            }
        }
    }
}

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.CommandHandlers
{
    public class UpdateChangeOfPartyRequestEventHandlerTests
    {
        public UpdateChangeOfPartyRequestEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new UpdateChangeOfPartyRequestEventHandlerTestsFixture();
        }

        [Test]
        public async Task When_HandlingCommand_And_IsChangeOfProviderRequest_Then_ChangeOfPartyRequestIsUpdated()
        {
            await _fixture.Handle();

            _fixture.VerifyChangeOfPartyUpdated();
        }

        public class UpdateChangeOfPartyRequestEventHandlerTestsFixture
        {
            private UpdateChangeOfPartyRequestEventHandler _handler;
            private UpdateChangeOfPartyRequestEvent _command;
            private Mock<ProviderCommitmentsDbContext> _mockDbContext;
            private Cohort _cohort;
            private DraftApprenticeship _draftApprenticeship;
            private Mock<ChangeOfPartyRequest> _changeOfPartyRequest;
            private Mock<IMessageHandlerContext> _messageHandlerContext;

            private DateTime _startDate = DateTime.Today;
            private DateTime _endDate = DateTime.Today.AddYears(2);
            private decimal? _cost = 100.00m;

            public UpdateChangeOfPartyRequestEventHandlerTestsFixture()
            {
                var autoFixture = new Fixture();

                _command = autoFixture.Build<UpdateChangeOfPartyRequestEvent>()
                    .Create();

                _draftApprenticeship = new DraftApprenticeship
                {
                    StartDate = _startDate,
                    EndDate = _endDate,
                    Cost = _cost
                };

                _cohort = new Cohort
                {
                    Id = _command.CohortId,
                    Apprenticeships = new List<ApprenticeshipBase> { _draftApprenticeship },
                    ChangeOfPartyRequestId = 123,
                    WithParty = Party.Provider
                };

                _changeOfPartyRequest = new Mock<ChangeOfPartyRequest>();
                _changeOfPartyRequest.Setup(x => x.Id).Returns(_cohort.ChangeOfPartyRequestId.Value);
                _changeOfPartyRequest.Setup(x => x.ChangeOfPartyType).Returns(ChangeOfPartyRequestType.ChangeProvider);

                _mockDbContext = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options) { CallBase = true };

                _mockDbContext.Object.Cohorts.Add(_cohort);
                _mockDbContext.Object.ChangeOfPartyRequests.Add(_changeOfPartyRequest.Object);
                _mockDbContext.Object.SaveChanges();

                _messageHandlerContext = new Mock<IMessageHandlerContext>();

                _handler = new UpdateChangeOfPartyRequestEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _mockDbContext.Object));
            }

            public async Task Handle()
            {
                await _handler.Handle(_command, _messageHandlerContext.Object);
            }

            public void VerifyChangeOfPartyUpdated()
            {
                _changeOfPartyRequest.Verify(x => x.UpdateChangeOfPartyRequest(_draftApprenticeship, _cohort.EmployerAccountId, _cohort.ProviderId, It.IsAny<UserInfo>(), _cohort.WithParty), Times.Once);
            }
        }
    }
}

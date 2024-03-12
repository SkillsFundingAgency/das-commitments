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
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.CommitmentsV2.TestHelpers;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
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
            _fixture.AddChangeOfProviderRequest();

            await _fixture.Handle();

            _fixture.VerifyChangeOfPartyUpdated();
        }

        [Test]
        public async Task When_HandlingCommand_And_IsChangeOfEmployerRequest_Then_ChangeOfPartyRequestIsNotUpdated()
        {
            _fixture.AddChangeOfEmployerRequest();

            await _fixture.Handle();

            _fixture.VerifyChangeOfPartyNotUpdated();
        }

        [Test]
        public async Task When_HandlingCommand_And_Cohort_Is_Already_Fully_Approved_Then_ChangeOfPartyRequestIsNotUpdated_And_Message_Is_Swallowed()
        {
            _fixture.AddChangeOfProviderRequest()
                .AddFullyApprovedCohort();

            await _fixture.Handle();

            _fixture.VerifyChangeOfPartyNotUpdated();
        }

        public class UpdateChangeOfPartyRequestEventHandlerTestsFixture
        {
            private CohortWithChangeOfPartyUpdatedEventHandler _handler;
            private CohortWithChangeOfPartyUpdatedEvent _command;
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

                _command = autoFixture.Build<CohortWithChangeOfPartyUpdatedEvent>()
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
                
                _mockDbContext = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options) { CallBase = true };

                _mockDbContext
                    .Setup(context => context.Cohorts)
                    .ReturnsDbSet(new List<Cohort> { _cohort });

                _mockDbContext
                    .Setup(context => context.ChangeOfPartyRequests)
                    .ReturnsDbSet(new List<ChangeOfPartyRequest> { _changeOfPartyRequest.Object });


                _messageHandlerContext = new Mock<IMessageHandlerContext>();

                _handler = new CohortWithChangeOfPartyUpdatedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _mockDbContext.Object), Mock.Of<ILogger<CohortWithChangeOfPartyUpdatedEventHandler>>());
            }

            public UpdateChangeOfPartyRequestEventHandlerTestsFixture AddChangeOfProviderRequest()
            {
                _changeOfPartyRequest.Setup(x => x.Id).Returns(_cohort.ChangeOfPartyRequestId.Value);
                _changeOfPartyRequest.Setup(x => x.ChangeOfPartyType).Returns(ChangeOfPartyRequestType.ChangeProvider);

                _mockDbContext.Object.ChangeOfPartyRequests.Add(_changeOfPartyRequest.Object);
                _mockDbContext.Object.SaveChanges();

                return this;
            }

            public UpdateChangeOfPartyRequestEventHandlerTestsFixture AddFullyApprovedCohort()
            {
                _cohort.SetValue(x => x.WithParty, null); 
                return this;
            }

            public UpdateChangeOfPartyRequestEventHandlerTestsFixture AddChangeOfEmployerRequest()
            {
                _changeOfPartyRequest.Setup(x => x.Id).Returns(_cohort.ChangeOfPartyRequestId.Value);
                _changeOfPartyRequest.Setup(x => x.ChangeOfPartyType).Returns(ChangeOfPartyRequestType.ChangeEmployer);

                _mockDbContext.Object.ChangeOfPartyRequests.Add(_changeOfPartyRequest.Object);
                _mockDbContext.Object.SaveChanges();

                return this;
            }
            public async Task Handle()
            {
                await _handler.Handle(_command, _messageHandlerContext.Object);
            }

            public void VerifyChangeOfPartyUpdated()
            {
                _changeOfPartyRequest.Verify(x => x.UpdateChangeOfPartyRequest(_draftApprenticeship, _cohort.EmployerAccountId, _cohort.ProviderId, It.IsAny<UserInfo>(), _cohort.WithParty), Times.Once);
            }

            public void VerifyChangeOfPartyNotUpdated()
            {
                _changeOfPartyRequest.Verify(x => x.UpdateChangeOfPartyRequest(It.IsAny<DraftApprenticeship>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<UserInfo>(), It.IsAny<Party>()), Times.Never);
            }
        }
    }
}

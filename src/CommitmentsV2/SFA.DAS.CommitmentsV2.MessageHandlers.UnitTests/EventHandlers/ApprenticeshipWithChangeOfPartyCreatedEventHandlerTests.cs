using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
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
    public class ApprenticeshipWithChangeOfPartyCreatedEventHandlerTests
    {
        private ApprenticeshipWithChangeOfPartyCreatedEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ApprenticeshipWithChangeOfPartyCreatedEventHandlerTestsFixture();
        }

        [Test]
        public async Task Handle_WhenHandlingEvent_ChangeOfParty_CohortId_Is_Set()
        {
            await _fixture.Handle();
            _fixture.VerifyNewApprenticeshipIsSet();
        }

        [Test]
        public async Task Handle_WhenHandlingEvent_If_CohortId_Already_Set_Then_It_Is_Not_Updated()
        {
            _fixture.WithNewApprenticeshipIdAlreadySet();
            await _fixture.Handle();
            _fixture.VerifyNewApprenticeshipIsNotUpdated();
        }

        private class ApprenticeshipWithChangeOfPartyCreatedEventHandlerTestsFixture
        {
            private readonly ApprenticeshipWithChangeOfPartyCreatedEventHandler _handler;
            private readonly ProviderCommitmentsDbContext _db;
            private readonly ApprenticeshipWithChangeOfPartyCreatedEvent _event;
            private readonly Apprenticeship _apprenticeship;
            private readonly Mock<IMessageHandlerContext> _messageHandlerContext;
            private readonly Mock<ChangeOfPartyRequest> _changeOfPartyRequest;

            public ApprenticeshipWithChangeOfPartyCreatedEventHandlerTestsFixture()
            {
                var autoFixture = new Fixture();

                _event = autoFixture.Create<ApprenticeshipWithChangeOfPartyCreatedEvent>();

                _apprenticeship = new Apprenticeship();
                _apprenticeship.SetValue(x => x.Id, _event.ApprenticeshipId);
                _apprenticeship.SetValue(x => x.Cohort, new Cohort
                {
                    AccountLegalEntity = new AccountLegalEntity()
                });

                _changeOfPartyRequest = new Mock<ChangeOfPartyRequest>();
                _changeOfPartyRequest.Setup(x => x.Id).Returns(_event.ChangeOfPartyRequestId);
                _changeOfPartyRequest.Setup(x => x.SetNewApprenticeship(_apprenticeship, _event.UserInfo, Party.Employer));

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options);

                _db.Apprenticeships.Add(_apprenticeship);
                _db.ChangeOfPartyRequests.Add(_changeOfPartyRequest.Object);
                _db.SaveChanges();

                _messageHandlerContext = new Mock<IMessageHandlerContext>();

                _handler = new ApprenticeshipWithChangeOfPartyCreatedEventHandler(
                    new Lazy<ProviderCommitmentsDbContext>(() => _db), Mock.Of<ILogger<ApprenticeshipWithChangeOfPartyCreatedEventHandler>>());
            }

            public ApprenticeshipWithChangeOfPartyCreatedEventHandlerTestsFixture WithNewApprenticeshipIdAlreadySet()
            {
                _changeOfPartyRequest.Setup(x => x.NewApprenticeshipId).Returns(123);
                return this;
            }

            public async Task Handle()
            {
                await _handler.Handle(_event, _messageHandlerContext.Object);
                _db.SaveChanges();
            }

            public void VerifyNewApprenticeshipIsSet()
            {
                _changeOfPartyRequest.Verify(x => x.SetNewApprenticeship(_apprenticeship, _event.UserInfo, _event.LastApprovedBy), Times.Once);
            }

            public void VerifyNewApprenticeshipIsNotUpdated()
            {
                _changeOfPartyRequest.Verify(x => x.SetNewApprenticeship(It.IsAny<Apprenticeship>(), It.IsAny<UserInfo>(), It.IsAny<Party>()), Times.Never);
            }
        }
    }
}

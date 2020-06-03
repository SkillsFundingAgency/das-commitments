using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class ChangeOfPartyRequestCreatedEventHandlerTests
    {
        private ChangeOfPartyRequestCreatedEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ChangeOfPartyRequestCreatedEventHandlerTestsFixture();
        }

        [Test]
        public async Task Handle_WhenHandlingEvent_CohortIsCreatedWithChangeOfPartyReservation()
        {
            await _fixture.Handle();
            _fixture.VerifyReservation();
        }

        [Test]
        public async Task Handle_WhenHandlingEvent_CohortIsCreatedWithPreviousApprenticeDetails()
        {
            await _fixture.Handle();
            _fixture.VerifyApprenticeship();
        }

        [Test]
        public async Task Handle_WhenHandlingEvent_CohortIsCreated()
        {
            await _fixture.Handle();
            _fixture.VerifyCohortCreated();
        }

        [Test]
        public async Task Handle_WhenHandlingEvent_CohortIsCreatedWithCorrectReference()
        {
            await _fixture.Handle();
            _fixture.VerifyCohortReference();
        }

        [Test]
        public async Task Handle_WhenHandlingEvent_NullReservationIdPropagatesToNewRecord()
        {
            _fixture.WithNoReservationId();
            await _fixture.Handle();
            _fixture.VerifyNullReservation();
        }

        private class ChangeOfPartyRequestCreatedEventHandlerTestsFixture
        {
            public ChangeOfPartyRequestCreatedEventHandler Handler { get; private set; }
            public ChangeOfPartyRequestCreatedEvent Event { get; private set; }
            public Mock<IMessageHandlerContext> MessageHandlerContext { get; private set; }
            public Mock<IReservationsApiClient> ReservationsApiClient { get; private set; }
            public Mock<IEncodingService> EncodingService { get; }
            public ProviderCommitmentsDbContext Db { get; set; }
            public Guid ChangeOfPartyReservationId { get; set; }
            public string CohortReference { get; set; }
            public Mock<ChangeOfPartyRequest> ChangeOfPartyRequest { get; private set; }
            public Apprenticeship Apprenticeship { get; private set; }
            public Cohort Cohort { get; }

            public ChangeOfPartyRequestCreatedEventHandlerTestsFixture()
            {
                var autoFixture = new Fixture();

                Event = autoFixture.Create<ChangeOfPartyRequestCreatedEvent>();

                Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                    .Options);

                Cohort = new Cohort();
                Cohort.SetValue(x => x.Id, autoFixture.Create<long>());

                var apprenticeshipId = autoFixture.Create<long>();

                ChangeOfPartyRequest = new Mock<ChangeOfPartyRequest>();
                ChangeOfPartyRequest.Setup(x => x.Id).Returns(Event.ChangeOfPartyRequestId);
                ChangeOfPartyRequest.Setup(x => x.ChangeOfPartyType).Returns(ChangeOfPartyRequestType.ChangeEmployer);
                ChangeOfPartyRequest.Setup(x => x.ApprenticeshipId).Returns(apprenticeshipId);
                ChangeOfPartyRequest.Setup(x => x.AccountLegalEntityId).Returns(autoFixture.Create<long>());
                ChangeOfPartyRequest.Setup(x => x.ProviderId).Returns(autoFixture.Create<long>());
                ChangeOfPartyRequest.Setup(x => x.CreateCohort(It.IsAny<Apprenticeship>(), It.IsAny<Guid?>(), It.IsAny<UserInfo>())).Returns(Cohort);

                Apprenticeship = new Apprenticeship
                {
                    Id = apprenticeshipId,
                    Cohort = new Cohort(),
                    ChangeOfPartyRequests = new List<ChangeOfPartyRequest> {ChangeOfPartyRequest.Object},
                    ReservationId = autoFixture.Create<Guid>()
                };
                ChangeOfPartyRequest.Setup(x => x.Apprenticeship).Returns(Apprenticeship);

                Db.ChangeOfPartyRequests.Add(ChangeOfPartyRequest.Object);
                Db.Apprenticeships.Add(Apprenticeship);
                Db.SaveChanges();

                MessageHandlerContext = new Mock<IMessageHandlerContext>();
                ReservationsApiClient = new Mock<IReservationsApiClient>();
                EncodingService = new Mock<IEncodingService>();

                ChangeOfPartyReservationId = autoFixture.Create<Guid>();
                ReservationsApiClient.Setup(x =>
                        x.CreateChangeOfPartyReservation(It.Is<Guid>(id => id == Apprenticeship.ReservationId.Value),
                            It.IsAny<CreateChangeOfPartyReservationRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new CreateChangeOfPartyReservationResult(ChangeOfPartyReservationId));

                CohortReference = autoFixture.Create<string>();
                EncodingService.Setup(x => x.Encode(Cohort.Id, EncodingType.CohortReference)).Returns(CohortReference);

                Handler = new ChangeOfPartyRequestCreatedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db), ReservationsApiClient.Object, Mock.Of<ILogger<ChangeOfPartyRequestCreatedEventHandler>>(), EncodingService.Object);
            }

            public ChangeOfPartyRequestCreatedEventHandlerTestsFixture WithNoReservationId()
            {
                Apprenticeship.SetValue(x => x.ReservationId, null);
                return this;
            }

            public async Task Handle()
            {
                await Handler.Handle(Event, MessageHandlerContext.Object);
                Db.SaveChanges();
            }

            public void VerifyReservation()
            {
                ChangeOfPartyRequest.Verify(x => x.CreateCohort(It.IsAny<Apprenticeship>(),
                        It.Is<Guid>(r => r == ChangeOfPartyReservationId), It.IsAny<UserInfo>()),
                    Times.Once);
            }

            public void VerifyNullReservation()
            {
                ChangeOfPartyRequest.Verify(x => x.CreateCohort(It.IsAny<Apprenticeship>(),
                    It.Is<Guid?>(r => r == null),
                    It.IsAny<UserInfo>()));
            }

            public void VerifyApprenticeship()
            {
                ChangeOfPartyRequest.Verify(x => x.CreateCohort(It.Is<Apprenticeship>(a => a == Apprenticeship),
                    It.IsAny<Guid>(), It.IsAny<UserInfo>()), Times.Once);
            }

            public void VerifyCohortCreated()
            {
                Assert.Contains(Cohort, Db.Cohorts.ToList());
            }

            public void VerifyCohortReference()
            {
                Assert.AreEqual(CohortReference, Cohort.Reference);
            }
        }
    }
}

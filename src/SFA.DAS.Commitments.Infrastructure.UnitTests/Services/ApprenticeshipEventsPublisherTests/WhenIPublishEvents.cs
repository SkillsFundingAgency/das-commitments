using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Services;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Types;
using ApprenticeshipEvent = SFA.DAS.Events.Api.Types.ApprenticeshipEvent;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Services.ApprenticeshipEventsPublisherTests
{
    [TestFixture]
    public class WhenIPublishEvents
    {
        private ApprenticeshipEventsPublisher _publisher;
        private Mock<IApprenticeshipEventsList> _eventsList;
        private Mock<IEventsApi> _eventsApi;
        private Commitment _commitment;
        private Apprenticeship _apprenticeship;
        private string _event = "Test";
        private Mock<IApprenticeshipEvent> _mockApprenticeshipEvent;

        [SetUp]
        public void Given()
        {
            _eventsList = new Mock<IApprenticeshipEventsList>();
            _eventsApi = new Mock<IEventsApi>();
            _publisher = new ApprenticeshipEventsPublisher(_eventsApi.Object, Mock.Of<ICommitmentsLogger>());

            _commitment = new Commitment
            {
                Id = 348957,
                ProviderId = 123,
                EmployerAccountId = 987,
                LegalEntityId = "LE ID",
                LegalEntityName = "LE Name",
                LegalEntityOrganisationType = SFA.DAS.Common.Domain.Types.OrganisationType.CompaniesHouse
            };

            _apprenticeship = new Apprenticeship
            {
                EndDate = DateTime.Now.AddYears(3),
                StartDate = DateTime.Now.AddDays(1),
                Cost = 123.45m,
                TrainingCode = "TRCODE",
                AgreementStatus = Domain.Entities.AgreementStatus.BothAgreed,
                Id = 34875,
                ULN = "ULN",
                PaymentStatus = Domain.Entities.PaymentStatus.Active,
                TrainingType = TrainingType.Framework,
                PaymentOrder = 213,
                DateOfBirth = DateTime.Now.AddYears(-18)
            };

            _mockApprenticeshipEvent = new Mock<IApprenticeshipEvent>();
            _mockApprenticeshipEvent.SetupGet(x => x.Apprenticeship).Returns(_apprenticeship);
            _mockApprenticeshipEvent.SetupGet(x => x.Commitment).Returns(_commitment);
            _mockApprenticeshipEvent.SetupGet(x => x.Event).Returns(_event);
            var events = new List<IApprenticeshipEvent> { _mockApprenticeshipEvent.Object };

            _eventsList.SetupGet(x => x.Events).Returns(events);
        }

        [Test]
        public async Task AndTheTrainingTypeIsFrameworkThenTheEventIsPublishedWithTheCorrectTrainingType()
        {
            _apprenticeship.TrainingType = TrainingType.Framework;

            await _publisher.Publish(_eventsList.Object);

            VerifyEventWasAdded(_event);
        }

        [Test]
        public async Task AndTheTrainingTypeIsStandardThenTheEventIsAddedWithTheCorrectTrainingType()
        {
            _apprenticeship.TrainingType = TrainingType.Standard;

            await _publisher.Publish(_eventsList.Object);

            VerifyEventWasAdded(_event);
        }

        [Test]
        public async Task AndTheEffectiveFromDateIsProvidedThenTheEventIsAddedWithTheCorrectEffectiveFromDate()
        {
            _apprenticeship.TrainingType = TrainingType.Standard;
            var effectiveFrom = DateTime.Now.AddDays(-1);
            _mockApprenticeshipEvent.SetupGet(x => x.EffectiveFrom).Returns(effectiveFrom);

            await _publisher.Publish(_eventsList.Object);

            VerifyEventWasAdded(_event, effectiveFrom);
        }

        [Test]
        public async Task AndTheEffectiveToDateIsProvidedThenTheEventIsAddedWithTheCorrectEffectiveToDate()
        {
            _apprenticeship.TrainingType = TrainingType.Standard;
            var effectiveTo = DateTime.Now.AddDays(1);
            _mockApprenticeshipEvent.SetupGet(x => x.EffectiveTo).Returns(effectiveTo);

            await _publisher.Publish(_eventsList.Object);

            VerifyEventWasAdded(_event, effectiveTo: effectiveTo);
        }

        [Test]
        public async Task ThenTheEventListIsCleared()
        {
            await _publisher.Publish(_eventsList.Object);

            _eventsList.Verify(x => x.Clear(), Times.Once);
        }

        [Test]
        public async Task AndTheCommitmentIsATransferWhichHasBeenApprovedThenEventMapsCorrectly()
        {
            _commitment.TransferApprovalStatus = TransferApprovalStatus.TransferApproved;

            await _publisher.Publish(_eventsList.Object);

            VerifyEventWasAdded(_event);
        }


        private void VerifyEventWasAdded(string @event, DateTime? effectiveFrom = null, DateTime? effectiveTo = null)
        {
            _eventsApi.Verify(x => x.BulkCreateApprenticeshipEvent(It.Is<IList<ApprenticeshipEvent>>(y => EventMatchesParameters(y, effectiveFrom, effectiveTo))));
        }

        private bool EventMatchesParameters(IList<ApprenticeshipEvent> apprenticeshipEvents, DateTime? effectiveFrom, DateTime? effectiveTo)
        {
            if (apprenticeshipEvents.Count != 1)
            {
                return false;
            }

            var apprenticeshipEvent = apprenticeshipEvents.First();
            return apprenticeshipEvent.AgreementStatus == (Events.Api.Types.AgreementStatus)_apprenticeship.AgreementStatus &&
                   apprenticeshipEvent.ApprenticeshipId == _apprenticeship.Id &&
                   apprenticeshipEvent.EmployerAccountId == _commitment.EmployerAccountId.ToString() &&
                   apprenticeshipEvent.Event == _event &&
                   apprenticeshipEvent.LearnerId == (_apprenticeship.ULN ?? "NULL") &&
                   apprenticeshipEvent.TrainingId == _apprenticeship.TrainingCode &&
                   apprenticeshipEvent.PaymentStatus == (Events.Api.Types.PaymentStatus)_apprenticeship.PaymentStatus &&
                   apprenticeshipEvent.ProviderId == _commitment.ProviderId.ToString() &&
                   apprenticeshipEvent.TrainingEndDate == _apprenticeship.EndDate &&
                   apprenticeshipEvent.TrainingStartDate == _apprenticeship.StartDate &&
                   apprenticeshipEvent.TrainingTotalCost == _apprenticeship.Cost &&
                   apprenticeshipEvent.TrainingType == (_apprenticeship.TrainingType == TrainingType.Framework ? TrainingTypes.Framework : TrainingTypes.Standard) &&
                   apprenticeshipEvent.PaymentOrder == _apprenticeship.PaymentOrder &&
                   apprenticeshipEvent.LegalEntityId == _commitment.LegalEntityId &&
                   apprenticeshipEvent.TransferSenderApproved == (_commitment.TransferApprovalStatus == TransferApprovalStatus.TransferApproved) &&
                   apprenticeshipEvent.LegalEntityName == _commitment.LegalEntityName &&
                   apprenticeshipEvent.LegalEntityOrganisationType == _commitment.LegalEntityOrganisationType.ToString() &&
                   apprenticeshipEvent.DateOfBirth == _apprenticeship.DateOfBirth &&
                   apprenticeshipEvent.EffectiveFrom == effectiveFrom &&
                   apprenticeshipEvent.EffectiveTo == effectiveTo;
        } 
    }
}

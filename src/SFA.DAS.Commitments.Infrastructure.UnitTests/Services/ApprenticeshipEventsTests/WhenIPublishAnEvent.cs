using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Events.Api.Types;
using AgreementStatus = SFA.DAS.Events.Api.Types.AgreementStatus;
using PaymentStatus = SFA.DAS.Events.Api.Types.PaymentStatus;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Services.ApprenticeshipEventsTests
{
    [TestFixture]
    public class WhenIPublishAnEvent : ApprenticeshipEventsTestsBase
    {
        private string _event = "Test";

        [Test]
        public async Task AndTheTrainingTypeIsFrameworkThenTheEventIsPublishedWithTheCorrectTrainingType()
        {
            Apprenticeship.TrainingType = TrainingType.Framework;

            await Service.PublishEvent(Commitment, Apprenticeship, _event);

            VerifyEventWasPublished(_event);
        }

        [Test]
        public async Task AndTheTrainingTypeIsStandardThenTheEventIsPublishedWithTheCorrectTrainingType()
        {
            Apprenticeship.TrainingType = TrainingType.Standard;

            await Service.PublishEvent(Commitment, Apprenticeship, _event);

            VerifyEventWasPublished(_event);
        }

        [Test]
        public async Task AndTheUlnIsNotProvidedThenTheEventIsPublishedWithADefaultValue()
        {
            Apprenticeship.ULN = null;

            await Service.PublishEvent(Commitment, Apprenticeship, _event);

            VerifyEventWasPublished(_event);
        }
        
        private void VerifyEventWasPublished(string @event)
        {
            CommitmentsLogger.Verify(x => x.Info($"Create apprenticeship event: {@event}", null, null, Commitment.Id, Apprenticeship.Id), Times.Once);
            EventsApi.Verify(x => x.CreateApprenticeshipEvent(It.Is<ApprenticeshipEvent>(y => EventMatchesParameters(y, @event))), Times.Once);
        }

        private bool EventMatchesParameters(ApprenticeshipEvent apprenticeshipEvent, string @event)
        {
            return apprenticeshipEvent.AgreementStatus == (AgreementStatus)Apprenticeship.AgreementStatus &&
                   apprenticeshipEvent.ApprenticeshipId == Apprenticeship.Id &&
                   apprenticeshipEvent.EmployerAccountId == Commitment.EmployerAccountId.ToString() &&
                   apprenticeshipEvent.Event == @event &&
                   apprenticeshipEvent.LearnerId == (Apprenticeship.ULN ?? "NULL") &&
                   apprenticeshipEvent.TrainingId == Apprenticeship.TrainingCode &&
                   apprenticeshipEvent.PaymentStatus == (PaymentStatus)Apprenticeship.PaymentStatus &&
                   apprenticeshipEvent.ProviderId == Commitment.ProviderId.ToString() &&
                   apprenticeshipEvent.TrainingEndDate == Apprenticeship.EndDate &&
                   apprenticeshipEvent.TrainingStartDate == Apprenticeship.StartDate &&
                   apprenticeshipEvent.TrainingTotalCost == Apprenticeship.Cost &&
                   apprenticeshipEvent.TrainingType == (Apprenticeship.TrainingType == TrainingType.Framework ? TrainingTypes.Framework : TrainingTypes.Standard) &&
                   apprenticeshipEvent.PaymentOrder == Apprenticeship.PaymentOrder &&
                   apprenticeshipEvent.LegalEntityId == Commitment.LegalEntityId &&
                   apprenticeshipEvent.LegalEntityName == Commitment.LegalEntityName &&
                   apprenticeshipEvent.LegalEntityOrganisationType == Commitment.LegalEntityOrganisationType.ToString();

        }
    }
}

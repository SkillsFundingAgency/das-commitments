using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Services;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Types;
using ApprenticeshipEvent = SFA.DAS.Events.Api.Types.ApprenticeshipEvent;
using System.Linq;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Services.ApprenticeshipEventsTests
{
    public abstract class ApprenticeshipEventsTestsBase
    {
        protected Mock<IEventsApi> EventsApi;
        protected Mock<ICommitmentsLogger> CommitmentsLogger;
        protected ApprenticeshipEvents Service;
        protected Commitment Commitment;
        protected Apprenticeship Apprenticeship;

        [SetUp]
        public void Arrange()
        {
            EventsApi = new Mock<IEventsApi>();
            CommitmentsLogger = new Mock<ICommitmentsLogger>();

            Service = new ApprenticeshipEvents(EventsApi.Object, CommitmentsLogger.Object);

            Commitment = new Commitment
            {
                Id = 348957,
                ProviderId = 123,
                EmployerAccountId = 987,
                LegalEntityId = "LE ID",
                LegalEntityName = "LE Name",
                LegalEntityOrganisationType = SFA.DAS.Common.Domain.Types.OrganisationType.CompaniesHouse,
                AccountLegalEntityPublicHashedId = "ALEPHI"
            };

            Apprenticeship = new Apprenticeship
            {
                EndDate = DateTime.Now.AddYears(3),
                StartDate = DateTime.Now.AddDays(1),
                PauseDate = DateTime.Now.AddMonths(1),
                StopDate = DateTime.Now.AddMonths(2),
                Cost = 123.45m,
                TrainingCode = "TRCODE",
                AgreementStatus = Domain.Entities.AgreementStatus.BothAgreed,
                Id = 34875,
                ULN = "ULN",
                PaymentStatus = Domain.Entities.PaymentStatus.Active,
                TrainingType = TrainingType.Framework,
                PaymentOrder = 213,
                DateOfBirth = DateTime.Now.AddYears(-18),
                PriceHistory = new List<Domain.Entities.PriceHistory> { new Domain.Entities.PriceHistory { ApprenticeshipId = 34875, Cost = 123.45m, FromDate = DateTime.Now.AddDays(1), ToDate = null } }
            };
        }

        protected bool EventMatchesParameters(ApprenticeshipEvent apprenticeshipEvent, string @event, Events.Api.Types.PaymentStatus paymentStatus)
        {
            return apprenticeshipEvent.AgreementStatus == (Events.Api.Types.AgreementStatus)Apprenticeship.AgreementStatus &&
                   apprenticeshipEvent.ApprenticeshipId == Apprenticeship.Id &&
                   apprenticeshipEvent.EmployerAccountId == Commitment.EmployerAccountId.ToString() &&
                   apprenticeshipEvent.Event == @event &&
                   apprenticeshipEvent.LearnerId == (Apprenticeship.ULN ?? "NULL") &&
                   apprenticeshipEvent.TrainingId == Apprenticeship.TrainingCode &&
                   apprenticeshipEvent.PaymentStatus == paymentStatus &&
                   apprenticeshipEvent.ProviderId == Commitment.ProviderId.ToString() &&
                   apprenticeshipEvent.TrainingEndDate == Apprenticeship.EndDate &&
                   apprenticeshipEvent.TrainingStartDate == Apprenticeship.StartDate &&
                   apprenticeshipEvent.TrainingTotalCost == Apprenticeship.Cost &&
                   apprenticeshipEvent.TrainingType == (Apprenticeship.TrainingType == TrainingType.Framework ? TrainingTypes.Framework : TrainingTypes.Standard) &&
                   apprenticeshipEvent.PaymentOrder == Apprenticeship.PaymentOrder &&
                   apprenticeshipEvent.LegalEntityId == Commitment.LegalEntityId &&
                   apprenticeshipEvent.LegalEntityName == Commitment.LegalEntityName &&
                   apprenticeshipEvent.LegalEntityOrganisationType == Commitment.LegalEntityOrganisationType.ToString() &&
                   apprenticeshipEvent.AccountLegalEntityPublicHashedId == Commitment.AccountLegalEntityPublicHashedId &&
                   apprenticeshipEvent.DateOfBirth == Apprenticeship.DateOfBirth &&
                   apprenticeshipEvent.StoppedOnDate == Apprenticeship.StopDate &&
                   apprenticeshipEvent.PausedOnDate == Apprenticeship.PauseDate &&
                   PriceHistoryIsValid(apprenticeshipEvent.PriceHistory);
        }

        private bool PriceHistoryIsValid(IEnumerable<Events.Api.Types.PriceHistory> priceHistory)
        {
            if (Apprenticeship.PriceHistory == null && (priceHistory != null && priceHistory.Count() != 0))
                return false;

            if (Apprenticeship.PriceHistory != null)
            {
                if (priceHistory == null)
                    return false;

                if (Apprenticeship.PriceHistory.Count != priceHistory.Count())
                    return false;
            }

            return true;
        }
    }
}

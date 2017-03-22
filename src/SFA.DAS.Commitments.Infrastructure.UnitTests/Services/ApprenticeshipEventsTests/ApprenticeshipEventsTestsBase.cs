using System;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Services;
using SFA.DAS.Events.Api.Client;

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
                ProviderId = 123,
                EmployerAccountId = 987,
                LegalEntityId = "LE ID",
                LegalEntityName = "LE Name",
                LegalEntityOrganisationType = OrganisationType.CompaniesHouse
            };

            Apprenticeship = new Apprenticeship
            {
                EndDate = DateTime.Now.AddYears(3),
                StartDate = DateTime.Now.AddDays(1),
                Cost = 123.45m,
                TrainingCode = "TRCODE",
                AgreementStatus = AgreementStatus.BothAgreed,
                Id = 34875,
                ULN = "ULN",
                PaymentStatus = PaymentStatus.Active,
                TrainingType = TrainingType.Framework,
                PaymentOrder = 213
            };
        }
    }
}

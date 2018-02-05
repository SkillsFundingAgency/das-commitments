using System;
using Moq;
using NUnit.Framework;
using SFA.DAS.AssessmentOrgs.Api.Client;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Provider.Events.Api.Client;

namespace SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob.UnitTests
{
    [TestFixture]
    public class WhenRunningAddEpaToApprenticeships
    {
        private IAddEpaToApprenticeships _addEpaToApprenticeships;

        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private Mock<IAssessmentOrganisationRepository> _assessmentOrganisationRepository;

        //private Mock<IAssessmentOrgsApiClient> _assessmentOrgsApiClient;
        //private Mock<IPaymentsEventsApiClient> _paymentsEventsApiClient;

        private Mock<IAssessmentOrgs> _assessmentOrgs;
        private Mock<IPaymentEvents> _paymentEvents;

        private Mock<ILog> _log;

        [SetUp]
        public void Arrange()
        {
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _assessmentOrganisationRepository = new Mock<IAssessmentOrganisationRepository>();

            //_assessmentOrgsApiClient = new Mock<IAssessmentOrgsApiClient>();
            //_paymentsEventsApiClient = new Mock<IPaymentsEventsApiClient>();

            _assessmentOrgs = new Mock<IAssessmentOrgs>();
            _paymentEvents = new Mock<IPaymentEvents>();

            _log = new Mock<ILog>();

            _addEpaToApprenticeships = new AddEpaToApprenticeships(
                _log.Object,
                _assessmentOrgs.Object,
                _paymentEvents.Object,
                _assessmentOrganisationRepository.Object,
                _apprenticeshipRepository.Object);
        }

        [Test]
        public void ThenSumfinkOrNuffink()
        {
        }
    }
}

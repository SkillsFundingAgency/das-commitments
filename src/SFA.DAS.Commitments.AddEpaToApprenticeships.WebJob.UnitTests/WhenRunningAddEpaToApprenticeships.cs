using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KellermanSoftware.CompareNetObjects;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Api.Types.AssessmentOrgs;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Provider.Events.Api.Types;

namespace SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob.UnitTests
{
    [TestFixture]
    public class WhenRunningAddEpaToApprenticeships
    {
        private IAddEpaToApprenticeships _addEpaToApprenticeships;

        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private Mock<IAssessmentOrganisationRepository> _assessmentOrganisationRepository;

        private Mock<IAssessmentOrgs> _assessmentOrgs;
        private Mock<IPaymentEvents> _paymentEvents;

        private Mock<ILog> _log;

        [SetUp]
        public void Arrange()
        {
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _assessmentOrganisationRepository = new Mock<IAssessmentOrganisationRepository>();

            _assessmentOrgs = new Mock<IAssessmentOrgs>();
            _paymentEvents = new Mock<IPaymentEvents>();

            _log = new Mock<ILog>();

            _paymentEvents.Setup(x => x.GetSubmissionEvents(
                    It.IsAny<long>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<long>(),
                    It.IsAny<int>()))
                .ReturnsAsync(new PageOfResults<SubmissionEvent>
                {
                    PageNumber = 1,
                    TotalNumberOfPages = 1,
                    Items = new SubmissionEvent[] {}
                });

            _addEpaToApprenticeships = new AddEpaToApprenticeships(
                _log.Object,
                _assessmentOrgs.Object,
                _paymentEvents.Object,
                _assessmentOrganisationRepository.Object,
                _apprenticeshipRepository.Object);
        }

        [Test]
        public async Task ThenAllOrganisationSummariesFromApiAreWrittenToTableWhenTableIsEmpty()
        {
            const string orgId1 = "ORG0001";
            const string orgName1 = "ASM Org";

            var organisationSummaries = new[]
            {
                new OrganisationSummary { Id = orgId1, Name = orgName1 }
            };

            _assessmentOrgs.Setup(x => x.AllAsync()).ReturnsAsync(organisationSummaries);

            _assessmentOrganisationRepository.Setup(x => x.GetLatestEPAOrgIdAsync()).ReturnsAsync((string)null);

            // act
            await _addEpaToApprenticeships.Update();

            // assert
            IEnumerable<AssessmentOrganisation> expectedAssessmentOrganisations = new[]
            {
                new AssessmentOrganisation {EPAOrgId = orgId1, Name = orgName1}
            };

            _assessmentOrganisationRepository.Verify(x => x.AddAsync(It.Is<IEnumerable<AssessmentOrganisation>>(o =>
                new CompareLogic(new ComparisonConfig { IgnoreObjectTypes = true }).Compare(expectedAssessmentOrganisations, o).AreEqual
            )), Times.Once);
        }

        [Test]
        public async Task ThenNewOrganisationSummariesFromApiAreWrittenToTable()
        {
            const string orgId1 = "ORG0001", orgId2 = "ORG0001";
            const string orgName1 = "ASM Org", orgName2 = "B Org";

            var organisationSummaries = new[]
            {
                new OrganisationSummary { Id = orgId1, Name = orgName1 },
                new OrganisationSummary { Id = orgId2, Name = orgName2 },
            };

            _assessmentOrgs.Setup(x => x.AllAsync()).ReturnsAsync(organisationSummaries);

            _assessmentOrganisationRepository.Setup(x => x.GetLatestEPAOrgIdAsync()).ReturnsAsync(orgId1);

            // act
            await _addEpaToApprenticeships.Update();

            // assert
            IEnumerable<AssessmentOrganisation> expectedAssessmentOrganisations = new[]
            {
                new AssessmentOrganisation {EPAOrgId = orgId2, Name = orgName2}
            };

            _assessmentOrganisationRepository.Verify(x => x.AddAsync(It.Is<IEnumerable<AssessmentOrganisation>>(o =>
                new CompareLogic(new ComparisonConfig { IgnoreObjectTypes = true }).Compare(expectedAssessmentOrganisations, o).AreEqual
            )), Times.Once);
        }
    }
}

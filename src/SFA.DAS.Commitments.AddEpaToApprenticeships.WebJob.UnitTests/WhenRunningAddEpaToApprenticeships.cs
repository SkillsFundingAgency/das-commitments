using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KellermanSoftware.CompareNetObjects;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Api.Types;
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
        #region Setup

        private IAddEpaToApprenticeships _addEpaToApprenticeships;

        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private Mock<IAssessmentOrganisationRepository> _assessmentOrganisationRepository;
        private Mock<IJobProgressRepository> _jobProgressRepository;

        private Mock<IAssessmentOrgs> _assessmentOrgs;
        private Mock<IPaymentEvents> _paymentEvents;

        private Mock<ILog> _log;

        private const string EpaOrgId1 = "EPA0001", EpaOrgId2 = "EPA0002";
        private const string EpaOrgName1 = "ASM Org", EpaOrgName2 = "B Org";
        private const long apprenticeshipId1 = 456L, apprenticeshipId2 = 999L, apprenticeshipId3 = 7L, apprenticeshipId4 = 22L;
        private const long unknownApprenticeshipId = 987654L;

        [SetUp]
        public void Arrange()
        {
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _assessmentOrganisationRepository = new Mock<IAssessmentOrganisationRepository>();
            _jobProgressRepository = new Mock<IJobProgressRepository>();

            _assessmentOrgs = new Mock<IAssessmentOrgs>();
            _paymentEvents = new Mock<IPaymentEvents>();

            _log = new Mock<ILog>();

            // default to returning an empty page of submission events
            _paymentEvents.Setup(x => x.GetSubmissionEvents(
                    It.IsAny<long>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<long>(),
                    It.IsAny<int>()))
                .ReturnsAsync(new PageOfResults<SubmissionEvent>
                {
                    PageNumber = 1,
                    TotalNumberOfPages = 0,
                    Items = new SubmissionEvent[] { }
                });

            _addEpaToApprenticeships = new AddEpaToApprenticeships(
                _log.Object,
                _assessmentOrgs.Object,
                _paymentEvents.Object,
                _assessmentOrganisationRepository.Object,
                _apprenticeshipRepository.Object,
                _jobProgressRepository.Object);
        }

        #endregion Setup

        #region Organisation Caching

        [Test]
        public async Task AndAtLeastOneSubmissionEventThenAllOrganisationSummariesFromApiAreWrittenToTableWhenTableIsEmpty()
        {
            SetupOrganisationSummaries();

            const long sinceEventId = 0L;
            SetupSubmissionEventsPageWithSingleEvent(sinceEventId);

            // act
            await _addEpaToApprenticeships.Update();

            // assert
            IEnumerable<AssessmentOrganisation> expectedAssessmentOrganisations = new[]
            {
                new AssessmentOrganisation {EPAOrgId = EpaOrgId1, Name = EpaOrgName1}
            };

            _assessmentOrganisationRepository.Verify(x => x.Add(It.Is<IEnumerable<AssessmentOrganisation>>(o =>
                IEnumerablesAreEqual(expectedAssessmentOrganisations, o))), Times.Once);
        }

        [Test]
        public async Task AndMoreThanOnePageOfSubmissionEventsThenAllOrganisationSummariesFromApiAreWrittenToTableOnlyOnceWhenTableIsEmpty()
        {
            SetupOrganisationSummaries();

            const long sinceEventId = 0L;
            SetupSubmissionEventsTwoCalls(sinceEventId);

            // act
            await _addEpaToApprenticeships.Update();

            // assert
            IEnumerable<AssessmentOrganisation> expectedAssessmentOrganisations = new[]
            {
                new AssessmentOrganisation {EPAOrgId = EpaOrgId1, Name = EpaOrgName1}
            };

            _assessmentOrganisationRepository.Verify(x => x.Add(It.Is<IEnumerable<AssessmentOrganisation>>(o =>
                IEnumerablesAreEqual(expectedAssessmentOrganisations, o))), Times.Once);
        }

        [Test]
        public async Task AndAtLeastOneSubmissionEventThenNewOrganisationSummariesFromApiAreWrittenToTable()
        {
            var organisationSummaries = new[]
            {
                new OrganisationSummary { Id = EpaOrgId1, Name = EpaOrgName1 },
                new OrganisationSummary { Id = EpaOrgId2, Name = EpaOrgName2 },
            };

            _assessmentOrgs.Setup(x => x.All()).ReturnsAsync(organisationSummaries);
            _assessmentOrganisationRepository.Setup(x => x.GetLatestEpaOrgId()).ReturnsAsync(EpaOrgId1);

            const long sinceEventId = 0L;
            SetupSubmissionEventsPageWithSingleEvent(sinceEventId);

            // act
            await _addEpaToApprenticeships.Update();

            // assert
            IEnumerable<AssessmentOrganisation> expectedAssessmentOrganisations = new[]
            {
                new AssessmentOrganisation {EPAOrgId = EpaOrgId2, Name = EpaOrgName2}
            };

            _assessmentOrganisationRepository.Verify(x => x.Add(It.Is<IEnumerable<AssessmentOrganisation>>(o =>
                IEnumerablesAreEqual(expectedAssessmentOrganisations, o))), Times.Once);
        }

        [Test]
        public async Task AndAtLeastOneSubmissionEventThenNoOrganisationSummariesFromApiAreWrittenWhenThereAreNoNewOrgs()
        {
            var organisationSummaries = new[]
            {
                new OrganisationSummary { Id = EpaOrgId1, Name = EpaOrgName1 },
                new OrganisationSummary { Id = EpaOrgId2, Name = EpaOrgName2 },
            };

            _assessmentOrgs.Setup(x => x.All()).ReturnsAsync(organisationSummaries);
            _assessmentOrganisationRepository.Setup(x => x.GetLatestEpaOrgId()).ReturnsAsync(EpaOrgId2);

            const long sinceEventId = 0L;
            SetupSubmissionEventsPageWithSingleEvent(sinceEventId);

            // act
            await _addEpaToApprenticeships.Update();

            // assert
            _assessmentOrganisationRepository.Verify(x => x.Add(It.IsAny<IEnumerable<AssessmentOrganisation>>()), Times.Never);
        }

        #endregion Organisation Caching

        #region Update Apprenticeships

        [Test]
        public async Task ThenApprenticeshipIsUpdatedFromSubmissionEventAndLastSubmissionEventIdIsSet()
        {
            const long sinceEventId = 0L;

            SetupOrganisationSummaries();

            SetupSubmissionEventsPageWithSingleEvent(sinceEventId);

            // act
            await _addEpaToApprenticeships.Update();

            // assert - should probably be seperate tests, but to cut down on test proliferation, we check both
            _apprenticeshipRepository.Verify(x => x.UpdateApprenticeshipEpa(apprenticeshipId1, EpaOrgId1), Times.Once());
            _jobProgressRepository.Verify(x => x.Set_AddEpaToApprenticeships_LastSubmissionEventId(sinceEventId+1), Times.Once);
        }

        /// <summary>
        /// As EPAOrgId is optional in the ilr, we need to make sure we set it to null against the apprenticeship,
        /// if a subsequent irl submission removes it
        /// </summary>
        [Test]
        public async Task AndSubmissionEventContainsNullEpaOrgIdThenApprenticeshipIsUpdatedFromSubmissionEvent()
        {
            const long sinceEventId = 0L;

            SetupOrganisationSummaries();

            SetupSubmissionEventsPageWithSingleEventWithNullEpaOrgId(sinceEventId);

            // act
            await _addEpaToApprenticeships.Update();

            // assert
            _apprenticeshipRepository.Verify(x => x.UpdateApprenticeshipEpa(apprenticeshipId1, null), Times.Once());
        }

        [Test]
        public async Task AndSubmissionEventContainsNullApprenticeshipIdThenSubmissionEventIsIgnored()
        {
            const long sinceEventId = 0L;

            SetupOrganisationSummaries();

            SetupSubmissionEventsPageWithSingleEventWithNullApprenticeshipId(sinceEventId);

            // act
            await _addEpaToApprenticeships.Update();

            // assert
            _apprenticeshipRepository.Verify(x => x.UpdateApprenticeshipEpa(It.IsAny<long>(), It.IsAny<string>()), Times.Never());

            // verify a log message is written with the submission event id that was ignored
            _log.Verify(x => x.Warn(It.Is<string>(message => message.Contains($"{sinceEventId+1}"))), Times.Once);
        }

        [Test]
        public async Task AndAnEmptyPageIsReturnedThenNoApprenticeshipsAreUpdatedAndLastSubmissionEventIdIsNotChanged()
        {
            SetupOrganisationSummaries();

            // act
            await _addEpaToApprenticeships.Update();

            // assert - should probably be seperate tests, but to cut down on test proliferation, we check both
            _apprenticeshipRepository.Verify(x => x.UpdateApprenticeshipEpa(It.IsAny<long>(), It.IsAny<string>()), Times.Never());

            // either of these two verify's would be ok, as it stands we don't update the last submission event id, so we check for that
            //_jobProgressRepository.Verify(x => x.Set_AddEpaToApprenticeships_LastSubmissionEventId(sinceEventId), Times.Once);
            _jobProgressRepository.Verify(x => x.Set_AddEpaToApprenticeships_LastSubmissionEventId(It.IsAny<long>()), Times.Never);
        }

        [Test]
        public async Task AndSubmissionEventContainsUnknownApprenticeshipThenValidApprenticeshipsAreStillUpdatedAndInvalidApprenticeshipIsLogged()
        {
            const long sinceEventId = 0L;

            SetupOrganisationSummaries();

            var submissionEventsPage = new PageOfResults<SubmissionEvent>
            {
                PageNumber = 1,
                TotalNumberOfPages = 1,
                Items = new[]
                {
                    new SubmissionEvent { Id = sinceEventId + 1, ApprenticeshipId = apprenticeshipId1, EPAOrgId = EpaOrgId1 },
                    new SubmissionEvent { Id = sinceEventId + 2, ApprenticeshipId = unknownApprenticeshipId, EPAOrgId = EpaOrgId1 },
                    new SubmissionEvent { Id = sinceEventId + 3, ApprenticeshipId = apprenticeshipId2, EPAOrgId = EpaOrgId2 }
                }
            };

            _paymentEvents.Setup(x => x.GetSubmissionEvents(sinceEventId, null, 0L, 1)).ReturnsAsync(submissionEventsPage);

            _apprenticeshipRepository.Setup(x => x.UpdateApprenticeshipEpa(unknownApprenticeshipId, EpaOrgId1))
                .Throws<ArgumentOutOfRangeException>();

            // act
            await _addEpaToApprenticeships.Update();

            // assert

            // check valid apprenticeships before and after unknown apprenticeship are updated
            _apprenticeshipRepository.Verify(x => x.UpdateApprenticeshipEpa(apprenticeshipId1, EpaOrgId1), Times.Once());
            _apprenticeshipRepository.Verify(x => x.UpdateApprenticeshipEpa(apprenticeshipId2, EpaOrgId2), Times.Once());

            _log.Verify(x => x.Error(It.IsAny<ArgumentOutOfRangeException>(), It.Is<string>(
                message => CheckUnknownApprenticeshipLogMessage(message))), Times.Once);

            _jobProgressRepository.Verify(x => x.Set_AddEpaToApprenticeships_LastSubmissionEventId(sinceEventId+3), Times.Once);
        }

        /// <summary>
        /// We check that a log message has been written that contains "EPAOrgId" and the unknown apprenticeship id.
        /// We don't check for the exact error message as it is now, so as not to make the check brittle to log message changes
        /// </summary>
        private static bool CheckUnknownApprenticeshipLogMessage(string message)
        {
            return message.IndexOf("epaorgid", StringComparison.OrdinalIgnoreCase) >= 0
                   && message.Contains($"{unknownApprenticeshipId}");
        }

        [Test]
        public async Task AndTheSubmissionEventsDontFitInOnePageThenApprenticeshipIsUpdatedFromSubmissionEventAndLastSubmissionEventIdIsSet()
        {
            const long sinceEventId = 0L;

            SetupOrganisationSummaries();

            SetupSubmissionEventsTwoCalls(sinceEventId);

            // act
            await _addEpaToApprenticeships.Update();

            // assert - should probably be seperate tests, but to cut down on test proliferation, we check both
            _apprenticeshipRepository.Verify(x => x.UpdateApprenticeshipEpa(apprenticeshipId1, EpaOrgId1), Times.Once());
            _apprenticeshipRepository.Verify(x => x.UpdateApprenticeshipEpa(apprenticeshipId2, EpaOrgId2), Times.Once());
            _apprenticeshipRepository.Verify(x => x.UpdateApprenticeshipEpa(apprenticeshipId3, EpaOrgId1), Times.Once());
            _apprenticeshipRepository.Verify(x => x.UpdateApprenticeshipEpa(apprenticeshipId4, EpaOrgId2), Times.Once());

            _jobProgressRepository.Verify(x => x.Set_AddEpaToApprenticeships_LastSubmissionEventId(sinceEventId + 4), Times.Once);
        }

        [Test]
        public async Task AndASecondEmptyPageOfSubmissionEventsIsReturnedThenFirstPageIsProcessedAndSecondPageIgnored()
        {
            const long sinceEventId = 0L;

            SetupOrganisationSummaries();

            SetupSubmissionEventsTwoCallsSecondCallReturnsEmptyPage(sinceEventId);

            // act
            await _addEpaToApprenticeships.Update();

            // assert
            _apprenticeshipRepository.Verify(x => x.UpdateApprenticeshipEpa(apprenticeshipId1, EpaOrgId1), Times.Once());
            _apprenticeshipRepository.Verify(x => x.UpdateApprenticeshipEpa(apprenticeshipId2, EpaOrgId2), Times.Once());

            _jobProgressRepository.Verify(x => x.Set_AddEpaToApprenticeships_LastSubmissionEventId(sinceEventId + 2), Times.Once);
        }

        [Test]
        public async Task ThenWeRequestNextSubmissionEventFromPreviousRun()
        {
            const long lastSubmissionEventId = 1024L;

            //todo: what standard setup can we move into setup?
            //todo: split into 2 files, one for unit testing organisation summaries, and one for submission events
            SetupOrganisationSummaries();

            _jobProgressRepository.Setup(x => x.Get_AddEpaToApprenticeships_LastSubmissionEventId()).ReturnsAsync(lastSubmissionEventId);

            SetupSubmissionEventsPageWithSingleEvent(lastSubmissionEventId);

            // act
            await _addEpaToApprenticeships.Update();

            // assert
            _paymentEvents.Verify(x => x.GetSubmissionEvents(lastSubmissionEventId, null, 0L, 1), Times.Once);
        }

        #endregion Update Apprenticeships

        #region Helpers

        private void SetupOrganisationSummaries()
        {
            var organisationSummaries = new[]
            {
                new OrganisationSummary { Id = EpaOrgId1, Name = EpaOrgName1 }
            };

            _assessmentOrgs.Setup(x => x.All()).ReturnsAsync(organisationSummaries);
            _assessmentOrganisationRepository.Setup(x => x.GetLatestEpaOrgId()).ReturnsAsync((string)null);
        }

        private void SetupSubmissionEventsPageWithSingleEvent(long sinceEventId)
        {
            var submissionEventsPage = new PageOfResults<SubmissionEvent>
            {
                PageNumber = 1,
                TotalNumberOfPages = 1,
                Items = new[] { new SubmissionEvent { Id = sinceEventId+1, ApprenticeshipId = apprenticeshipId1, EPAOrgId = EpaOrgId1 } }
            };

            _paymentEvents.Setup(x => x.GetSubmissionEvents(sinceEventId, null, 0L, 1)).ReturnsAsync(submissionEventsPage);
        }

        private void SetupSubmissionEventsPageWithSingleEventWithNullEpaOrgId(long sinceEventId)
        {
            var submissionEventsPage = new PageOfResults<SubmissionEvent>
            {
                PageNumber = 1,
                TotalNumberOfPages = 1,
                Items = new[] { new SubmissionEvent { Id = sinceEventId + 1, ApprenticeshipId = apprenticeshipId1, EPAOrgId = null } }
            };

            _paymentEvents.Setup(x => x.GetSubmissionEvents(sinceEventId, null, 0L, 1)).ReturnsAsync(submissionEventsPage);
        }

        private void SetupSubmissionEventsPageWithSingleEventWithNullApprenticeshipId(long sinceEventId)
        {
            var submissionEventsPage = new PageOfResults<SubmissionEvent>
            {
                PageNumber = 1,
                TotalNumberOfPages = 1,
                Items = new[] { new SubmissionEvent { Id = sinceEventId + 1, ApprenticeshipId = null, EPAOrgId = EpaOrgId1 } }
            };

            _paymentEvents.Setup(x => x.GetSubmissionEvents(sinceEventId, null, 0L, 1)).ReturnsAsync(submissionEventsPage);
        }

        private void SetupSubmissionEventsTwoCalls(long sinceEventId)
        {
            var submissionEventsPageCall1 = new PageOfResults<SubmissionEvent>
            {
                PageNumber = 1,
                TotalNumberOfPages = 2,
                Items = new[] { new SubmissionEvent { Id = sinceEventId+1, ApprenticeshipId = apprenticeshipId1, EPAOrgId = EpaOrgId1 },
                                new SubmissionEvent { Id = sinceEventId+2, ApprenticeshipId = apprenticeshipId2, EPAOrgId = EpaOrgId2 }}
            };

            _paymentEvents.Setup(x => x.GetSubmissionEvents(sinceEventId, null, 0L, 1)).ReturnsAsync(submissionEventsPageCall1);

            var submissionEventsPageCall2 = new PageOfResults<SubmissionEvent>
            {
                PageNumber = 1,
                TotalNumberOfPages = 1,
                Items = new[] { new SubmissionEvent { Id = sinceEventId+3, ApprenticeshipId = apprenticeshipId3, EPAOrgId = EpaOrgId1 },
                                new SubmissionEvent { Id = sinceEventId+4, ApprenticeshipId = apprenticeshipId4, EPAOrgId = EpaOrgId2 }}
            };

            _paymentEvents.Setup(x => x.GetSubmissionEvents(sinceEventId+2, null, 0L, 1)).ReturnsAsync(submissionEventsPageCall2);
        }

        private void SetupSubmissionEventsTwoCallsSecondCallReturnsEmptyPage(long sinceEventId)
        {
            var submissionEventsPageCall1 = new PageOfResults<SubmissionEvent>
            {
                PageNumber = 1,
                TotalNumberOfPages = 2,
                Items = new[] { new SubmissionEvent { Id = sinceEventId+1, ApprenticeshipId = apprenticeshipId1, EPAOrgId = EpaOrgId1 },
                    new SubmissionEvent { Id = sinceEventId+2, ApprenticeshipId = apprenticeshipId2, EPAOrgId = EpaOrgId2 }}
            };

            _paymentEvents.Setup(x => x.GetSubmissionEvents(sinceEventId, null, 0L, 1)).ReturnsAsync(submissionEventsPageCall1);

            var submissionEventsPageCall2 = new PageOfResults<SubmissionEvent>
            {
                PageNumber = 1,
                TotalNumberOfPages = 0,
                Items = new SubmissionEvent[0]
            };

            _paymentEvents.Setup(x => x.GetSubmissionEvents(sinceEventId + 2, null, 0L, 1)).ReturnsAsync(submissionEventsPageCall2);
        }

        private bool IEnumerablesAreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            return new CompareLogic(new ComparisonConfig {IgnoreObjectTypes = true})
                .Compare(expected, actual).AreEqual;
        }

        #endregion Helpers
    }
}

using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Cohort.Creation
{
    [TestFixture]
    public class WhenCohortIsCreated
    {
        private CohortCreationTestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CohortCreationTestFixture();
        }


        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void TheCohortBelongsToTheProvider(Party creatingParty)
        {
            _fixture
                .WithCreatingParty(creatingParty)
                .WithDraftApprenticeship()
                .CreateCohort();

            _fixture.VerifyCohortBelongsToProvider();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void TheCohortBelongsToTheGivenAccount(Party creatingParty)
        {
            _fixture
                .WithCreatingParty(creatingParty)
                .WithDraftApprenticeship()
                .CreateCohort();

            _fixture.VerifyCohortBelongsToAccount();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void TheCohortBelongsToTheGivenAccountLegalEntity(Party creatingParty)
        {
            _fixture
                .WithCreatingParty(creatingParty)
                .WithDraftApprenticeship()
                .CreateCohort();

            _fixture.VerifyCohortBelongsToLegalEntity();
        }

        [TestCase(Party.Provider, Originator.Provider)]
        [TestCase(Party.Employer, Originator.Employer)]
        public void ThenCohortOriginatorIsSetCorrectly(Party creatingParty, Originator expectedOriginator)
        {
            _fixture
                .WithCreatingParty(creatingParty)
                .WithDraftApprenticeship()
                .CreateCohort();

            _fixture.VerifyOriginator(expectedOriginator);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenCohortMustBeWithCreator(Party creatingParty)
        {
            _fixture
                .WithCreatingParty(creatingParty)
                .WithDraftApprenticeship()
                .CreateCohort();

            _fixture.VerifyCohortIsWithCreator();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenCohortCannotBeEmpty(Party creatingParty)
        {
            _fixture
                .WithCreatingParty(creatingParty)
                .CreateCohort();

            _fixture.VerifyException<DomainException>();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenCohortIsUnapproved(Party creatingParty)
        {
            _fixture
                .WithCreatingParty(creatingParty)
                .WithDraftApprenticeship()
                .CreateCohort();

            _fixture.VerifyCohortIsUnapproved();
        }

        [TestCase(Party.Provider, false)]
        [TestCase(Party.Employer, false)]
        [TestCase(Party.TransferSender, true)]
        [TestCase(Party.None, true)]
        public void ThenCreatingPartyMustBeValid(Party creatingParty, bool expectThrows)
        {
            _fixture
                .WithCreatingParty(creatingParty)
                .WithDraftApprenticeship()
                .CreateCohort();

            if (expectThrows) _fixture.VerifyException<DomainException>();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void TheDraftApprenticeshipCreatedEventIsPublished(Party creatingParty)
        {
            _fixture
                .WithCreatingParty(creatingParty)
                .WithDraftApprenticeship()
                .CreateCohort();

            _fixture.VerifyDraftApprenticeshipCreatedEventIsPublished();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenCohortContainsDraftApprenticeship(Party creatingParty)
        {
            _fixture
                .WithCreatingParty(creatingParty)
                .WithDraftApprenticeship()
                .CreateCohort();

            _fixture.VerifyCohortContainsDraftApprenticeship();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenCohortHasLastedUpdatedBy(Party creatingParty)
        {
            _fixture
                .WithCreatingParty(creatingParty)
                .WithDraftApprenticeship()
                .CreateCohort();

            _fixture.VerifyLastUpdatedFieldsAreSetForParty(creatingParty);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenCohortIsADraftIfAssignedToCreator(Party creatingParty)
        {
            _fixture
                .WithCreatingParty(creatingParty)
                .WithDraftApprenticeship()
                .CreateCohort();

            _fixture.VerifyCohortIsDraft();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenNoMessageIsAdded(Party creatingParty)
        {
            _fixture
                .WithCreatingParty(creatingParty)
                .WithDraftApprenticeship()
                .CreateCohort();

            _fixture.VerifyNoMessageIsAdded();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenTheStateChangesAreTracked(Party creatingParty)
        {
            _fixture
                .WithCreatingParty(creatingParty)
                .WithDraftApprenticeship()
                .CreateCohort();

            _fixture.VerifyCohortTracking();
            _fixture.VerifyDraftApprenticeshipTracking();
        }
    }
}

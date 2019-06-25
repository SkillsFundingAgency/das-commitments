using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Cohort.Creation
{
    [TestFixture]
    public class WhenEmployerCreatesCohort
    {
        private CohortCreationTestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CohortCreationTestFixture();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenCohortCanBeWithEitherParty(Party initialParty)
        {
            _fixture
                .WithCreatingParty(Party.Employer)
                .WithInitialParty(initialParty)
                .WithDraftApprenticeship()
                .CreateCohort();

            _fixture.VerifyNoException();
        }
    }
}

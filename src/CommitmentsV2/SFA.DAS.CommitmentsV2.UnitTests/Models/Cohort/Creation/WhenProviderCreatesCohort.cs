using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Cohort.Creation
{
    [TestFixture]
    public class WhenProviderCreatesCohort
    {
        private CohortCreationTestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CohortCreationTestFixture();
        }

        [TestCase(Party.Provider, false)]
        [TestCase(Party.Employer, true)]
        public void ThenCohortMustBeWithProvider(Party initialParty, bool expectThrow)
        {
            _fixture
                .WithCreatingParty(Party.Provider)
                .WithInitialParty(initialParty)
                .WithDraftApprenticeship()
                .CreateCohort();
            
            if(expectThrow) _fixture.VerifyException<DomainException>();
        }

        [Test]
        public void ThenCohortCannotBeEmpty()
        {
            _fixture
                .WithCreatingParty(Party.Provider)
                .CreateCohort();

            _fixture.VerifyException<DomainException>();
        }
    }
}

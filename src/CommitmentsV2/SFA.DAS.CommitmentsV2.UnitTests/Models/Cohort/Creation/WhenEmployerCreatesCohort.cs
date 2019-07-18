using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
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

        [Test]
        public void ThenCohortMustBeEmptyIfInitialPartyIsProvider()
        {
            _fixture
                .WithCreatingParty(Party.Employer)
                .WithInitialParty(Party.Provider)
                .WithDraftApprenticeship()
                .CreateCohort();

            _fixture.VerifyException<DomainException>();
        }

        [Test]
        public void ThenCohortCannotBeEmptyIfInitialPartyIsEmployer()
        {
            _fixture
                .WithCreatingParty(Party.Employer)
                .WithInitialParty(Party.Employer)
                .CreateCohort();

            _fixture.VerifyException<DomainException>();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Cohort.CreationWithOtherParty
{
    [TestFixture]
    public class WhenCreatingCohortWithOtherParty
    {
        private CohortCreationWithOtherPartyTestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CohortCreationWithOtherPartyTestFixture();
        }

        [TestCase(Party.Employer, false)]
        [TestCase(Party.Provider, true)]
        [TestCase(Party.TransferSender, true)]
        public void ThenOnlyTheEmployerCanCreateCohort(Party creatingParty, bool expectThrows)
        {
            _fixture
                .WithCreatingParty(creatingParty)
                .CreateCohort();

            if (expectThrows)
            {
                _fixture.VerifyException<DomainException>();
            }
            else
            {
                _fixture.VerifyNoException();
            }
        }

        [Test]
        public void ThenCohortIsNotADraft()
        {
            _fixture
                .WithCreatingParty(Party.Employer)
                .CreateCohort();

            _fixture.VerifyCohortIsNotDraft();
        }

        [Test]
        public void ThenMessageMustBeSetCorrectly()
        {
            _fixture
                .WithCreatingParty(Party.Employer)
                .WithMessage("test")
                .CreateCohort();

            _fixture.VerifyMessageIsAdded();
        }

        [Test]
        public void ThenCohortMustBeWithOtherParty()
        {
            _fixture
                .WithCreatingParty(Party.Employer)
                .CreateCohort();

            _fixture.VerifyCohortIsWithOtherParty();
        }

        [Test]
        public void ThenTheStateChangesAreTracked()
        {
            _fixture
                .WithCreatingParty(Party.Employer)
                .CreateCohort();

            _fixture.VerifyCohortTracking();
        }
    }
}

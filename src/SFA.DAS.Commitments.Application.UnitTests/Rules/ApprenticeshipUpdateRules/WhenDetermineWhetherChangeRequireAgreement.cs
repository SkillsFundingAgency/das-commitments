using System;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Rules.ApprenticeshipUpdateRules
{
    [TestFixture]
    public class WhenDetermineWhetherChangeRequireAgreement
    {
        private Application.Rules.ApprenticeshipUpdateRules _rules;

        [SetUp]
        public void Setup()
        {
            _rules = new Application.Rules.ApprenticeshipUpdateRules();
        }

        [Test]
        public void ThenSetTrueIfAnySignificantFieldsDiffer()
        {
            var existingApprenticeship = CreateApprenticeship();
            var updatedApprenticeship = CreateApprenticeship();

            existingApprenticeship.StartDate = updatedApprenticeship.StartDate;
            existingApprenticeship.EndDate = updatedApprenticeship.EndDate;

            // both the same initially
            Assert.IsFalse(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));

            updatedApprenticeship.Cost *= 2;
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));
            updatedApprenticeship = CreateApprenticeship();

            updatedApprenticeship.DateOfBirth = DateTime.Now;
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));
            updatedApprenticeship = CreateApprenticeship();

            updatedApprenticeship.FirstName += "X";
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));
            updatedApprenticeship = CreateApprenticeship();

            updatedApprenticeship.LastName += "X";
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));
            updatedApprenticeship = CreateApprenticeship();

            updatedApprenticeship.NINumber += "X";
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));
            updatedApprenticeship = CreateApprenticeship();

            updatedApprenticeship.StartDate = DateTime.Now;
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));
            updatedApprenticeship = CreateApprenticeship();

            updatedApprenticeship.EndDate = DateTime.Now;
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));
            updatedApprenticeship = CreateApprenticeship();

            updatedApprenticeship.TrainingCode += "X";
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));
            updatedApprenticeship = CreateApprenticeship();

            updatedApprenticeship.TrainingType = TrainingType.Standard;
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));

            updatedApprenticeship.TrainingName += "X";
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));
            updatedApprenticeship = CreateApprenticeship();
        }

        [Test]
        public void ThenSetFalseIfOnlyInsignificantFieldsDiffer()
        {
            var existingApprenticeship = CreateApprenticeship();
            var updatedApprenticeship = CreateApprenticeship();

            existingApprenticeship.StartDate = updatedApprenticeship.StartDate;
            existingApprenticeship.EndDate = updatedApprenticeship.EndDate;

            Assert.IsFalse(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));

            updatedApprenticeship.EmployerRef += "X";
            Assert.IsFalse(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));

            updatedApprenticeship.ProviderRef += "X";
            Assert.IsFalse(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));

            updatedApprenticeship.ULN += "X";
            Assert.IsFalse(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));
        }

        private static Apprenticeship CreateApprenticeship()
        {
            return new Apprenticeship
            {
                AgreementStatus = AgreementStatus.NotAgreed,
                PaymentStatus = PaymentStatus.PendingApproval,
                Cost = 1000,
                FirstName = "First name",
                LastName = "Last name",
                NINumber = "NINO",
                ULN = "ULN",
                TrainingType = TrainingType.Framework,
                TrainingName = "TRAINING",
                TrainingCode = "CODE",
                StartDate = DateTime.Now.AddMonths(1),
                EndDate = DateTime.Now.AddMonths(6),
                EmployerRef = "EMPLOYER REF",
                ProviderRef = "PROVIDER REF",
                DateOfBirth = new DateTime(2000,12,30)
            };
        }
    }
}

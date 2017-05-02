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
            var updatedApprenticeship = CreateApiApprenticeship();

            existingApprenticeship.StartDate = updatedApprenticeship.StartDate;
            existingApprenticeship.EndDate = updatedApprenticeship.EndDate;

            // both the same initially
            Assert.IsFalse(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));

            updatedApprenticeship.Cost *= 2;
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));
            updatedApprenticeship = CreateApiApprenticeship();

            updatedApprenticeship.DateOfBirth = DateTime.Now;
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));
            updatedApprenticeship = CreateApiApprenticeship();

            updatedApprenticeship.FirstName += "X";
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));
            updatedApprenticeship = CreateApiApprenticeship();

            updatedApprenticeship.LastName += "X";
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));
            updatedApprenticeship = CreateApiApprenticeship();

            updatedApprenticeship.NINumber += "X";
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));
            updatedApprenticeship = CreateApiApprenticeship();

            updatedApprenticeship.StartDate = DateTime.Now;
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));
            updatedApprenticeship = CreateApiApprenticeship();

            updatedApprenticeship.EndDate = DateTime.Now;
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));
            updatedApprenticeship = CreateApiApprenticeship();

            updatedApprenticeship.TrainingCode += "X";
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));
            updatedApprenticeship = CreateApiApprenticeship();

            updatedApprenticeship.TrainingType = Api.Types.Apprenticeship.Types.TrainingType.Standard;
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));

            updatedApprenticeship.TrainingName += "X";
            Assert.IsTrue(_rules.DetermineWhetherChangeRequiresAgreement(existingApprenticeship, updatedApprenticeship));
            updatedApprenticeship = CreateApiApprenticeship();
        }

        [Test]
        public void ThenSetFalseIfOnlyInsignificantFieldsDiffer()
        {
            var existingApprenticeship = CreateApprenticeship();
            var updatedApprenticeship = CreateApiApprenticeship();

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

        private static Api.Types.Apprenticeship.Apprenticeship CreateApiApprenticeship()
        {
            return new Api.Types.Apprenticeship.Apprenticeship
            {
                AgreementStatus = Api.Types.AgreementStatus.NotAgreed,
                PaymentStatus = Api.Types.Apprenticeship.Types.PaymentStatus.PendingApproval,
                Cost = 1000,
                FirstName = "First name",
                LastName = "Last name",
                NINumber = "NINO",
                ULN = "ULN",
                TrainingType = Api.Types.Apprenticeship.Types.TrainingType.Framework,
                TrainingName = "TRAINING",
                TrainingCode = "CODE",
                StartDate = DateTime.Now.AddMonths(1),
                EndDate = DateTime.Now.AddMonths(6),
                EmployerRef = "EMPLOYER REF",
                ProviderRef = "PROVIDER REF",
                DateOfBirth = new DateTime(2000, 12, 30)
            };
        }
    }
}

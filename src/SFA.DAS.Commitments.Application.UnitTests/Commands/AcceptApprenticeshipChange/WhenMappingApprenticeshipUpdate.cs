using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.AcceptApprenticeshipChange;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.AcceptApprenticeshipChange
{
    [TestFixture]
    public class WhenMappingApprenticeshipUpdate
    {
        private AcceptApprenticeshipUpdateMapper _sut;
        private Apprenticeship _apprenticeship;

        [SetUp]
        public void SetUp()
        {
            _sut = new AcceptApprenticeshipUpdateMapper();
            _apprenticeship = new Apprenticeship
                {
                    Id = 55,
                    EmployerAccountId = 555,
                    ProviderId = 666,
                    FirstName = "Original First name",
                    LastName = "Original Last name",
                    DateOfBirth = new DateTime(1998, 12, 8),
                    ULN = "1112223301",
                    TrainingCode = "original-code",
                    TrainingName = "Original name",
                    TrainingType = TrainingType.Framework,
                    StartDate = new DateTime(2020, 12, 01),
                    EndDate = new DateTime(2021, 12, 01),
                    Cost = new decimal(1600),
                    ProviderRef = "Provider ref",
                    EmployerRef = "Employer ref",

                    CommitmentId = 11,
                    PaymentStatus = PaymentStatus.Withdrawn,
                    AgreementStatus = AgreementStatus.ProviderAgreed,
                    CreatedOn = new DateTime(2006, 1, 1),
                    AgreedOn = new DateTime(2006, 5, 5),
                    PaymentOrder = 666,
                    UpdateOriginator = Originator.Provider,
                    ProviderName = "Provider name",
                    LegalEntityName = "Legal entity name",
                    DataLockCourse = true
            };
        }

        private void EnsureNoOtherChanges()
        {
            _apprenticeship.Id.Should().Be(55);
            _apprenticeship.EmployerAccountId.Should().Be(555);
            _apprenticeship.ProviderId.Should().Be(666);
            _apprenticeship.ULN.Should().Be("1112223301");
            _apprenticeship.ProviderRef.Should().Be("Provider ref");
            _apprenticeship.EmployerRef.Should().Be("Employer ref");

            _apprenticeship.CommitmentId.Should().Be(11);
            _apprenticeship.PaymentStatus.Should().Be(PaymentStatus.Withdrawn);
            _apprenticeship.AgreementStatus.Should().Be(AgreementStatus.ProviderAgreed);
            _apprenticeship.CreatedOn.Should().Be(new DateTime(2006, 1, 1));
            _apprenticeship.AgreedOn.Should().Be(new DateTime(2006, 5, 5));
            _apprenticeship.PaymentOrder.Should().Be(666);
            _apprenticeship.UpdateOriginator.Should().Be(Originator.Provider);
            _apprenticeship.ProviderName.Should().Be("Provider name");
            _apprenticeship.LegalEntityName.Should().Be("Legal entity name");
            _apprenticeship.TrainingType.Should().Be(TrainingType.Framework);
            _apprenticeship.DataLockCourse.Should().Be(true);
        }

        [Test]
        public void EmptyUpdate()
        {
            var update = new ApprenticeshipUpdate();
            _sut.ApplyUpdate(_apprenticeship, update);

            _apprenticeship.FirstName.Should().Be("Original First name");
            _apprenticeship.LastName.Should().Be("Original Last name");
            _apprenticeship.DateOfBirth.Should().Be(new DateTime(1998, 12, 8));

            EnsureNoOtherChanges();
        }

        [Test]
        public void NameUpdate()
        {
            var update = new ApprenticeshipUpdate {FirstName = "New First name", LastName = "New Last name"};
            _sut.ApplyUpdate(_apprenticeship, update);

            _apprenticeship.FirstName.Should().Be("New First name");
            _apprenticeship.LastName.Should().Be("New Last name");
            _apprenticeship.DateOfBirth.Should().Be(new DateTime(1998, 12, 8));
            EnsureNoOtherChanges();
        }

        [Test]
        public void UpdateAllFields()
        {
            var dob = DateTime.Now.AddYears(19);
            var startDate = DateTime.Now.AddYears(2);
            var endDate = DateTime.Now.AddYears(4);
            var update = new ApprenticeshipUpdate
            {
                FirstName = "New First name",
                LastName = "New Last name",
                DateOfBirth = dob,
                TrainingType = TrainingType.Framework,
                TrainingCode = "training-code",
                TrainingName = "Training name",
                Cost = 1500,
                StartDate = startDate,
                EndDate = endDate
            };
            _sut.ApplyUpdate(_apprenticeship, update);

            _apprenticeship.FirstName.Should().Be("New First name");
            _apprenticeship.LastName.Should().Be("New Last name");
            _apprenticeship.DateOfBirth.Should().Be(dob);

            _apprenticeship.TrainingType.Should().Be(TrainingType.Framework);
            _apprenticeship.TrainingCode.Should().Be("training-code");
            _apprenticeship.TrainingName.Should().Be("Training name");

            _apprenticeship.StartDate.Should().Be(startDate);
            _apprenticeship.EndDate.Should().Be(endDate);

            _apprenticeship.Cost.Should().Be(1500);

            EnsureNoOtherChanges();
        }
    }
}

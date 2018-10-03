using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.AcceptApprenticeshipChange;
using SFA.DAS.Commitments.Domain.Entities;
using System.Linq;
using System.Collections.Generic;
using Castle.Components.DictionaryAdapter;
using Moq;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.AcceptApprenticeshipChange
{
    [TestFixture]
    public class WhenMappingApprenticeshipUpdate
    {
        private AcceptApprenticeshipChangeMapper _sut;
        private Apprenticeship _apprenticeship;

        private Mock<ICurrentDateTime> _currentTime;

        private int _yearNow;

        [SetUp]
        public void SetUp()
        {
            _yearNow = DateTime.Now.Year;
            _currentTime = new Mock<ICurrentDateTime>();
            _sut = new AcceptApprenticeshipChangeMapper(_currentTime.Object);
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
                    DataLocks = new List<DataLockStatusSummary>
                    {
                        new DataLockStatusSummary{ ErrorCode = DataLockErrorCode.Dlock04 }
                    },
                    PriceHistory = new List<PriceHistory>
                    {
                        new PriceHistory { FromDate = new DateTime(2020, 12, 01), Cost = 1234, ApprenticeshipId = 55 }
                    }
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
            _apprenticeship.DataLocks.Should().Contain(x => x.ErrorCode == DataLockErrorCode.Dlock04);
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

        [Test]
        public void UpdateCostWhenSinglePriceHistory()
        {
            var update = new ApprenticeshipUpdate
            {
                Cost = 32333
            };

            _sut.ApplyUpdate(_apprenticeship, update);


            _apprenticeship.PriceHistory.Single().Cost.Should().Be(32333);
        }

        [Test]
        public void ShouldNotAllowChangesToCostWhenMoreThanOnePriceHistory()
        {
            _currentTime.Setup(m => m.Now).Returns(new DateTime(_yearNow, 05, 23));
            _apprenticeship.PriceHistory = new List<PriceHistory>
                    {
                        new PriceHistory { FromDate = new DateTime(_yearNow, 01, 01), ToDate = new DateTime(_yearNow, 03, 31), Cost = 1199, ApprenticeshipId = 55 },
                        new PriceHistory { FromDate = new DateTime(_yearNow, 04, 01), ToDate = new DateTime(_yearNow, 06, 30), Cost = 2299, ApprenticeshipId = 55 },
                        new PriceHistory { FromDate = new DateTime(_yearNow, 07, 01), Cost = 3399, ApprenticeshipId = 55 }
                    };

            var update = new ApprenticeshipUpdate
            {
                Cost = 32333
            };

            Action act = () => _sut.ApplyUpdate(_apprenticeship, update);
            act.ShouldThrow<InvalidOperationException>().Which.Message.Should().Be("Multiple Prices History Items not expected.");
        }
        
        [Test]
        public void ShouldNotAllowChangesToStartDateWhenMoreThanOnePriceHistory()
        {
            _currentTime.Setup(m => m.Now).Returns(new DateTime(_yearNow, 05, 23));
            _apprenticeship.PriceHistory = new List<PriceHistory>
                    {
                        new PriceHistory { FromDate = new DateTime(_yearNow, 01, 01), ToDate = new DateTime(_yearNow, 03, 31), Cost = 1199, ApprenticeshipId = 55 },
                        new PriceHistory { FromDate = new DateTime(_yearNow, 04, 01), ToDate = new DateTime(_yearNow, 06, 30), Cost = 2299, ApprenticeshipId = 55 },
                        new PriceHistory { FromDate = new DateTime(_yearNow, 07, 01), Cost = 3399, ApprenticeshipId = 55 }
                    };

            var update = new ApprenticeshipUpdate
            {
                Cost = 32333,
                StartDate = new DateTime(_yearNow, 08, 01)
            };

            Action act = () => _sut.ApplyUpdate(_apprenticeship, update);
            act.ShouldThrow<InvalidOperationException>().Which.Message.Should().Be("Multiple Prices History Items not expected.");
        }

        [Test]
        public void ShouldUpdateStartDateOnPriceHistory()
        {
            _currentTime.Setup(m => m.Now).Returns(new DateTime(_yearNow, 05, 23));
            _apprenticeship.PriceHistory = new List<PriceHistory>
                    {
                        new PriceHistory { FromDate = new DateTime(_yearNow, 01, 01), Cost = 3399, ApprenticeshipId = 55 }
                    };

            var newStartDate = new DateTime(_yearNow, 08, 01);
            var update = new ApprenticeshipUpdate
            {
                Cost = 32333,
                StartDate = newStartDate
            };

            _sut.ApplyUpdate(_apprenticeship, update);
            _apprenticeship.PriceHistory[0].Cost.ShouldBeEquivalentTo(32333);
            _apprenticeship.PriceHistory[0].FromDate.ToString("O").ShouldBeEquivalentTo(newStartDate.ToString("O"));
        }
    }
}

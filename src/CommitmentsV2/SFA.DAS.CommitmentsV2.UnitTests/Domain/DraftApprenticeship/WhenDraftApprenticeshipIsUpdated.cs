using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Domain.DraftApprenticeship
{
    [TestFixture]
    public class WhenDraftApprenticeshipIsUpdated
    {
        private DraftApprenticeshipUpdateTestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new DraftApprenticeshipUpdateTestFixture();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenFirstNameIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.That(result.FirstName, Is.EqualTo(_fixture.DraftApprenticeshipDetails.FirstName));
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenLastNameIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.That(result.LastName, Is.EqualTo(_fixture.DraftApprenticeshipDetails.LastName));
        }

        [Test]
        public void ThenUlnIsMappedCorrectly()
        {
            var result = _fixture.WithModifyingParty(Party.Provider).WithUlnUpdateOnly().ApplyUpdate();
            Assert.That(result.Uln, Is.EqualTo(_fixture.DraftApprenticeshipDetails.Uln));
        }

        [Test]
        public void ThenEmployerUlnUpdateIsNotAllowed()
        {
            Assert.Throws<DomainException>(() => _fixture.WithModifyingParty(Party.Employer).WithUlnUpdateOnly().ApplyUpdate());
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenCostIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.That(result.Cost, Is.EqualTo(_fixture.DraftApprenticeshipDetails.Cost));
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenStartDateIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.That(result.StartDate, Is.EqualTo(_fixture.DraftApprenticeshipDetails.StartDate));
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenActualStartDateIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.That(result.ActualStartDate, Is.EqualTo(_fixture.DraftApprenticeshipDetails.ActualStartDate));
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenEndDateIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.That(result.EndDate, Is.EqualTo(_fixture.DraftApprenticeshipDetails.EndDate));
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenEmploymentEndDateIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.That(result.FlexibleEmployment, Is.Not.Null);
            Assert.That(result.FlexibleEmployment.EmploymentEndDate, Is.EqualTo(_fixture.DraftApprenticeshipDetails.EmploymentEndDate));
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenEmploymentPriceIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.That(result.FlexibleEmployment, Is.Not.Null);
            Assert.That(result.FlexibleEmployment.EmploymentPrice, Is.EqualTo(_fixture.DraftApprenticeshipDetails.EmploymentPrice));
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenDateOfBirthIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.That(result.DateOfBirth, Is.EqualTo(_fixture.DraftApprenticeshipDetails.DateOfBirth));
        }


        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenProgrammeTypeIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.That(result.ProgrammeType, Is.EqualTo(_fixture.DraftApprenticeshipDetails.TrainingProgramme.ProgrammeType));
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenCourseCodeIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.That(result.CourseCode, Is.EqualTo(_fixture.DraftApprenticeshipDetails.TrainingProgramme.CourseCode));
        }


        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenCourseNameIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.That(result.CourseName, Is.EqualTo(_fixture.DraftApprenticeshipDetails.TrainingProgramme.Name));
        }

        [Test]
        public void ThenProviderReferenceIsMappedCorrectly()
        {
            var result = _fixture.WithModifyingParty(Party.Provider).ApplyUpdate();
            Assert.That(result.ProviderRef, Is.EqualTo(_fixture.DraftApprenticeshipDetails.Reference));
        }

        [Test]
        public void ThenEmployerReferenceIsMappedCorrectly()
        {
            var result = _fixture.WithModifyingParty(Party.Employer).ApplyUpdate();
            Assert.That(result.EmployerRef, Is.EqualTo(_fixture.DraftApprenticeshipDetails.Reference));
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void And_StandardUIdIsNotUpdated_Then_SelectedOptionIsResetToNull(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).WithReferenceUpdateOnly().ApplyUpdate();
            Assert.That(result.TrainingCourseOption, Is.EqualTo(_fixture.DraftApprenticeshipDetails.TrainingCourseOption));
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void And_NewStandardOrStandardVersionIsSelected_Then_SelectedOptionIsResetToNull(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).WithNewStandardUId().ApplyUpdate();
            Assert.That(result.TrainingCourseOption, Is.Null);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenRegularDeliveryModelIsMappedCorrectly(Party modifyingParty)
        {
            var flexibleEmployment = new Fixture().Create<FlexibleEmployment>();
            var result = _fixture.WithModifyingParty(modifyingParty).WithDeliveryModel(DeliveryModel.Regular, flexibleEmployment).ApplyUpdate(flexibleEmployment);
            Assert.Multiple(() =>
            {
                Assert.That(result.FlexibleEmployment.EmploymentEndDate, Is.Null);
                Assert.That(result.FlexibleEmployment.EmploymentPrice, Is.Null);
            });
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenFlexiDeliveryModelIsMappedCorrectly(Party modifyingParty)
        {
            var flexibleEmployment = new Fixture().Create<FlexibleEmployment>();
            var result = _fixture.WithModifyingParty(modifyingParty).WithDeliveryModel(DeliveryModel.PortableFlexiJob, flexibleEmployment).ApplyUpdate(flexibleEmployment);
            Assert.Multiple(() =>
            {
                Assert.That(flexibleEmployment.EmploymentEndDate, Is.EqualTo(result.FlexibleEmployment.EmploymentEndDate));
                Assert.That(flexibleEmployment.EmploymentPrice, Is.EqualTo(result.FlexibleEmployment.EmploymentPrice));
            });
        }

        private class DraftApprenticeshipUpdateTestFixture
        {
            private readonly Fixture _autoFixture;
            private readonly CommitmentsV2.Models.DraftApprenticeship _draftApprenticeship;
            public DraftApprenticeshipDetails DraftApprenticeshipDetails { get; private set; }
            private CommitmentsV2.Models.DraftApprenticeship _result;
            private Party _modifyingParty;

            public DraftApprenticeshipUpdateTestFixture()
            {
                _autoFixture = new Fixture();
                _draftApprenticeship = new CommitmentsV2.Models.DraftApprenticeship(_autoFixture.Build<DraftApprenticeshipDetails>().Without(o=>o.Uln).Create(), Party.Provider);
                DraftApprenticeshipDetails = _autoFixture.Build<DraftApprenticeshipDetails>().Without(o => o.Uln).Create();
            }

            public DraftApprenticeshipUpdateTestFixture WithUlnUpdateOnly()
            {
                DraftApprenticeshipDetails = CreateUpdateFromOriginal();
                DraftApprenticeshipDetails.Uln = _autoFixture.Create<string>();
                return this;
            }

            public DraftApprenticeshipUpdateTestFixture WithReferenceUpdateOnly()
            {
                DraftApprenticeshipDetails = CreateUpdateFromOriginal();
                DraftApprenticeshipDetails.Reference = _autoFixture.Create<string>();
                return this;
            }

            public DraftApprenticeshipUpdateTestFixture WithTrainingCourseResetOnly()
            {
                DraftApprenticeshipDetails = CreateUpdateFromOriginal();
                DraftApprenticeshipDetails.TrainingProgramme = null;
                return this;
            }

            public DraftApprenticeshipUpdateTestFixture WithNewStandardUId()
            {
                DraftApprenticeshipDetails = CreateUpdateFromOriginal();
                DraftApprenticeshipDetails.StandardUId = _autoFixture.Create<string>(); ;
                return this;
            }

            public DraftApprenticeshipUpdateTestFixture WithDeliveryModel(DeliveryModel deliveryModel, FlexibleEmployment flexibleEmployment)
            {
                DraftApprenticeshipDetails = CreateUpdateFromOriginal();
                DraftApprenticeshipDetails.DeliveryModel = deliveryModel;
                DraftApprenticeshipDetails.EmploymentEndDate = flexibleEmployment.EmploymentEndDate;
                DraftApprenticeshipDetails.EmploymentPrice = flexibleEmployment.EmploymentPrice;
                return this;
            }          

            public DraftApprenticeshipUpdateTestFixture WithModifyingParty(Party party)
            {
                _modifyingParty = party;
                return this;
            }
            
            public CommitmentsV2.Models.DraftApprenticeship ApplyUpdate()
            {
                _result  = TestHelper.Clone(_draftApprenticeship);
                _result.Merge(DraftApprenticeshipDetails, _modifyingParty);
                return _result;
            }

            public CommitmentsV2.Models.DraftApprenticeship ApplyUpdate(FlexibleEmployment flexibleEmployment)
            {
                _result  = TestHelper.Clone(_draftApprenticeship);
                
                _result.FlexibleEmployment = flexibleEmployment;
                _result.Merge(DraftApprenticeshipDetails, _modifyingParty);
                return _result;
            }

            private DraftApprenticeshipDetails CreateUpdateFromOriginal()
            {
                return new DraftApprenticeshipDetails
                {
                    Uln = _draftApprenticeship.Uln,
                    FirstName = _draftApprenticeship.FirstName,
                    LastName = _draftApprenticeship.LastName,
                    Cost = (int?)_draftApprenticeship.Cost,
                    TrainingProgramme = new CommitmentsV2.Domain.Entities.TrainingProgramme(
                        _draftApprenticeship.CourseCode,
                        _draftApprenticeship.CourseName,
                        _draftApprenticeship.ProgrammeType.Value,
                        null,
                        null),
                    StandardUId = _draftApprenticeship.StandardUId,
                    TrainingCourseOption = _draftApprenticeship.TrainingCourseOption,
                    DateOfBirth = _draftApprenticeship.DateOfBirth,
                    StartDate = _draftApprenticeship.StartDate,
                    EndDate = _draftApprenticeship.EndDate,
                    Id = _draftApprenticeship.Id,
                    Reference = _modifyingParty == Party.Employer ? _draftApprenticeship.EmployerRef : _draftApprenticeship.ProviderRef
                };
            }
        }

    }
}
